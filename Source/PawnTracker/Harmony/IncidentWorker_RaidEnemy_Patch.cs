using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "GenerateRaidLoot")]
public static class IncidentWorker_RaidEnemy_Patch
{
    public static void Prefix(IncidentParms parms, float raidLootPoints, List<Pawn> pawns)
    {
        pawns = [.. pawns.Where(PawnTracker.ShouldTrack)];
        if (!pawns.Any())
            return;

        var otherCount = pawns.Count - 1;
        var other = otherCount > 1 ? "others" : "other";
        var eventDef = pawns.Count > 1 ? PawnEventDefOf.Raid : PawnEventDefOf.RaidSingle;

        GameEventListener.Publish(new GroupEvent(pawns, pawns[0].Faction, eventDef, (pawn) =>
        {
            return eventDef.description.Formatted(
                pawn.NameShortColored.Named("PAWN"),
                $"{otherCount} {other}".ApplyTag(TagType.Threat).Named("OTHERRAIDER"),
                pawns[0].Faction.NameColored.Named("FACTION")
            );
        }));
    }
}
