using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class AnimalHuntedRecorder : HistoryTaleRecorder<AnimalHuntedRecorder.Input>
{
    public record Input(Pawn Pawn, Pawn Prey);
    protected override float DaysToRecordAgain => 6f;

    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale != TaleDefOf.Hunted)
                return;

            if (e.Params[0] is not Pawn prey || !prey.RaceProps.Animal)
                return;

            CreateRecord(new Input(e.Pawn, prey));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawn, prey) = input;
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

        Expect.ThatAny(pawns).ToHaveHistoryRecordOf(HistoryRecordDefOf.Hunted);
    }
}
