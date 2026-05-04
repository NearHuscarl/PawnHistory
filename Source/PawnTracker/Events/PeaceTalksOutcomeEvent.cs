using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum PeaceTalksOutcome
{
    Backfire,
    TalksFlounder,
    Success,
    Triumph,
    Disaster,
}

public record PeaceTalksOutcomeEvent(Pawn Negotiator, Faction Faction, PeaceTalksOutcome Outcome, List<Pawn> Enemies) : GameEventBase;

[HarmonyPatch(typeof(PeaceTalks), "Outcome_Backfire")]
internal static class PeaceTalks_Outcome_Backfire_Patch
{
    private static void Postfix(PeaceTalks __instance, Caravan caravan)
    {
        var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
        GameEventBus.Publish(new PeaceTalksOutcomeEvent(negotiator, __instance.Faction, PeaceTalksOutcome.Backfire, []));
    }
}

[HarmonyPatch(typeof(PeaceTalks), "Outcome_TalksFlounder")]
internal static class PeaceTalks_Outcome_TalksFlounder_Patch
{
    private static void Postfix(PeaceTalks __instance, Caravan caravan)
    {
        var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
        GameEventBus.Publish(new PeaceTalksOutcomeEvent(negotiator, __instance.Faction, PeaceTalksOutcome.TalksFlounder, []));
    }
}

[HarmonyPatch(typeof(PeaceTalks), "Outcome_Success")]
internal static class PeaceTalks_Outcome_Success_Patch
{
    private static void Postfix(PeaceTalks __instance, Caravan caravan)
    {
        var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
        GameEventBus.Publish(new PeaceTalksOutcomeEvent(negotiator, __instance.Faction, PeaceTalksOutcome.Success, []));
    }
}

[HarmonyPatch(typeof(PeaceTalks), "Outcome_Triumph")]
internal static class PeaceTalks_Outcome_Triumph_Patch
{
    private static void Postfix(PeaceTalks __instance, Caravan caravan)
    {
        var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
        GameEventBus.Publish(new PeaceTalksOutcomeEvent(negotiator, __instance.Faction, PeaceTalksOutcome.Triumph, []));
    }
}

internal static class PeaceTalksOutcomeContext
{
    internal static Pawn Negotiator;
}

[HarmonyPatch(typeof(PeaceTalks), "Outcome_Disaster")]
internal static class PeaceTalks_Outcome_Disaster_Patch
{
    private static void Prefix(Caravan caravan)
    {
        PeaceTalksOutcomeContext.Negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
    }
}

[HarmonyPatch(typeof(CaravanIncidentUtility), nameof(CaravanIncidentUtility.SetupCaravanAttackMap))]
internal static class CaravanIncidentUtility_SetupCaravanAttackMap_Patch
{
    private static void Postfix(Caravan caravan, List<Pawn> enemies, Map __result)
    {
        if (__result == null || caravan == null)
            return;

        var negotiator = PeaceTalksOutcomeContext.Negotiator;
        if (negotiator == null)
            return;

        GameEventBus.Publish(new PeaceTalksOutcomeEvent(negotiator, enemies.FirstOrDefault()?.Faction, PeaceTalksOutcome.Disaster, enemies));
        PeaceTalksOutcomeContext.Negotiator = null;
    }
}
