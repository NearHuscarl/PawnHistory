using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch]
[HarmonyPriority(Priority.First)]
internal class ForcedRitualOutcome
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        const string methodName = nameof(RitualOutcomeEffectWorker_FromQuality.GetOutcome);

        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_FromQuality), methodName);
        yield return AccessTools.Method(typeof(RitualOutcomeEffectWorker_Trial), methodName);
    }

    private static void Postfix(ref RitualOutcomePossibility __result)
    {
        if (TestManager.Scenario.ForcedRitualOutcome == null)
            return;

        __result = TestManager.Scenario.ForcedRitualOutcome;
    }
}

[HarmonyPatch(typeof(RitualOutcomeComp_RoleChangeParticipants), nameof(RitualOutcomeComp_RoleChangeParticipants.QualityOffset))]
internal class ForcedRitualOutcome_RoleChange
{
    private static void Postfix(ref float __result, LordJob_Ritual ritual)
    {
        if (TestManager.Scenario.ForcedRitualOutcome == null)
            return;

        __result = TestManager.Scenario.ForcedRitualOutcome.BestPositiveOutcome(ritual) ? 1f : 0f;
    }
}
