using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MineralFoundRecorder : RecorderBase<LongRangeMineralFoundEvent>, IRecord<DeepMineralFoundEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<LongRangeMineralFoundEvent>(CreateRecord);
        GameEventBus.Subscribe<DeepMineralFoundEvent>(CreateRecord);
    }

    public override void CreateRecord(LongRangeMineralFoundEvent input)
    {
        var (pawn, material) = input;
        if (!ShouldRecord(pawn))
            return;

        var recordDef = HistoryRecordDefOf.LongRangeMineralFound;
        if (IsTooSoonToRecordAgain(pawn, recordDef, 5f))
            return;
        
        var desc = recordDef.Description(pawn)
            .AddRule("Material", material)
            .Format();

        AddRecord(recordDef, pawn, desc);
    }

    public void CreateRecord(DeepMineralFoundEvent e)
    {
        var (pawn, material, position) = e;
        if (!ShouldRecord(pawn))
            return;
    
        var recordDef = HistoryRecordDefOf.DeepMineralFound;
        if (IsTooSoonToRecordAgain(pawn, recordDef, 6f))
            return;

        var desc = recordDef.Description(pawn)
            .AddRule("Material", material)
            .Format();

        AddRecord(recordDef, pawn, desc, location: new RecordLocation { map = pawn.Map, position = position });
    }

    [TestTag("Flaky")]
    public void TestLongRange(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var scanner = scenario.Thing(DefLookup.Thing.LongRangeMineralScanner).CreateSingle();
        var longRangeScanner = scanner.TryGetComp<CompLongRangeMineralScanner>();
        
        Accessor.CompLongRangeMineralScanner.TargetMineable(longRangeScanner) = ThingDefOf.MineableGold;
        Accessor.CompLongRangeMineralScanner.DoFind(longRangeScanner, pawn);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] found a lump of gold some distance away using the long-range mineral scanner.", HistoryRecordDefOf.LongRangeMineralFound);
    }

    public void TestDeep(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var scanner = scenario.Thing(ThingDefOf.GroundPenetratingScanner).CreateSingle();
        var deepScanner = scanner.TryGetComp<CompDeepScanner>();

        Accessor.CompDeepScanner.DoFind(deepScanner, pawn);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] found a lump of buried [Material] using the ground-penetrating scanner.", HistoryRecordDefOf.DeepMineralFound);
    }
}
