using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;

namespace PawnHistory.Source.PawnTracker;

// Features:
// - Display history record
//   - Colorize names and important information
//   - Tooltip
//   - Click to jump to related pawns
//   - Some icons

// Icon References (AssetRipper)
// - Assets\Resources\textures\things\mote\thoughtsymbol
// - Assets\Resources\textures\things\mote\speechsymbols
// - Assets\Resources\textures\things\mote\battlesymbols
// - Assets\Resources\textures\ui

// Events:
// Mental breaks, reason
// Crawling to safety
// Permanent injury
// + From combat
// - From other curcumstances: scarification ritual, anomaly ritual, healing wound `.ispermanent = true`
// Raid type: seige with a different icon
// Ideology convert, belief reduced
// Pawn.Notify_PassedToWorld() event?
// More record icons?

// Create a filter in WorldPawn window (All/Alive/Dead)
// - Add a column to see the history of dead AND destroyed pawn. Concerned dead&destoryed pawn will open a dedicated history window.

// Bug:
// -- Human fist kill -> wrong message
// -- kill record POV is wrong for the killer

// TODO: handle another related pawn in BattleLogEntry_RangedImpact

[StaticConstructorOnStartup]
internal class PawnTracker
{
    static PawnTracker()
    {
        new HarmonyLib.Harmony("rimworld.mod.nearhuscarl.pawnhistory").PatchAllUncategorized(Assembly.GetExecutingAssembly());

        AddCompToHumanlikes();
        SetupEvenListeners();
    }

    public static bool ShouldTrack(ThingDef thingDef) => thingDef.race?.intelligence == Intelligence.Humanlike;
    public static bool ShouldTrack(Pawn pawn) => pawn != null && ShouldTrack(pawn.def);

