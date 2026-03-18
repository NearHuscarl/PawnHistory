using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Events;

[HarmonyPatch(typeof(Transition), nameof(Transition.CheckSignal))]
public static class Transition_CheckSignal_Patch
{
    // Insert OnTransitionChange() right before changing lordToil to get the correct trigger type safely
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var executeMethod = AccessTools.Method(typeof(Transition), nameof(Transition.Execute));
        var hookMethod = AccessTools.Method(typeof(Transition_CheckSignal_Patch), nameof(OnTransitionChange));

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Calls(executeMethod))
            {
                // insert before Execute(lord)
                yield return new CodeInstruction(OpCodes.Ldarg_0); // __instance
                yield return new CodeInstruction(OpCodes.Ldloc_1); // index1
                yield return new CodeInstruction(OpCodes.Ldarg_1); // lord
                yield return new CodeInstruction(OpCodes.Call, hookMethod);
            }

            yield return list[i];
        }
    }

    public static void OnTransitionChange(Transition __instance, int index1, Lord lord)
    {
        var trigger = __instance.triggers[index1];
        var currentToil = lord.CurLordToil;
        var nextToil = __instance.target;

        if (currentToil == nextToil)
            return;

        GameEventBus.Publish(new LordToilChangeEvent(currentToil, nextToil, trigger, lord));
    }
}

