using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PawnGeneratedRecorder : RecorderBase<PawnGeneratedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PawnGeneratedEvent>(CreateRecord);
    }

    public override void CreateRecord(PawnGeneratedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        AddRecord(HistoryRecordDefOf.PawnGenerated, e.Pawn, $"{e.Pawn.NameFullColored} was generated.");
    }

    // Basic test: Arrival stays at anchor and managed records move earlier
    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var arrival = MakeRecord(HistoryRecordDefOf.NewArrival, pawn);
        var scar = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        pawn.HistoryRecords.AddRange([arrival, scar, generated]);

        RunWithSeed(12345, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));

        Expect.That(scar.date).Not().Equal(anchor);
        Expect.That(scar.date).LessThan(generated.date);
        Expect.That(generated.date).Equal(arrival.date);
        Expect.That(pawn.HistoryRecords).SequenceEqual([scar, arrival, generated]);
        Expect.That(generated.pinned).True();
    }

    // Unregistered same tick records stay at anchor
    public void TestUnregistered(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        ResetHistory(pawn);

        var leaderChanged = MakeRecord(HistoryRecordDefOf.LeaderChanged, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        pawn.HistoryRecords.AddRange([leaderChanged, generated]);

        RunWithSeed(12345, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));

        Expect.That(pawn.HistoryRecords).SequenceEqual([leaderChanged, generated]);
        Expect.That(leaderChanged.date).Equal(generated.date);
    }

    // record backfill of Stellarch only keeps the latest psylink
    [RequiresRoyalty]
    public void TestStellarch(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        
        ResetHistory(pawn);

        var title = MakeRecord(HistoryRecordDefOf.TitleGained, pawn);
        var psylink1 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn);
        var psylink2 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn);
        var psylink3 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        pawn.HistoryRecords.AddRange([title, psylink1, psylink2, psylink3, generated]);

        RunWithSeed(12345, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));

        Expect.That(title.date).LessThan(anchor);
        Expect.That(title.date + GenDate.TicksPerDay <= psylink3.date).True();
        Expect.That(psylink3.date).LessThan(anchor);
        Expect.That(generated.date).Equal(anchor);
        Expect.That(pawn.HistoryRecords).SequenceEqual([title, psylink3, generated]);
    }

    [RequiresRoyalty]
    public void TestWeaponBonded(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        ResetHistory(pawn);

        var bonded = MakeRecord(HistoryRecordDefOf.WeaponBonded, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        pawn.HistoryRecords.AddRange([bonded, generated]);

        RunWithSeed(12345, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));

        Expect.That(bonded.date).LessThan(generated.date);
    }
    private static void ResetHistory(Pawn pawn) => CompHistoryManager.GetComp(pawn).ClearAll();

    private static void RunWithSeed(int seed, Action action)
    {
        Rand.PushState(seed);
        try
        {
            action();
        }
        finally
        {
            Rand.PopState();
        }
    }

    private static HistoryRecord MakeRecord(HistoryRecordDef def, Pawn pawn)
    {
        return new HistoryRecord(def, pawn, def.label);
    }
}
