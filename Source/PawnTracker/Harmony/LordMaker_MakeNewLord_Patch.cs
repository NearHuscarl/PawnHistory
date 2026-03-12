using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Harmony;

[HarmonyPatch(typeof(LordMaker), nameof(LordMaker.MakeNewLord))]
public static class LordMaker_MakeNewLord_Patch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var SetJob = AccessTools.Method(typeof(Lord), nameof(Lord.SetJob));
        var hookMethod = AccessTools.Method(typeof(LordMaker_MakeNewLord_Patch), nameof(OnLordJobStart));

        foreach (var code in instructions)
        {
            yield return code;

            if (code.Calls(SetJob))
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
        GameEventListener.Publish(new LordToilChangeEvent(null, lord.Graph.StartingToil, null, lord));
    }
}