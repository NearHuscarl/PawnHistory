using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RaidRecorder : RecorderBase<RaidStartedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<RaidStartedEvent>(CreateRecord);
    }

    public override void CreateRecord(RaidStartedEvent input)
    {
        var (pawns, faction, raidStrategy, raidArrivalMode, isFriendly, quest) = input;
        
        pawns = pawns.Where(ShouldRecord).ToList();
        
        if (isFriendly)
            RecordRaidFriendlyStarted(pawns, faction);
        else
            RecordRaidEnemyStarted(pawns, faction, raidStrategy, raidArrivalMode, quest);
    }

    private void RecordRaidFriendlyStarted(List<Pawn> pawns, Faction faction)
    {
        var recordDef = HistoryRecordDefOf.RaidFriendly;
        var hostileFaction = pawns[0].MapHeld.lordManager.lords
            .FirstOrDefault(l => l.faction != null && l.faction.HostileTo(faction))
            ?.faction;

        foreach (var pawn in pawns)
        {
            var desc = recordDef.Description(pawn)
                .AddRule("Faction", faction)
                .AddRule("HostileFaction", hostileFaction)
                .WithOthers(pawns)
                .AddConstant("enemyHasFaction", hostileFaction != null) // not manhunter/insect
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

    private void RecordRaidEnemyStarted(List<Pawn> pawns, Faction faction, RaidStrategyDef raidStrategy, PawnsArrivalModeDef raidArrivalMode, Quest quest)
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
        var asker = QuestHelper.GetAsker(quest);

        foreach (var pawn in pawns)
        {
            var desc = recordDef.Description(pawn)
                .IncludePawnGrammar()
                .WithOthers(pawns)
                .AddRule("Faction", faction)
                .AddRule("QuestAsker", asker)
                .AddConstant("raidProperty", raidProperty)
                .AddConstant("quest", quest?.root.defName)
                .Resolve();
            AddRecord(recordDef, pawn, desc, [asker], quest: quest);
        }
    }

    [DebugValues(70, 100, 140, 500)]
    public void Test(TestScenario scenario, int point)
    {
        scenario.Incident(IncidentDefOf.RaidEnemy).Point(point).Execute();
    }

    public void TestSiege(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.RaidEnemy).RaidStrategy(DefLookup.RaidStrategy.Siege).Point(500).Execute();
        
        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN] and [n] others from [FACTION] besieged the colony.", HistoryRecordDefOf.Raid);
    }

    [SkipTest]
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
