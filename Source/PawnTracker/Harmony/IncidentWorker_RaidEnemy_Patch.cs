using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

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
        var eventDef = PawnEventDefOf.Raid;

        GameEventListener.Publish(new GroupEvent(pawns, pawns[0].Faction, eventDef, (pawn) =>
        {
            var request = new GrammarRequest();
            var threat = otherCount == 1
                ? $"{otherCount} other"
                : $"{otherCount} others";

            request.Includes.Add(eventDef.rulePackDef);
            request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));
            request.Rules.Add(new Rule_String("THREAT", threat.ApplyTag(TagType.Threat).Resolve()));
            request.Rules.Add(new Rule_String("FACTION", pawns[0].Faction.NameColored.Resolve()));
            request.Constants.Add("otherCount", otherCount.ToString());

            return GrammarResolver.Resolve("raid", request);
        }));
    }
}
