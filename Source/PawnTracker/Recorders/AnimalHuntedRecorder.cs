using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class AnimalHuntedRecorder : HistoryTaleRecorder<AnimalHuntedEvent>
{
    protected override float DaysToRecordAgain => 6f;

    public override void Register()
    {
        GameEventBus.Subscribe<AnimalHuntedEvent>(CreateRecord);
    }

    public override void CreateRecord(AnimalHuntedEvent e)
    {
        var (pawn, prey) = e;
        var recordDef = HistoryRecordDefOf.Hunted;
        var desc = recordDef.Description(pawn)
            .AddRule("Prey", prey, addSubsymbols: true)
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc, [prey]);
    }

    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Execute();

        foreach (var pawn in pawns)
        {
            var animal = scenario.Pawn().Animal().CreateSingle();

            TaleRecorder.RecordTale(TaleDefOf.Hunted, pawn, animal);
            TaleRecorder.RecordTale(TaleDefOf.Hunted, pawn, animal);
        }

        Expect.ThatAll(pawns).ToHaveHistoryRecordOf(HistoryRecordDefOf.Hunted);
    }
}
