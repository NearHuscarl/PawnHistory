using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class StripRecorder : HistoryTaleRecorder<StripRecorder.Input>
{
    public record Input(Pawn pawn, Pawn strippedPawn);

    private static readonly TaleDef Stripped = DefDatabase<TaleDef>.GetNamed("Stripped");

    public override void Register()
    {
        DaysToRecordAgain = 5f;

        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != Stripped)
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
            TaleRecorder.RecordTale(Stripped, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Stripped, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Stripped, pawns[i], pawns[(i + 1) % pawns.Count]);
        }
    }
}
