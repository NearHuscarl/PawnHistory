using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

// TODO: more testing
public class RitualOutcomeRecorder : RecorderBase<RitualOutcomeCompletedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<RitualOutcomeCompletedEvent>(CreateRecord);
    }

    public override void CreateRecord(RitualOutcomeCompletedEvent e)
    {
        if (e?.Host == null)
            return;

        if (!ShouldRecord(e.Host))
            return;

        var ritualJoiners = e.Participants.Concat(e.Host).ToList();
        var recordDef = HistoryRecordDefOf.RitualOutcome;
        var desc = recordDef
            .Description(e.Host)
            .AddRule("Ritual", e.RitualLabel)
            .AddRule("Outcome", e.OutcomeLabel.ToLowerInvariant(), addSubsymbols: true)
            .WithOthers(ritualJoiners)
            .Resolve();

        AddRecord(recordDef, e.Host, desc);
    }

    [RequiresRoyalty]
    public void TestSpeech(TestScenario scenario)
    {
        var organizer = scenario.Pawn().FullHeal().Colonist().SetRoyalTitle(Extra.RoyalTitleDefOf.Praetor).CreateSingle();
        var spectators = scenario.Pawn(4).Colonist().Execute();

        scenario.Map()
            .BuildRoom(10, 10, floorDef: TerrainDefOf.MetalTile)
            .AsThroneRoom(organizer)
            .Execute();
        
        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.AttendedSpeech.BestOutcome)
            .ThroneSpeech(spectators)
            .Execute();

        Expect.That(organizer).ToHaveHistoryRecord(HistoryRecordDefOf.RitualOutcome, "[PAWN] delivered an inspirational throne speech to 4 others.");
    }

    [RequiresIdeology]
    public void TestExecution(TestScenario scenario)
    {
        var executionIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.Execution).Execute();
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(executionIdeo)
            .CreateSingle();
        var spectators = scenario.Pawn(2).Colonist().Execute();
        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8, "prison")
            .AsPrison(1, prisoners: prisoners)
            .Execute();

        scenario.Map()
            .BuildRoom(MapBuilder.Beside("prison", Rot4.East, 8, 8), "shrine", floorDef: TerrainDefOf.MetalTile)
            .AsShrine(executionIdeo)
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.Execution.BestOutcome)
            .Execution(prisoners[0], spectators)
            .Execute();
        
        Expect.That(organizer).ToHaveHistoryRecord(HistoryRecordDefOf.RitualOutcome, "[PAWN] delivered a spectacular public execution to 2 others.");
    }
}
