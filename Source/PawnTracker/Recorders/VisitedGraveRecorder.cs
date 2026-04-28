using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class VisitedGraveRecorder : HistoryTaleRecorder<VisitedGraveRecorder.Input>
{
    public record Input(Pawn Pawn, Pawn DeadPawn);

    protected override float DaysToRecordAgain => 3f;
    
    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale != Extra.TaleDefOf.VisitedGrave)
                return;

            CreateRecord(new Input(e.Pawn, e.Params[0] as Pawn));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawn, deadPawn) = input;
        var recordDef = HistoryRecordDefOf.VisitedGrave;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Corpse", deadPawn)
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc, [deadPawn]);
        AddRecord(recordDef, deadPawn, desc, [pawn]);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(5).Colonist().Execute();

        for (var i = 0; i < pawns.Count; i++)
        {
            TaleRecorder.RecordTale(Extra.TaleDefOf.VisitedGrave, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Extra.TaleDefOf.VisitedGrave, pawns[i], pawns[(i + 1) % pawns.Count]);
            TaleRecorder.RecordTale(Extra.TaleDefOf.VisitedGrave, pawns[i], pawns[(i + 1) % pawns.Count]);
        }
    }
}