    private static void AddCompToHumanlikes()
    {
        var defsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;

        for (var i = 0; i < defsListForReading.Count; ++i)
        {
            var thingDef = defsListForReading[i];
            if (ShouldTrack(thingDef) && !thingDef.IsCorpse)
            {
                thingDef.comps.Add(new CompProperties_History());
                CompHistoryManager.TrackingDefHash.Add(thingDef.shortHash);
                var type = typeof(ITab_Pawn_History);
                var sharedInstance = InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_History));

                thingDef.inspectorTabs?.AddDistinct(type);
                thingDef.inspectorTabsResolved?.AddDistinct(sharedInstance);

                if (thingDef.race?.corpseDef != null)
                {
                    thingDef.race.corpseDef.inspectorTabs?.AddDistinct(type);
                    thingDef.race.corpseDef.inspectorTabsResolved?.AddDistinct(sharedInstance);
                }
                else Log.Warning("[ModName] thingDef.race?.corpseDef == null for thingDef = " + thingDef.defName);
            }
        }
    }

    private static void SetupEvenListeners()
    {
        GameEventListener.Subscribe<GroupEvent>(e =>
        {
            foreach (var pawn in e.Pawns)
            {
                CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(e.eventDef, pawn, e.resolveDesc(pawn)));
            }
        });

        GameEventListener.Subscribe<GameEvent>(e =>
        {
            CompHistoryManager.GetComp(e.Pawn).records.Add(new HistoryRecord(e.eventDef, e.Pawn, e.resolvedDesc, e.relatedPawns));
        });

        GameEventListener.Subscribe<RaidEvent>(e =>
        {
            var pawns = e.Pawns.Where(ShouldTrack).ToList();

            if (e.IsFriendly)
                HandleRaidFriendlyStartedEvent(pawns, e.Faction);
            else
                HandleRaidEnemyStartedEvent(pawns, e.Faction, e.RaidStrategy, e.RaidArrivalMode);
        });

        GameEventListener.Subscribe<LordToilChangeEvent>(e =>
        {
            var lord = e.Lord;
            var currentToil = e.CurrentToil;
            var nextToil = e.NextToil;
            var trigger = e.Trigger;
            var pawns = lord.ownedPawns.Where(ShouldTrack).ToList();
            var isStartingLord = currentToil == null;

            Log.Message($"{lord.LordJob}: {currentToil}->{nextToil} trigger={trigger?.GetType().Name}");

            if (lord.LordJob is LordJob_TradeWithColony && !isStartingLord)
            {
                var trader = pawns.FirstOrDefault(p => p.trader != null);
                var traderKind = trader?.trader?.traderKind?.label ?? "trader";
                HandleCaravanEvents(nextToil, lord, pawns, trigger, traderKind);
            }
        });
    }

    private static void HandleRaidFriendlyStartedEvent(List<Pawn> pawns, Faction faction)
    {
        var otherCount = pawns.Count - 1;
        var eventDef = PawnEventDefOf.RaidFriendly;
        var hostileFaction = pawns[0].MapHeld.lordManager.lords
            .FirstOrDefault(l => l.faction != null && l.faction.HostileTo(faction))
            ?.faction;

        foreach (var pawn in pawns)
        {
            var request = new GrammarRequest();
            var friend = otherCount switch
            {
                0 => "",
                1 => $" and {(otherCount + " other").ApplyTag(TagType.ColonistCount).Resolve()}",
                _ => $" and {(otherCount + " others").ApplyTag(TagType.ColonistCount).Resolve()}",
            };
            request.Includes.Add(eventDef.rulePackDef);
            request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));
            request.Rules.Add(new Rule_String("FRIEND", friend));
            request.Rules.Add(new Rule_String("FACTION", faction.NameColored.Resolve()));

            if (hostileFaction != null) // not manhunter/insect
            {
                request.Rules.Add(new Rule_String("HOSTILEFACTION", hostileFaction.NameColored.Resolve()));
                request.Constants.Add("hostileFaction", "true");
            }
            var desc = GrammarResolver.Resolve("raidFriendly", request);
            CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(eventDef, pawn, desc));
        }
    }

    enum RaidProperty
    {
        None,
        Siege,
        Breacher,
        Sapper,
        CenterDrop,
    }

    private static void HandleRaidEnemyStartedEvent(List<Pawn> pawns, Faction faction, RaidStrategyDef raidStrategy, PawnsArrivalModeDef raidArrivalMode)
    {
        var raidProperty = RaidProperty.None;

        if (raidStrategy.defName.StartsWith("ImmediateAttackBreaching"))
            raidProperty = RaidProperty.Breacher;
        else if (raidStrategy.defName.StartsWith("ImmediateAttackSappers"))
            raidProperty = RaidProperty.Sapper;
        else if (raidStrategy.defName.StartsWith("Siege"))
            raidProperty = RaidProperty.Siege;
        else if (raidArrivalMode.defName == "CenterDrop")
            raidProperty = RaidProperty.CenterDrop;

        var otherCount = pawns.Count - 1;
        var eventDef = PawnEventDefOf.Raid;

        foreach (var pawn in pawns)
        {
            var request = new GrammarRequest();
            var threat = otherCount switch
            {
                0 => "",
                1 => $" and {(otherCount + " other").ApplyTag(TagType.Threat).Resolve()}",
                _ => $" and {(otherCount + " others").ApplyTag(TagType.Threat).Resolve()}",
            };

            request.Includes.Add(eventDef.rulePackDef);
            request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));
            request.Rules.Add(new Rule_String("THREAT", threat));
            request.Rules.Add(new Rule_String("FACTION", faction.NameColored.Resolve()));
            request.Constants.Add("raidProperty", raidProperty.ToString());

            var desc = GrammarResolver.Resolve("raid", request);
            CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(eventDef, pawn, desc));
        }
    }

    enum CaravanLeftReason
    {
        Timeout,
        DangerousTemperature,
        AnomalousWeather,
        Trapped,
        TraderLost,
        PawnLost,
    }

    private static void HandleCaravanEvents(LordToil nextToil, Lord lord, List<Pawn> pawns, Trigger trigger, string traderKind)
    {
        var faction = lord.faction;
        var reason = CaravanLeftReason.Timeout;

        if (trigger is Trigger_PawnExperiencingDangerousTemperatures)
            reason = CaravanLeftReason.DangerousTemperature;
        else if (trigger is Trigger_PawnExperiencingAnomalousWeather)
            reason = CaravanLeftReason.AnomalousWeather;
        else if (trigger is Trigger_PawnCannotReachMapEdge)
            reason = CaravanLeftReason.Trapped;
        else if (trigger is Trigger_ImportantTraderCaravanPeopleLost)
            reason = CaravanLeftReason.TraderLost;
        else if (trigger is Trigger_PawnLost || trigger is Trigger_FractionPawnsLost)
            reason = CaravanLeftReason.PawnLost;

        if (nextToil is LordToil_ExitMapAndEscortCarriers
            || nextToil is LordToil_ExitMap
            || nextToil is LordToil_ExitMapTraderFighting)
        {
            var eventDef = PawnEventDefOf.TradeCaravanLeft;

            foreach (var pawn in pawns)
            {
                var request = new GrammarRequest();

                request.Includes.Add(eventDef.rulePackDef);
                request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));
                request.Rules.Add(new Rule_String("TRADERKIND", traderKind));
                request.Rules.Add(new Rule_String("FACTION", faction.NameColored.Resolve()));
                request.Constants.Add("reason", reason.ToString());

                if (reason != CaravanLeftReason.Timeout)
                    request.Rules.Add(new Rule_String("REASON", reason.ToString()));

                var desc = GrammarResolver.Resolve("tradeCaravanLeft", request);
                CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(eventDef, pawn, desc));
            }
        }
    }
}
