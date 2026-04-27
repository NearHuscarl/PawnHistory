using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PawnLostRecorder : RecorderBase<PawnLostEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PawnLostEvent>(CreateRecord);
    }

    public override void CreateRecord(PawnLostEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.PawnLost;
        var desc = recordDef.Description(e.Pawn)
            .AddRule("WorldObject", e.MapName)
            .AddConstant("mapType", e.MapType)
            .AddConstant("isKidnapped", e.IsKidnapped)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc);
    }

    public void TestHome(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();

        Current.Game.DeinitAndRemoveMap(Find.CurrentMap, true);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] was lost after [WorldObject] was abandoned.", HistoryRecordDefOf.PawnLost);
    }

    // TODO: fix and test tileid because map deinit remove that information
    public void TestCaravan(TestScenario scenario)
    {
        Expect.Assertions(1);

        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var settlement = Find.WorldObjects.Settlements.FirstOrDefault();
        
        scenario.Caravan([pawn])
            .Attack(settlement)
            .OnMapGenerated(e =>
            {
                Current.Game.DeinitAndRemoveMap(e.Map, true);
                Expect.That(pawn).ToHaveHistoryRecord("[PAWN] was lost after the caravan was disbanded.", HistoryRecordDefOf.PawnLost);
            })
            .Execute();
    }
}
