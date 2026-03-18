using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class RaidStartedEvent(List<Pawn> pawns, Faction faction, RaidStrategyDef raidStrategy, PawnsArrivalModeDef raidArrivalMode, bool isFriendly) : GameEventBase
{
    public List<Pawn> Pawns { get; } = pawns;
    public Faction Faction { get; } = faction;
    public RaidStrategyDef RaidStrategy { get; } = raidStrategy;
    public PawnsArrivalModeDef RaidArrivalMode { get; } = raidArrivalMode;
    public bool IsFriendly { get; } = isFriendly;
}

[HarmonyPatch(typeof(IncidentWorker_Raid), nameof(IncidentWorker_Raid.TryGenerateRaidInfo))]
public static class IncidentWorker_Raid_TryGenerateRaidInfo_Patch
{
    public static void Postfix(bool __result, IncidentWorker_Raid __instance, IncidentParms parms, List<Pawn> pawns, bool debugTest = false)
    {
        if (!__result)
            return; // cannot spawn a raid due to internal error

        GameEventBus.Publish(new RaidStartedEvent(pawns, parms.faction, parms.raidStrategy, parms.raidArrivalMode, isFriendly: __instance is IncidentWorker_RaidFriendly));
    }
}
