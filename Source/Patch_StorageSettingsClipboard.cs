using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using HarmonyLib;

namespace RestrictedStorage
{
    [HarmonyPatch(typeof(StorageSettingsClipboard), "Copy")]
    static class Patch_StorageSettingsClipboard_Copy {
        static void Postfix(StorageSettings s) {
            crs=null; // clear old one
            Building_Storage building;
            if ((building=(s.owner as Building_Storage))!=null) {
                CompRestrictedStorage origComp=building.GetComp<CompRestrictedStorage>();
                if (origComp==null || origComp.IsDefault()) return;
                crs=new CompRestrictedStorage();
                crs.CopyAllowancesFrom(origComp);
                //Log.Warning("Copying restrictions from "+building);
            }
        }
        public static CompRestrictedStorage crs=null;
    }
    [HarmonyPatch(typeof(StorageSettingsClipboard), "PasteInto")]
    static class Patch_StorageSettingsClipboard_PasteInto {
        static void Postfix(StorageSettings s) {
            Building_Storage building;
            if ((building=(s.owner as Building_Storage))!=null) {
                var oldcrs=building.GetComp<CompRestrictedStorage>();
                if (oldcrs!=null) building.AllComps.Remove(oldcrs);
                // would like to not bother with comp if is defaults, but...
                //if (Patch_StorageSettingsClipboard_Copy.crs!=null) {
                CompRestrictedStorage newcrs=new CompRestrictedStorage();
                newcrs.parent=building;
                newcrs.Initialize(null);
                building.AllComps.Add(newcrs);
                if (Patch_StorageSettingsClipboard_Copy.crs!=null)
                    newcrs.CopyAllowancesFrom(Patch_StorageSettingsClipboard_Copy.crs);
                //Log.Message("Copying restictions to "+building);
                //}
            }
        }
    }
}
