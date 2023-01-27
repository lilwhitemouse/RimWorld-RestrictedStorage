using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using System.Diagnostics;
using HarmonyLib;

namespace LWM.RestrictedStorage
{
    [StaticConstructorOnStartup]
    public class RestrictedStorage
    {
        public static bool mechsAreActive = false;
        /* RE: Mechs:
         * <) What the Hack - they just add an hediff to mechs, so should be easy to work with!
         * Thier packageId is roolo.whatthehack
         * <) VFEM   -> definitely use mechanoid flesh, so .RaceProps.IsMechanoid will work!
         * Their packageId is OskarPotocki.VFE.Mechanoid        
         * and of course, Biotech (mechanator. mechinator?)        
         * check pawn.RaceProps.IsMechanoid
         */

        static RestrictedStorage()
        {
            var harmony = new HarmonyLib.Harmony("net.littlewhitemouse.RimWorld.RestrictedStorage");
            harmony.PatchAll();
            // Add ITab and Comp to proper storage buildings:
            //   Add to all Building_Storage but not ones in Production (hoppers?)
            // This should be slightly faster than xpath xml patching.
            // Also slightly safer given some of the crazy **** I do in DS
            var desigProduction = DefDatabase<DesignationCategoryDef>.GetNamed("Production");
            var itabResolved = InspectTabManager.GetSharedInstance(typeof(ITab_RestrictedStorage));
            #if DEBUG
            List<string> newRestricted = new List<string>();
            #endif
            foreach (var b in DefDatabase<ThingDef>.AllDefs
                     .Where(d => (d?.thingClass != null &&
                                (d.thingClass == typeof(Building_Storage) ||
                                 d.thingClass.IsSubclassOf(typeof(Building_Storage)))))
                     .Where(d => d.designationCategory != desigProduction))
            {
                // This should be the equivalent of
                //   <comps>
                //     <li>
                //       <compClass>CompRestrictedStorage</compClass>etc
                if (b.comps != null)
                {
                    b.comps.Add(new CompProperties { compClass = typeof(CompRestrictedStorage) });
                    #if DEBUG
                    newRestricted.Add(b.defName);
                    #endif
                }
                else
                {
                    Log.Message("LWM.Restricted Storage: " + b + " does not have comps");
                    continue;
                }
                // but....we don't actually want to add this comp to EVERYTHING - I mean, why
                // bother?  It's not going to be used in the majority of cases.  Except...
                // the game won't load save-game-data unless the comp is already there.  Yes...
                // So we do add it to everything.
                //
                // We DO want to use the ITab for all storage buildings:
                // This mirrors ThingDef's resolve references - I didn't want to take the time
                //   to do a ResolveReferences for every single ThingDef, but if anything
                //   breaks, that's always an option...
                if (b.inspectorTabs != null)
                {
                    b.inspectorTabs.Add(typeof(ITab_RestrictedStorage));
                    if (b.inspectorTabsResolved != null)
                    {
                        b.inspectorTabsResolved.Add(itabResolved);
                    }
                    else
                    {
                        Log.Message("LWM.Restricted Storage: " + b + " does not have inspectorTabsResolved");
                    }
                }
                else
                {
                    Log.Message("LWM.Restricted Storage: " + b + " does not have inspectorTabs");
                }
                // Note: above seems to work just fine
            } // done with storage buildings

            /********************* Mechs! *********************/
            // We only want to display options for mechs if the player can - in whatever way - actually
            //   have mechs carrying stuff about the base. So far the options I know about are:
            //   * Biotech - mechanator. mechinator?
            //   * VFEM - packageId is OskarPotocki.VFE.Mechanoid
            //   * What the Hack - packageId is roolo.whatthehack
            // NOTE: We will test for mechaniods by checking .RaceProps.IsMechanoid
            //    (which checks what kind of flesh it has, basically)
            //    This is obviously fine for Biotech, and WTH just adds an hediff to the standard
            //    mechs. And someone checked for me: VFEM uses their own classes, but they do have
            //    standard mechanoid flesh. Skin. Whatever. So the test will work!
            //  
            if (ModsConfig.BiotechActive || ModLister.GetActiveModWithIdentifier("OskarPotocki.VFE.Mechanoid") != null
                || ModLister.GetActiveModWithIdentifier("roolo.whatthehack") != null)
            {
                mechsAreActive = true;
            }
            #if DEBUG
            Debug.Log(newRestricted.Count == 0 ? "No Storage to restrict!! :(" : String.Join(", ", newRestricted));
            #endif
        }
    }

    internal class Debug
    {
        [Conditional("DEBUG")]
        internal static void Log(string s)
        {
            Verse.Log.Message("LWM.RestrictedStorage: " + s);
        }

        [Conditional("DEBUG")]
        internal static void Warning(string s)
        {
            Verse.Log.Warning("LWM.RestrictedStorage: " + s);
        }

        [Conditional("DEBUG")]
        internal static void Error(string s)
        {
            Verse.Log.Error("LWM.RestrictedStorage: " + s);
        }
    }
}
