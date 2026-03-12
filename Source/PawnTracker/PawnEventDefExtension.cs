using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker;

public class DescriptionParams(string rootKeyword, Pawn pawn, Faction faction = null)
{
    public string RootKeyword { get; } = rootKeyword;
    public Pawn Pawn { get; } = pawn;
    public Faction Faction { get; } = faction;
    public List<Rule> ExtraRules { get; set; } = [];
    public Dictionary<string, string> ExtraConstants { get; set; } = [];
    public List<Pawn> RelatedPawns { get; set; }
    public bool AddRulesForPawn { get; set; }
}

public static class PawnEventDefExtension
{
    public static string ResolveDescription(this PawnEventDef eventDef, DescriptionParams descParams)
    {
        var rootKeyword = descParams.RootKeyword;

        if (eventDef.rulePackDef == null)
        {
            Log.Error($"PawnEventDef '{eventDef.defName}' has null rulePackDef while resolving '{rootKeyword}'.");
            return eventDef.description ?? eventDef.defName;
        }

        var pawn = descParams.Pawn;
        var faction = descParams.Faction;
        var relatedPawns = descParams.RelatedPawns;
        var request = new GrammarRequest();

        request.Includes.Add(eventDef.rulePackDef);
        request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));

        if (descParams.AddRulesForPawn)
            request.Rules.AddRange(GrammarUtility.RulesForPawn("PAWN", pawn));

        if (faction != null)
            request.Rules.Add(new Rule_String("FACTION", faction.NameColored.Resolve()));

        if (relatedPawns != null)
        {
            var otherCount = relatedPawns.Count - 1;
            var otherTag = Faction.OfPlayer.HostileTo(relatedPawns[0]?.Faction) ? TagType.Threat : TagType.ColonistCount;
            var otherText = otherCount switch
            {
                0 => "",
                1 => $" and {(otherCount + " other").ApplyTag(otherTag).Resolve()}",
                _ => $" and {(otherCount + " others").ApplyTag(otherTag).Resolve()}",
            };
            request.Rules.Add(new Rule_String("OTHERS", otherText));
        }
        request.Rules.AddRange(descParams.ExtraRules ?? []);
        request.Constants.AddRange(descParams.ExtraConstants ?? []);

        return GrammarResolver.Resolve(rootKeyword, request);
    }
}
