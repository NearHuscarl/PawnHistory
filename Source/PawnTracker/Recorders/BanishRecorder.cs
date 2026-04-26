using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class BanishRecorder : RecorderBase<BanishEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<BanishEvent>(CreateRecord);
    }

    public override void CreateRecord(BanishEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.Banish;
        var desc = recordDef.Description(e.Pawn)
            .WithPlayerFaction()
            .AddConstant("leftToDie", e.LeftToDie)
            .AddConstant("reason", e.Reason)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc);
    }

    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        PawnBanishUtility.Banish(pawn, pawn.Tile);
        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] was banished from the colony.", HistoryRecordDefOf.Banish);

        var pawn2 = scenario.Pawn().Colonist().Do(p => HealthUtility.DamageUntilDowned(p)).CreateSingle();
        PawnBanishUtility.Banish(pawn2, pawn.Tile);
        Expect.That(pawn2).ToHaveHistoryRecord("[PAWN] was banished from the colony and left to die.", HistoryRecordDefOf.Banish);
    }

    // TODO: test anomaly dlc
    [RequiresAnomaly]
    public void TestCube(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .AddHediff(HediffDefOf.CubeInterest)
            .CreateSingle();
        var dickhead = scenario.Pawn().Colonist().CreateSingle();

        scenario.Caravan([pawn]).Execute();

        var cube = scenario.Thing(ThingDefOf.GoldenCube).CreateSingle<ThingWithComps>();

        Accessor.CompGoldenCube.OnInteracted(cube.GetComp<CompGoldenCube>(), dickhead);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] banished the colony after the golden cube was destroyed.", HistoryRecordDefOf.Banish);
    }
}
