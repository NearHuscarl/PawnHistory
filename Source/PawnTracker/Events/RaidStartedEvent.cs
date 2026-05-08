using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RaidStartedEvent(List<Pawn> Pawns, Faction Faction, RaidStrategyDef RaidStrategy, PawnsArrivalModeDef RaidArrivalMode, bool IsFriendly, Quest Quest = null) : GameEventBase;

file static class RaidStartedContext
{
    internal static Quest GetQuest(IncidentParms parms)
    {
        // QuestPart_SurpriseReinforcement doesn't define incidentParms.quest
        if (parms.customLetterLabel == "LetterLabelSurpriseReinforcements".TranslateSimple() && parms.target is Map map && QuestHelper.TryGetRelatedQuestFrom(map.Parent, out var quest))
            return quest;
        return parms.quest;
    }
}

[HarmonyPatch(typeof(IncidentWorker_Raid), nameof(IncidentWorker_Raid.TryGenerateRaidInfo))]
internal static class IncidentWorker_Raid_TryGenerateRaidInfo_Patch
{
    private static void Postfix(bool __result, IncidentWorker_Raid __instance, IncidentParms parms, List<Pawn> pawns, bool debugTest = false)
    {
        if (!__result)
            return; // cannot spawn a raid due to internal error

        var quest = RaidStartedContext.GetQuest(parms);
        var isFriendly = __instance is IncidentWorker_RaidFriendly;
        GameEventBus.Publish(new RaidStartedEvent(pawns, parms.faction, parms.raidStrategy, parms.raidArrivalMode, isFriendly, quest));
    }
}
