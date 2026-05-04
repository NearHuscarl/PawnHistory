using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ExhaustedRecorder : HistoryTaleRecorder<ExhaustedRecorder.Input>
{
    public record Input(Pawn Pawn);

    protected override float DaysToRecordAgain => 3f;

    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale != TaleDefOf.Exhausted)
                return;

            CreateRecord(new Input(e.Pawn));
        });
    }

    public override void CreateRecord(Input input)
    {
        var pawn = input.Pawn;
        var recordDef = HistoryRecordDefOf.Exhausted;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc);
    }

    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(5).Colonist()
            .Do(p => TaleRecorder.RecordTale(TaleDefOf.Exhausted, p))
            .Execute();

        Expect.ThatAll(pawns).ToHaveHistoryRecordOf(HistoryRecordDefOf.Exhausted);
    }
}
