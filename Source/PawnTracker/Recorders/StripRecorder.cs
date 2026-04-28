using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class StripRecorder : HistoryTaleRecorder<StripRecorder.Input>
{
    public record Input(Pawn Pawn, Pawn StrippedPawn);

    protected override float DaysToRecordAgain => 5f;

    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale != Extra.TaleDefOf.Stripped)
                return;

            CreateRecord(new Input(e.Pawn, e.Params[0] as Pawn));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawn, strippedPawn) = input;
        var recordDef = HistoryRecordDefOf.Stripped;
        var desc = recordDef.Description(pawn)
            .AddRule("STRIPPED", strippedPawn, addSubsymbols: true)
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc, [strippedPawn]);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Execute();

        for (var i = 0; i < pawns.Count; i++)
        {
            TaleRecorder.RecordTale(Extra.TaleDefOf.Stripped, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Extra.TaleDefOf.Stripped, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Extra.TaleDefOf.Stripped, pawns[i], pawns[(i + 1) % pawns.Count]);
        }
    }
}
