using HarmonyLib;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(RitualOutcomeEffectWorker_FromQuality), nameof(RitualOutcomeEffectWorker_FromQuality.GetOutcome))]
[HarmonyPriority(Priority.First)]
internal class ForcedRitualOutcome
{
    private static void Postfix(ref RitualOutcomePossibility __result)
    {
        if (TestManager.Scenario.ForcedRitualOutcome == null)
            return;

        __result = TestManager.Scenario.ForcedRitualOutcome;
    }
}
