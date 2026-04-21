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
        var organizer = scenario.Pawn().FullHeal().Colonist().SetRoyalTitle(DefLookup.RoyalTitle.Praetor).CreateSingle();
        var spectators = scenario.Pawn(4).Colonist().Execute();

        scenario.Map()
            .BuildRoom(10, 10, floorDef: TerrainDefOf.MetalTile)
            .AsThroneRoom(organizer)
            .Execute();
        
        scenario
            .Ritual(organizer)
            .Outcome(DefLookup.RitualOutcomeEffect.AttendedSpeech.BestOutcome)
            .ThroneSpeech(spectators)
            .Execute();

        Expect.That(organizer).ToHaveHistoryRecord("[PAWN] delivered an inspirational throne speech to 4 others.", HistoryRecordDefOf.RitualOutcome);
    }
}
