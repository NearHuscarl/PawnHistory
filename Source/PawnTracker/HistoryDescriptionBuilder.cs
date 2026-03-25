using PawnHistory.Source.Helper;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker;

public class HistoryDescriptionBuilder(HistoryRecordDef recordDef, string rootKeyword, Pawn pawn)
{
    public HistoryRecordDef HistoryRecordDef { get; } = recordDef;
    public string RootKeyword { get; } = rootKeyword;
    public Pawn Pawn { get; } = pawn;

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
            extraRules.Add(new Rule_String(keyword, value));
            if (addSubsymbols)
                extraRules.AddRange(LangUtility.RulesForString(keyword, value));
        }
        else
            namedArgs[keyword] = value;
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, TaggedString value, bool replaceIfExist = false)
    {
        if (value == null) return this;
        return AddRule(keyword, value.Resolve(), replaceIfExist);
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Pawn pawn, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (pawn == null) return this;

        AddRule(keyword, pawn.NameShortColored.Resolve(), replaceIfExist);

        if (addSubsymbols)
            return AddRules(GrammarUtility.RulesForPawn(keyword, pawn));
        return this;
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Faction faction, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (faction == null) return this;
        
        AddRule(keyword, faction.NameColored.Resolve(), replaceIfExist);
        if (addSubsymbols)
            return AddRules(GrammarUtility.RulesForFaction(keyword, faction));
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
        return AddRule(keyword, hediffDef.label.Colorize(hediffDef.defaultLabelColor), addSubsymbols, replaceIfExist);
    }

    public HistoryDescriptionBuilder AddRule(string keyword, Hediff hediff, BodyPartRecord bodyPart, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (bodyPart == null)
            return AddRule(keyword, hediff, addSubsymbols, replaceIfExist);

        return AddRule(keyword, hediff.def.PrettyTextForPart(bodyPart), addSubsymbols, replaceIfExist);
    }

    public HistoryDescriptionBuilder AddRule(string keyword, BodyPartRecord part, bool addSubsymbols = false, bool replaceIfExist = false)
    {
        if (part == null) return this;
        // Label = left middle toe
        // LabelShort = toe
        return AddRule(keyword, part.Label, addSubsymbols, replaceIfExist);
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
        if (HistoryRecordDef.descriptionMaker == null)
        {
            if (HistoryRecordDef.description == null)
            {
                Log.Error($"PawnEventDef '{HistoryRecordDef.defName}' does not have description defined in either rulePackDef or description.");
                return "ERR: No description found";
            }

            List<NamedArgument> args = [Pawn.NameShortColored.Named("PAWN")];

            foreach (var kvp in namedArgs)
                args.Add(kvp.Value.Named(kvp.Key));

            return HistoryRecordDef.description.Formatted(args).Resolve();
        }

        if (RootKeyword == null)
        {
            Log.Error($"Error when resolving '{HistoryRecordDef.defName}' description: RootKeyword is null.");
            return "ERR: RootKeyword=null";
        }

        var request = new GrammarRequest();

        request.Includes.Add(HistoryRecordDef.descriptionMaker);
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