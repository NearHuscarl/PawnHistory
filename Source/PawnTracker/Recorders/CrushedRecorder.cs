using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class CrushedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<CrushedEvent>(e =>
        {
            HandleCrushedEvent(e);
        });
    }

    private void HandleCrushedEvent(CrushedEvent e)
    {
        // This is just an intermidate record to store the crushed position so the Death record can reference it later.
        var recordDef = HistoryRecordDefOf.Crushed;
        foreach (var pawn in e.Pawns)
        {
            var desc = recordDef.Description(pawn)
                .Format();
            AddRecord(recordDef, pawn, desc, location: new RecordLocation() { map = e.Map, position = e.Position });
        }
    }

    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .CreateSingle();
        var spouse = scenario.Pawn().SetRelation(pawn, PawnRelationDefOf.Spouse).CreateSingle();

        scenario.Map().CollapseRoofAndCrush(pawn);

        Expect.That(pawn).ToHaveHistoryRecordOf(HistoryRecordDefOf.Crushed, -2);
        Expect.That(pawn).ToHaveHistoryRecordOf(HistoryRecordDefOf.Death, -1);
        Expect.That(pawn).ToHaveHistoryRecordPosition(pawn.Position);

        Expect.That(spouse).ToHaveHistoryRecordOf(HistoryRecordDefOf.RelativeDeath);
        Expect.That(spouse).ToHaveHistoryRecordPosition(pawn.Position);
    }
}