using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(SlaveRebellionUtility), "DecideSlaveRebellionType")]
internal static class ForceSlaveRebellionType
{
    private static readonly Type SlaveRebellionType = AccessTools.Inner(typeof(SlaveRebellionUtility), "SlaveRebellionType");

    private static void Postfix(ref object __result)
    {
        if (TestManager.Scenario.ForceSlaveRebellionType != null)
            __result = Enum.Parse(SlaveRebellionType, TestManager.Scenario.ForceSlaveRebellionType.ToString());
    }
}

[HarmonyPatch(
    typeof(SlaveRebellionUtility),
    nameof(SlaveRebellionUtility.StartSlaveRebellion),
    [typeof(Pawn), typeof(string), typeof(string), typeof(LetterDef), typeof(LookTargets), typeof(bool)],
    [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal]
)]
internal static class ForceSlaveRebellionViolent
{
    private static void Prefix(ref bool forceAggressive)
    {
        if (TestManager.Scenario.ForceSlaveRebellionType != null)
            forceAggressive = TestManager.Scenario.ForceSlaveRebellionViolent;
    }
}
