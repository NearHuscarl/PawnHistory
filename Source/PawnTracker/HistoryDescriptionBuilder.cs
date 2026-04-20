using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker;

public class HistoryDescriptionBuilder(HistoryRecordDef recordDef, Pawn pawn, string keyword = null)
{
    public HistoryRecordDef HistoryRecordDef { get; } = recordDef;
    public Pawn Pawn { get; } = pawn;
    public string Keyword { get; } = keyword ?? "PAWN";

    private bool includePawnRules;
    private readonly List<Rule> extraRules = [];
    private readonly Dictionary<string, string> extraConstants = [];
    private readonly Dictionary<string, object> namedArgs = [];

    public HistoryDescriptionBuilder IncludePawnGrammar(bool include = true)
    {
        includePawnRules = include;
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, string value, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (value == null) return this;

        if (HistoryRecordDef.descriptionMaker != null)
        {
            if (replaceIfExist)
                extraRules.RemoveAll(r => r.keyword.StartsWith(keyword));
            if (addSubsymbols)
                extraRules.AddRange(LangUtility.RulesForString(keyword, value));

            extraRules.Add(new Rule_String(keyword, value));
        }
        else
            namedArgs[keyword] = value;
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, TaggedString value, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        return AddRule(keyword, value.Resolve(), addSubsymbols, replaceIfExist);
    }

    public HistoryDescriptionBuilder AddRule(string keyword, int value, bool replaceIfExist = false)
    {
        return AddRule(keyword, value.ToString(), replaceIfExist);
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Pawn pawn, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (pawn == null) return this;

        AddRule(keyword, pawn.NameDef, replaceIfExist);

        if (addSubsymbols)
            return AddRules(GrammarUtility.RulesForPawn(keyword, pawn, extraConstants));
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Faction faction, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (faction == null) return this;
        
        AddRule(keyword, faction.NameColored.Resolve(), replaceIfExist);
        if (addSubsymbols)
            return AddRules(GrammarUtility.RulesForFaction(keyword, faction, extraConstants));
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Def def, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (def == null) return this;

        AddRule(keyword, def.label, replaceIfExist);

        if (addSubsymbols)
            return AddRules(GrammarUtility.RulesForDef(keyword, def));
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, ThingDef thingDef, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (thingDef == null) return this;

        AddRule(keyword, thingDef.label.Colorize(thingDef.graphicData.colorTwo), addSubsymbols, replaceIfExist);

        if (addSubsymbols)
            return AddRules(GrammarUtility.RulesForDef(keyword, thingDef));
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Hediff hediff, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (hediff == null) return this;
        return AddRule(keyword, hediff.LabelBase.ToLower().Colorize(hediff.LabelColor), addSubsymbols, replaceIfExist);
    }

    public HistoryDescriptionBuilder AddRule(string keyword, HediffDef hediffDef, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (hediffDef == null) return this;
        return AddRule(keyword, hediffDef.LabelColored(), addSubsymbols, replaceIfExist);
    }

    public HistoryDescriptionBuilder AddRule(string keyword, BodyPartRecord part, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (part == null) return this;
        // Label = left middle toe
        // LabelShort = toe
        return AddRule(keyword, part.Label, addSubsymbols, replaceIfExist);
    }

    public HistoryDescriptionBuilder AddRule(string keyword, RoyalTitleDef royalTitle, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (royalTitle == null) return this;
        return AddRule(keyword, royalTitle.GetLabelCapFor(Pawn).Colorize(Faction.OfEmpire.Color), addSubsymbols, replaceIfExist);
    }

    public HistoryDescriptionBuilder AddRules(IEnumerable<Rule> rules)
    {
        extraRules.AddRange(rules);
        return this;
    }

    public HistoryDescriptionBuilder AddConstant(string key, object value)
    {
        extraConstants[key] = value.ToString();
        return this;
    }

    public string Format()
    {
        if (HistoryRecordDef.description == null)
        {
            Log.Error($"PawnEventDef '{HistoryRecordDef.defName}' does not have description defined in either rulePackDef or description.");
            return "ERR: No description found";
        }

        List<NamedArgument> args = [Pawn.NameDef.Named(Keyword)];

        foreach (var kvp in namedArgs)
            args.Add(kvp.Value.Named(kvp.Key));

        return HistoryRecordDef.description.Formatted(args).Resolve();
    }

    public string Resolve(string rootKeyword = "entry")
    {
        if (rootKeyword == null)
        {
            Log.Error($"Error when resolving '{HistoryRecordDef.defName}' description: RootKeyword is null.");
            return "ERR: RootKeyword=null";
        }

        var request = new GrammarRequest();

        request.Includes.Add(HistoryRecordDef.descriptionMaker);
        request.Rules.Add(new Rule_String(Keyword, Pawn.NameDef));

        if (includePawnRules)
            request.Rules.AddRange(GrammarUtility.RulesForPawn(Keyword, Pawn, extraConstants));

        request.Rules.AddRange(extraRules);
        request.Constants.AddRange(extraConstants);

        return GrammarResolver.Resolve(rootKeyword, request);
    }
}

public static class HistoryDescriptionBuilderExtensions
{
    private static readonly RulePackDef VarRulePack = DefLookup.RulePack.PH_Var;
    public static string GetOtherText(string ruleName, int otherCount)
    {
        var request = new GrammarRequest();

        request.Includes.Add(VarRulePack);
        request.Rules.Add(new Rule_String("OtherCount", otherCount.ToString()));
        request.Constants.Add("OtherCount", otherCount.ToString());

        return GrammarResolver.Resolve(ruleName, request);
    }

    extension(HistoryDescriptionBuilder builder)
    {
        public HistoryDescriptionBuilder WithOthers(List<Pawn> pawns)
        {
            var otherCount = pawns.Count - 1;
            var isThreat = pawns[0].InMentalState == true && pawns[0].MentalStateDef == MentalStateDefOf.Manhunter || (pawns[0]?.Faction.HostileTo(Faction.OfPlayer) ?? false);
            var otherTag = isThreat ? TagType.Threat : TagType.ColonistCount;
            var otherText = GetOtherText("Others", otherCount).ApplyTag(otherTag).Resolve();

            return builder.AddRule("Others", otherText)
                .AddConstant("OtherCount", otherCount);
        }

        public HistoryDescriptionBuilder WithPlayerFaction()
        {
            return builder.AddRule("R_PlayerFaction", Faction.OfPlayer)
                .AddConstant("playerFactionHasName", Faction.OfPlayer.HasName);
        }
        
        public HistoryDescriptionBuilder WithPlayerSettlement(WorldObject worldObject)
        {
            if (worldObject is not Settlement settlement || settlement.Faction != Faction.OfPlayer)
                return builder;

            return builder.AddRule("R_PlayerSettlement", settlement.ColoredLabel)
                .AddConstant("playerSettlementHasName", settlement.namedByPlayer);
        }
    }
}
