using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RaidStartedEvent(List<Pawn> Pawns, Faction Faction, RaidStrategyDef RaidStrategy, PawnsArrivalModeDef RaidArrivalMode, bool IsFriendly, Quest Quest = null) : GameEventBase;

[HarmonyPatch(typeof(IncidentWorker_Raid), nameof(IncidentWorker_Raid.TryGenerateRaidInfo))]
internal static class IncidentWorker_Raid_TryGenerateRaidInfo_Patch
{
    private static void Postfix(bool __result, IncidentWorker_Raid __instance, IncidentParms parms, List<Pawn> pawns, bool debugTest = false)
    {
        if (!__result)
            return; // cannot spawn a raid due to internal error

        GameEventBus.Publish(new RaidStartedEvent(pawns, parms.faction, parms.raidStrategy, parms.raidArrivalMode, IsFriendly: __instance is IncidentWorker_RaidFriendly, parms.quest));
    }
}
