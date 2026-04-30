using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record DefenderGeneratedEvent(List<Pawn> Pawns, WorldObject WorldObject, Quest Quest) : GameEventBase;

[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
internal static class MapGenerator_GenerateMap_DefenderGenerated_Patch
{
    private static void Postfix(Map __result, MapParent parent)
    {
        if (parent is not WorldObject worldObject)
            return;
        
        // generated in GenStep_* e.g. GenStep_SettlementPawnsLoot
        var pawns = __result.mapPawns.AllPawnsSpawned
            .Where(p => p.HostileTo(Faction.OfPlayer) && p.HistoryRecords.Count == 0 && p.Spawned)
            .ToList();

        if (pawns.Count == 0)
            return;

        QuestHelper.TryGetRelatedQuestFrom(worldObject, out var quest);
        GameEventBus.Publish(new DefenderGeneratedEvent(pawns, worldObject, quest));
    }
}
