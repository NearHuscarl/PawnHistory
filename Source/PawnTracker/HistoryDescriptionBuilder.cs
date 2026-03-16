using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker;

public class HistoryDescriptionBuilder(PawnEventDef eventDef, string rootKeyword, Pawn pawn)
{
    public PawnEventDef EventDef { get; } = eventDef;
    public string RootKeyword { get; } = rootKeyword;
    public Pawn Pawn { get; } = pawn;

    private bool includePawnRules;
    private readonly List<Rule> extraRules = [];
    private readonly Dictionary<string, string> extraConstants = [];

    public HistoryDescriptionBuilder IncludePawnGrammar(bool include = true)
    {
        includePawnRules = include;
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, string value)
    {
        if (value == null) return this;
        extraRules.Add(new Rule_String(keyword, value));
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, TaggedString value)
    {
        if (value == null) return this;
        extraRules.Add(new Rule_String(keyword, value.Resolve()));
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Pawn pawn)
    {
        if (pawn == null) return this;
        return AddRule(keyword, pawn.NameShortColored.Resolve());
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Faction faction)
    {
        if (faction == null) return this;
        return AddRule(keyword, faction.NameColored.Resolve());
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Hediff hediff)
    {
        if (hediff == null) return this;
        return AddRule(keyword, hediff.LabelBase.ToLower().Colorize(hediff.LabelColor));
    }

    public HistoryDescriptionBuilder AddRule(string keyword, HediffDef hediffDef)
    {
        if (hediffDef == null) return this;
        return AddRule(keyword, hediffDef.label.Colorize(hediffDef.defaultLabelColor));
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Hediff hediff, BodyPartRecord bodyPart)
    {
        if (bodyPart == null)
            return AddRule(keyword, hediff);

        return AddRule(keyword, hediff.def.PrettyTextForPart(bodyPart));
    }

    public HistoryDescriptionBuilder AddRuleIf(bool condition, string keyword, object value)
    {
        if (!condition || value == null) return this;

        return value switch
        {
            string v => AddRule(keyword, v),
            Pawn v => AddRule(keyword, v),
            Faction v => AddRule(keyword, v),
            Hediff v => AddRule(keyword, v),
            TaggedString v => AddRule(keyword, v),
            _ => AddRule(keyword, value.ToString())
        };
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

    public HistoryDescriptionBuilder AddConstantIf(bool condition, string key, string value)
    {
        if (condition) AddConstant(key, value);
        return this;
    }

    public string Resolve()
    {
        if (EventDef.rulePackDef == null)
        {
            Log.Error($"PawnEventDef '{EventDef.defName}' has null rulePackDef while resolving '{RootKeyword}'.");
            return EventDef.description ?? EventDef.defName;
        }

        var request = new GrammarRequest();

        request.Includes.Add(EventDef.rulePackDef);
        request.Rules.Add(new Rule_String("PAWN", Pawn.NameShortColored.Resolve()));

        if (includePawnRules)
            request.Rules.AddRange(GrammarUtility.RulesForPawn("PAWN", Pawn));

        request.Rules.AddRange(extraRules);
        request.Constants.AddRange(extraConstants);

        return GrammarResolver.Resolve(RootKeyword, request);
    }
}

public static class HistoryDescriptionBuilderExtensions
{
    public static HistoryDescriptionBuilder WithFaction(this HistoryDescriptionBuilder builder, Faction faction)
    {
        if (faction == null)
            return builder;

        return builder.AddRule("FACTION", faction.NameColored.Resolve());
    }

    public static HistoryDescriptionBuilder RulesForPawn(this HistoryDescriptionBuilder builder, string pawnSymbol, Pawn pawn)
    {
        if (pawn == null)
            return builder;

        return builder.AddRules(GrammarUtility.RulesForPawn(pawnSymbol, pawn));
    }

    public static HistoryDescriptionBuilder WithOthers(this HistoryDescriptionBuilder builder, List<Pawn> pawns)
    {
        var otherCount = pawns.Count - 1;
        var otherTag = Faction.OfPlayer.HostileTo(pawns[0]?.Faction) ? TagType.Threat : TagType.ColonistCount;
        var otherText = otherCount switch
        {
            1 => (otherCount + " other").ApplyTag(otherTag).Resolve(),
            _ => (otherCount + " others").ApplyTag(otherTag).Resolve(),
        };

        return builder.AddRule("Others", otherText).AddConstant("OtherCount", otherCount);
    }
}