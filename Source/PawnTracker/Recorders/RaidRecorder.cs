using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class RaidRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<RaidStartedEvent>(e =>
        {
            var pawns = e.Pawns.Where(ShouldRecord).ToList();

            if (e.IsFriendly)
                HandleRaidFriendlyStartedEvent(pawns, e.Faction);
            else
                HandleRaidEnemyStartedEvent(pawns, e.Faction, e.RaidStrategy, e.RaidArrivalMode);
        });
    }

    private void HandleRaidFriendlyStartedEvent(List<Pawn> pawns, Faction faction)
    {
        var recordDef = HistoryRecordDefOf.RaidFriendly;
        var hostileFaction = pawns[0].MapHeld.lordManager.lords
            .FirstOrDefault(l => l.faction != null && l.faction.HostileTo(faction))
            ?.faction;

        foreach (var pawn in pawns)
        {
            var rules = new List<Rule>();
            var constants = new Dictionary<string, string>();
            var desc = recordDef.Description(pawn)
                .WithFaction(faction)
                .WithOthers(pawns)
                .AddConstantIf(hostileFaction != null, "hostileFaction", "true") // not manhunter/insect
                .AddRule("HostileFaction", hostileFaction)
                .Resolve();

            AddRecord(recordDef, pawn, desc);
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

    private void HandleRaidEnemyStartedEvent(List<Pawn> pawns, Faction faction, RaidStrategyDef raidStrategy, PawnsArrivalModeDef raidArrivalMode)
    {
        var raidProperty = RaidProperty.None;

        if (raidArrivalMode.defName == "CenterDrop")
            raidProperty = RaidProperty.CenterDrop;
        else if (raidStrategy.defName.StartsWith("ImmediateAttackBreaching"))
            raidProperty = RaidProperty.Breacher;
        else if (raidStrategy.defName.StartsWith("ImmediateAttackSappers"))
            raidProperty = RaidProperty.Sapper;
        else if (raidStrategy.defName.StartsWith("Siege"))
            raidProperty = RaidProperty.Siege;

        var recordDef = HistoryRecordDefOf.Raid;

        foreach (var pawn in pawns)
        {
            var desc = recordDef.Description(pawn)
                .WithFaction(faction)
                .WithOthers(pawns)
                .AddConstant("raidProperty", raidProperty)
                .Resolve();
            AddRecord(recordDef, pawn, desc);
        }
    }

    [DebugValues(70, 100, 140, 500)]
    public void Test(TestScenario scenario, int point)
    {
        scenario.Incident(IncidentDefOf.RaidEnemy).Point(point).Execute();
    }

    public void TestSiege(TestScenario scenario)
    {
        scenario.Siege().Point(500).Execute();
    }

    public void TestCenterDrop(TestScenario scenario)
    {
        scenario.Incident(IncidentDefOf.RaidEnemy).Point(500).RaidArrivalMode(PawnsArrivalModeDefOf.CenterDrop).Execute();
    }

    [DebugValues(70, 100, 140, 500)]
    public void TestFriendly(TestScenario scenario, int point)
    {
        scenario.RaidFriendly()
            .Point(point)
            .Execute();
    }
}
