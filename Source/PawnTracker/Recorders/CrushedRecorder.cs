using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class CrushedRecorder : RecorderBase<CrushedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<CrushedEvent>(CreateRecord);
    }

    public override void CreateRecord(CrushedEvent e)
    {
        var (pawns, map, position) = e;

        // This is just an intermidate record to store the crushed position so the Death record can reference it later.
        var recordDef = HistoryRecordDefOf.Crushed;
        foreach (var pawn in pawns)
        {
            if (!ShouldRecord(pawn))
                continue;

            var desc = recordDef.Description(pawn)
                .Format();
            AddRecord(recordDef, pawn, desc, location: new RecordLocation() { map = map, position = position });
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