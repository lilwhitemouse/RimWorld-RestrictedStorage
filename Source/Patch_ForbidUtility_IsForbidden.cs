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
    // In RimWorld.ForbidUtility's bool IsForbidden(Thing t, Pawn pawn),
    //   there are several tests for whether an item is forbidden
    // After the 1st few tests, we insert our test right before the last:
    // if (Patch_IsForbidden.IsInRestrictedStorage(pawn, t) //<----insert this test
    //     { return true; }
    // Lord lord = pawn.GetLord ();
    // if (lord != null && lord.extraForbiddenThings.Contains (t)) {
    //    return true;
    // }
[HarmonyPatch(typeof(RimWorld.ForbidUtility), "IsForbidden", new Type[] {typeof(Thing), typeof(Pawn)})]
    public static class Patch_IsForbidden {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
                                                       ILGenerator generator) {
            List<CodeInstruction> code=instructions.ToList();
            for (int i=0; i<code.Count; i++) {
                // Insert our test right before lord=pawn.GetLord();
                if (code[i].opcode==OpCodes.Call &&
                    (MethodInfo)code[i].operand==typeof(Verse.AI.Group.LordUtility)
                                       .GetMethod("GetLord", BindingFlags.Static | BindingFlags.Public)) {
                    // A Ldarg_1 was just called, so the Pawn is on the stack (to call GetLord)
                    // Also put the Thing t on the stack:
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // Thing t
                    // now call IsInRestrictedStorage(pawn,thing):
                    yield return new CodeInstruction(OpCodes.Call, typeof(Patch_IsForbidden).GetMethod("IsInRestrictedStorage",
                                                                      BindingFlags.Static | BindingFlags.NonPublic));
                    // if (IsInRestrictedStorage(t)) return true;
                    //  --->  branch false to jump past this:
                    Label continueWithLordTest=generator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Brfalse, continueWithLordTest);
                    //   return true (if it was in restricted storage):
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Ret);

                    // Done with everything, here is where we jump to to continue with the next test,
                    //   pawn.GetLord(); //etc
                    // We need to again put the Pawn pawn on the stack (and also give it the label to jump to):
                    var c=new CodeInstruction(OpCodes.Ldarg_1);
                    c.labels.Add(continueWithLordTest);
                    yield return c;
                }
                yield return code[i];
            }
        }
        // Note: it's Pawn p, Thing t and not the more natural Thing t, Pawn p because
        //   we're working with Transpiler here...
        static bool IsInRestrictedStorage(Pawn p, Thing t) {
            if (t==null) return false; // just making sure
            if (!t.Spawned) return false;
            // Check everything for null all at once:
            //   if any of those are NULL, then it's not true, so it's not in restricted storage!
            if ( (t.Position.GetSlotGroup(t.Map)?.parent as ThingWithComps)?.GetComp<CompRestrictedStorage>()?.IsForbidden(p)==true) {
                return true;
            }
            return false;
        }
    } // end Patch_IsForbidden
}
