using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class OnFireRecorder : HistoryTaleRecorder<OnFireRecorder.Input>
{
    public record Input(Pawn Pawn);

    protected override float DaysToRecordAgain => 3f;

    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale != TaleDefOf.WasOnFire)
                return;

            CreateRecord(new Input(e.Pawn));
        });
    }

    public override void CreateRecord(Input input)
    {
        var pawn = input.Pawn;
        var recordDef = HistoryRecordDefOf.OnFire;
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
            .Do(p => TaleRecorder.RecordTale(TaleDefOf.WasOnFire, p))
            .Execute();

        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN] caught fire.", HistoryRecordDefOf.OnFire);
    }
}
