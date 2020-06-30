using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using System.Reflection.Emit; // for OpCodes in Harmony Transpiler
using HarmonyLib;

namespace RestrictedStorage {
    [HarmonyPatch(typeof(RimWorld.Building_Storage), "Notify_ReceivedThing")]
    public static class Patch_NotifyReceivedThing {
        static void Postfix(Building_Storage __instance
                            #if DEBUG
                            , Thing newItem
                            #endif
            ) {
            #if DEBUG
            Log.Message(""+__instance+" recieved thing "+newItem);
            #endif
            __instance.GetComp<CompRestrictedStorage>()?.CheckForIncorrectItems();
        }
    }
    // This causes a crash-to-desktop, as Notify_LostThing is an empty virtual
    // that gets optimized away:
    // We have to find a different way to check for not having incorrect items...
    #if false
    [HarmonyPatch(typeof(RimWorld.Building_Storage), "Notify_LostThing")]
    public static class Patch_NotifyLostThing {
        static void Postfix(Building_Storage __instance
                            #if DEBUG
                            , Thing newItem // might be null?
                            #endif
            ) {
            #if DEBUG
            Log.Message(""+__instance+" lost thing "+newItem);
            #endif
            __instance.GetComp<CompRestrictedStorage>()?.CheckForIncorrectItems();
        }
    }
    #endif

}
