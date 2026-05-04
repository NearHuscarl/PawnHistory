using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Events;

public record LordToilChangeEvent(LordToil CurrentToil, LordToil NextToil, Trigger Trigger, Lord Lord, TriggerSignal? Signal = null) : GameEventBase;

[HarmonyPatch(typeof(LordMaker), nameof(LordMaker.MakeNewLord))]
public static class LordMaker_MakeNewLord_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var setJob = AccessTools.Method(typeof(Lord), nameof(Lord.SetJob));
        var hookMethod = AccessTools.Method(typeof(LordMaker_MakeNewLord_Patch), nameof(OnLordJobStart));

        foreach (var code in instructions)
        {
            yield return code;

            if (code.Calls(setJob))
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0); // newLord
                yield return new CodeInstruction(OpCodes.Call, hookMethod);
            }
        }
    }

    // Run only after Lord is assigned a LordJob

    public static void OnLordJobStart(Lord __instance)
    {
        var lord = __instance;
        GameEventBus.Publish(new LordToilChangeEvent(null, lord.Graph.StartingToil, null, lord));
    }
}

[HarmonyPatch(typeof(Transition), nameof(Transition.CheckSignal))]
public static class Transition_CheckSignal_Patch
{
    // Insert OnTransitionChange() right before changing lordToil to get the correct trigger type safely
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var executeMethod = AccessTools.Method(typeof(Transition), nameof(Transition.Execute));
        var hookMethod = AccessTools.Method(typeof(Transition_CheckSignal_Patch), nameof(OnTransitionChange));

        foreach (var t in list)
        {
            if (t.Calls(executeMethod))
            {
                // insert before Execute(lord)
                yield return new CodeInstruction(OpCodes.Ldarg_0); // __instance
                yield return new CodeInstruction(OpCodes.Ldloc_0); // index1
                yield return new CodeInstruction(OpCodes.Ldarg_1); // lord
                yield return new CodeInstruction(OpCodes.Ldarg_2); // signal (TriggerSignal)
                yield return new CodeInstruction(OpCodes.Call, hookMethod);
            }

            yield return t;
        }
    }

    public static void OnTransitionChange(Transition __instance, int index1, Lord lord, TriggerSignal signal)
    {
        var trigger = __instance.triggers[index1];
        var currentToil = lord.CurLordToil;
        var nextToil = __instance.target;

        if (currentToil == nextToil)
            return;

        GameEventBus.Publish(new LordToilChangeEvent(currentToil, nextToil, trigger, lord, signal));
    }
}
