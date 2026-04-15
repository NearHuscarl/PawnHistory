using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class WalkNakedRecorder : HistoryTaleRecorder<WalkNakedRecorder.Input>
{
    public record Input(Pawn Pawn);

    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != TaleDefOf.WalkedNaked)
                return;

            CreateRecord(new Input(e.Pawn));
        });
    }

    public override void CreateRecord(Input e)
    {
        var pawn = e.Pawn;
        var recordDef = HistoryRecordDefOf.WalkNaked;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Do(p => p.Strip()).Execute();

        foreach (var pawn in pawns)
        {
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
        }
    }
}
