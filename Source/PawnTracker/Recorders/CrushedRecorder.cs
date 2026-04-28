using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

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

        // This is just an intermediate record to store the crushed position so the Death record can reference it later.
        var recordDef = HistoryRecordDefOf.Crushed;
        foreach (var pawn in pawns)
        {
            if (!ShouldRecord(pawn))
                continue;

            var desc = recordDef.Description(pawn)
                .Format();
            AddRecord(recordDef, pawn, desc, location: new RecordLocation { map = map, position = position });
        }
    }

    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .Position(Find.CurrentMap.Center)
            .CreateSingle();
        var spouse = scenario.Pawn()
            .Position(CellFinder.RandomEdgeCell(Find.CurrentMap))
            .SetRelation(pawn, PawnRelationDefOf.Spouse)
            .CreateSingle();

        scenario.Map().CollapseRoofAndCrush(pawn).Execute();

        Expect.That(pawn).ToHaveHistoryRecordOf(HistoryRecordDefOf.Crushed, -2);
        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.Death,
            Position = pawn.Position,
            Map = pawn.Map,
        }, index: -1);

        Expect.That(spouse).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RelativeDeath,
            Position = pawn.Position,
            Map = pawn.Map,
            Concerns = [pawn]
        });
    }
}
