using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class VisitedGraveRecorder : HistoryTaleRecorder
{
    private static readonly TaleDef VisitedGrave = DefDatabase<TaleDef>.GetNamed("VisitedGrave");

    public override void Register()
    {
        DaysToRecordAgain = 3f;

        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != VisitedGrave)
                return;

            HandleVisitedGraveEvent(e);
        });
    }

    private void HandleVisitedGraveEvent(TaleRecordedEvent e)
    {
        var recordDef = HistoryRecordDefOf.VisitedGrave;
        var corpse = e.Params[0] as Pawn;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Corpse", corpse)
            .Resolve();

        if (!ShouldRecordTale(e.Pawn, recordDef, desc))
            return;

        AddRecord(recordDef, e.Pawn, desc, [corpse]);
        AddRecord(recordDef, corpse, desc, [e.Pawn]);
    }

    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(5).Colonist().Execute();

        for (var i = 0; i < pawns.Count; i++)
        {
            TaleRecorder.RecordTale(VisitedGrave, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(VisitedGrave, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(VisitedGrave, pawns[i], pawns[(i + 1) % pawns.Count]);
        }
    }
}
