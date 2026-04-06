using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class AnimalHuntedRecorder : HistoryTaleRecorder
{
    public override void Register()
    {
        DaysToRecordAgain = 6f;

        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != TaleDefOf.Hunted)
                return;

            if (e.Params[0] is not Pawn prey || !prey.RaceProps.Animal)
                return;

            HandleHuntedEvent(e.Pawn, prey);
        });
    }

    private void HandleHuntedEvent(Pawn pawn, Pawn prey)
    {
        var recordDef = HistoryRecordDefOf.Hunted;
        var desc = recordDef.Description(pawn)
            .AddRule("Prey", prey, addSubsymbols: true)
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc, [prey]);
    }

    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Execute();
        var animal = scenario.Pawn().Animal().CreateSingle();

        foreach (var pawn in pawns)
        {
            TaleRecorder.RecordTale(TaleDefOf.Hunted, pawn, animal);
            TaleRecorder.RecordTale(TaleDefOf.Hunted, pawn, animal);
        }

        Expect.AnyPawnOnMap().ToHaveHistoryRecordOf(HistoryRecordDefOf.Hunted);
    }
}
