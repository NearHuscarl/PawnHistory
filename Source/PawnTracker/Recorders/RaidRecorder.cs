using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using PawnHistory.Source.Helper;
using Verse;

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

        if (isFriendly)
            return;
        
        var recordDef = HistoryRecordDefOf.Raid;
        var asker = QuestHelper.GetAsker(quest);

        foreach (var pawn in pawns)
        {
            if (!ShouldRecord(pawn))
                continue;

            var concerns = new List<Thing> { asker };
            var builder = recordDef.Description(pawn)
                .IncludePawnGrammar()
                .WithOthers(pawns)
                .AddRule("Faction", faction)
                .AddRule("QuestAsker", asker)
                .AddConstant("raidProperty", GetRaidProperty(raidStrategy, raidArrivalMode))
                .AddConstant("quest", quest?.root.defName);
            
            var buildInput = new RaidComp.BuildInput(pawn, faction, quest);
            foreach (var comp in Comps.OfType<RaidComp>())
            {
                if (!comp.Match(buildInput))
                    continue;

                builder = comp.BuildGrammarRequest(builder, buildInput);
                concerns.AddRange(comp.GetConcerns(buildInput));
            }
            
            AddRecord(recordDef, pawn, builder.Resolve(), concerns, quest: quest);
        }
    }

    private enum RaidProperty
    {
        None,
        Siege,
        Breacher,
        Sapper,
        CenterDrop,
    }

    private static RaidProperty GetRaidProperty(RaidStrategyDef raidStrategy, PawnsArrivalModeDef raidArrivalMode)
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

        return raidProperty;
    }
    
    [DebugValues(70, 100, 140, 500)]
    public void TestWithParams(TestScenario scenario, int point)
    {
        scenario.Incident(IncidentDefOf.RaidEnemy).Point(point).Execute();
    }

    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.RaidEnemy).Point(180).Execute();
        Expect.ThatAll(pawns).ToHaveHistoryRecord(HistoryRecordDefOf.Raid, "[PAWN] and [Others] from [Faction] raided the colony.");
    }

    public void TestSiege(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.RaidEnemy).RaidStrategy(Extra.RaidStrategyDefOf.Siege).Point(500).Execute();
        Expect.ThatAll(pawns).ToHaveHistoryRecord(HistoryRecordDefOf.Raid, "[PAWN] and [n] others from [FACTION] besieged the colony.");
    }

    public void TestCenterDrop(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.RaidEnemy).Point(200).RaidArrivalMode(PawnsArrivalModeDefOf.CenterDrop).Execute();
        Expect.ThatAll(pawns).ToHaveHistoryRecord(HistoryRecordDefOf.Raid, "[PAWN] and [Others] from [Faction] raided the colony by dropping directly into it.");
    }
}
