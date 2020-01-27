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

namespace RestrictedStorage
{
  [StaticConstructorOnStartup]
  public class RestrictedStorage
  {
    static RestrictedStorage() {
      var harmony = Harmony.HarmonyInstance.Create("net.littlewhitemouse.RimWorld.RestrictedStorage");
      harmony.PatchAll();
      // Add ITab and Comp to proper storage buildings:
      //   Add to all Building_Storage but not ones in Production (hoppers?)
      // This should be slightly faster than xpath xml patching.
      var desigProduction=DefDatabase<DesignationCategoryDef>.GetNamed("Production");
      var itabResolved=InspectTabManager.GetSharedInstance(typeof(ITab_RestrictedStorage));
      foreach (var b in DefDatabase<ThingDef>.AllDefs
               .Where(d=>(d?.thingClass!=null &&
                          (d.thingClass == typeof(Building_Storage) ||
                           d.thingClass.IsSubclassOf(typeof(Building_Storage)))))
               .Where(d=>d.designationCategory!=desigProduction)) {
          // This should be the equivalent of
          //   <comps>
          //     <li>
          //       <compClass>CompRestrictedStorage</compClass>etc
          b.comps.Add(new CompProperties {compClass=typeof(CompRestrictedStorage)});
          // This mirrors ThingDef's resolve references - I didn't want to take the time
          //   to do a ResolveReferences for every single ThingDef, but if anything
          //   breaks, that's always an option...
          b.inspectorTabs.Add(typeof(ITab_RestrictedStorage));
          b.inspectorTabsResolved.Add(itabResolved);
      }
    }
  }

    internal class Debug
    {
        [Conditional("DEBUG")]
        internal static void Log(string s)
        {
            Verse.Log.Message(s);
        }

        [Conditional("DEBUG")]
        internal static void Warning(string s)
        {
            Verse.Log.Warning(s);
        }

        [Conditional("DEBUG")]
        internal static void Error(string s)
        {
            Verse.Log.Error(s);
        }
    }
}
