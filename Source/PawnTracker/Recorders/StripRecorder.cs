using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class StripRecorder : HistoryTaleRecorder
{
    private static readonly TaleDef Stripped = DefDatabase<TaleDef>.GetNamed("Stripped");

    public override void Register()
    {
        DaysToRecordAgain = 5f;

        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != Stripped)
                return;

            HandleStripEvent(e);
        });
    }

    private void HandleStripEvent(TaleRecordedEvent e)
    {
        var recordDef = HistoryRecordDefOf.Stripped;
        var strippedPawn = e.Params[0] as Pawn;
        var desc = recordDef.Description(e.Pawn)
            .AddRule("STRIPPED", strippedPawn, addSubsymbols: true)
            .Resolve();

        if (!ShouldRecordTale(e.Pawn, recordDef, desc))
            return;

        AddRecord(recordDef, e.Pawn, desc, [strippedPawn]);
    }

    public override void Test(TestScenario scenario)
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
