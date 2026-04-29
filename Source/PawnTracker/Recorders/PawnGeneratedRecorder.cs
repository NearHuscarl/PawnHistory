using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Linq;
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

        AddRecord(HistoryRecordDefOf.PawnGenerated, e.Pawn, $"{e.Pawn.NameFullColored} was generated.", pinned: true);
    }

    public void TestArrivalStaysAtAnchorAndManagedRecordsMoveEarlier(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var arrival = MakeRecord(HistoryRecordDefOf.NewArrival, pawn, anchor);
        var scar = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn, anchor);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn, anchor, pinned: true);
        pawn.HistoryRecords.AddRange([arrival, scar, generated]);

        HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated);

        Expect.Assertions(4);
        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PawnGenerated, Date = anchor, Pinned = true });
        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.NewArrival, Date = anchor });
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.BodyPartScarred, Date = anchor });
        Expect.That(pawn).ToHaveHistoryRecordOf(HistoryRecordDefOf.BodyPartScarred);
    }

    public void TestUnregisteredSameTickRecordsStayAtAnchor(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var leaderChanged = MakeRecord(HistoryRecordDefOf.LeaderChanged, pawn, anchor);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn, anchor, pinned: true);
        pawn.HistoryRecords.AddRange([leaderChanged, generated]);

        HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated);

        Expect.Assertions(2);
        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.LeaderChanged, Date = anchor });
        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PawnGenerated, Date = anchor, Pinned = true });
    }

    [RequiresRoyalty]
    public void TestRoyalRecordsAreBackdatedInValidOrder(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var title = MakeRecord(HistoryRecordDefOf.TitleGained, pawn, anchor);
        var psylink1 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn, anchor);
        var psylink2 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn, anchor);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn, anchor, pinned: true);
        pawn.HistoryRecords.AddRange([title, psylink1, psylink2, generated]);

        RunWithSeed(12345, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));

        if (title.date + GenDate.TicksPerDay > psylink1.date || psylink1.date + GenDate.TicksPerDay > psylink2.date)
        {
            throw new InvalidOperationException(
                $"Expected title and psylinks to be strictly ordered with day gaps. title={title.date}, psylink1={psylink1.date}, psylink2={psylink2.date}");
        }

        Expect.Assertions(4);
        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PawnGenerated, Date = anchor, Pinned = true });
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.TitleGained, Date = anchor });
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PsylinkLevelGained, Date = anchor }, index: 1);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PsylinkLevelGained, Date = anchor }, index: 2);
    }

    public void TestHealthRecordsUseCooldownAndDensity(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var scar1 = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn, anchor);
        var scar2 = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn, anchor);
        var destroyed = MakeRecord(HistoryRecordDefOf.BodyPartDestroyed, pawn, anchor);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn, anchor, pinned: true);
        pawn.HistoryRecords.AddRange([scar1, scar2, destroyed, generated]);

        RunWithSeed(9876, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));

        var dates = new[] { scar1.date, scar2.date, destroyed.date };
        if (dates.Distinct().Count() != dates.Length)
            throw new InvalidOperationException($"Expected health prehistory records to avoid same-tick clustering. dates={string.Join(", ", dates)}");

        if (scar1.date + GenDate.DaysToTicks(45f) > scar2.date)
            throw new InvalidOperationException($"Expected scar cooldown to be respected. scar1={scar1.date}, scar2={scar2.date}");

        Expect.Assertions(3);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.BodyPartScarred, Date = anchor }, index: 0);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.BodyPartScarred, Date = anchor }, index: 1);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.BodyPartDestroyed, Date = anchor }, index: 2);
    }

    [RequiresBiotech]
    public void TestMechlinkInstalledBackdates(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var mechlink = MakeRecord(HistoryRecordDefOf.MechlinkInstalled, pawn, anchor);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn, anchor, pinned: true);
        pawn.HistoryRecords.AddRange([mechlink, generated]);

        HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated);

        Expect.Assertions(1);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.MechlinkInstalled, Date = anchor });
    }

    [RequiresRoyalty]
    public void TestSameSeedIsDeterministicAndDifferentSeedChangesSpacing(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var title = MakeRecord(HistoryRecordDefOf.TitleGained, pawn, anchor);
        var psylink1 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn, anchor);
        var psylink2 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn, anchor);
        var scar1 = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn, anchor);
        var scar2 = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn, anchor);
        var destroyed = MakeRecord(HistoryRecordDefOf.BodyPartDestroyed, pawn, anchor);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn, anchor, pinned: true);
        var records = new[] { title, psylink1, psylink2, scar1, scar2, destroyed };
        pawn.HistoryRecords.AddRange(records);
        pawn.HistoryRecords.Add(generated);

        RunWithSeed(111, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));
        var firstPass = records.Select(record => record.date).ToArray();

        ResetDates(anchor, records);
        RunWithSeed(111, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));
        var secondPass = records.Select(record => record.date).ToArray();

        ResetDates(anchor, records);
        RunWithSeed(222, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));
        var thirdPass = records.Select(record => record.date).ToArray();

        if (!firstPass.SequenceEqual(secondPass))
            throw new InvalidOperationException($"Expected identical placement for the same seed. first={string.Join(", ", firstPass)} second={string.Join(", ", secondPass)}");

        if (firstPass.SequenceEqual(thirdPass))
            throw new InvalidOperationException($"Expected at least one placement change for a different seed. first={string.Join(", ", firstPass)} third={string.Join(", ", thirdPass)}");
    }

    public void TestRegistryAudit()
    {
        var expected = new[]
        {
            HistoryRecordDefOf.TitleGained,
            HistoryRecordDefOf.PsylinkLevelGained,
            HistoryRecordDefOf.BodyPartScarred,
            HistoryRecordDefOf.BodyPartDestroyed,
            HistoryRecordDefOf.MechlinkInstalled,
        }
        .Where(def => def != null)
        .Select(def => def.defName)
        .OrderBy(defName => defName)
        .ToArray();

        var actual = HistoryBackfillRegistry.ManagedDefs
            .Where(def => def != null)
            .Select(def => def.defName)
            .OrderBy(defName => defName)
            .ToArray();

        if (!expected.SequenceEqual(actual))
            throw new InvalidOperationException($"Expected managed defs [{string.Join(", ", expected)}] but found [{string.Join(", ", actual)}].");
    }

    private static void ResetHistory(Pawn pawn)
    {
        var historyComp = CompHistoryManager.GetComp(pawn);
        if (historyComp == null)
            return;

        historyComp.ClearAll();
        historyComp.PawnGeneratedRecord = null;
    }

    private static void ResetDates(int anchor, params HistoryRecord[] records)
    {
        foreach (var record in records)
            record.date = anchor;
    }

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

    private static HistoryRecord MakeRecord(HistoryRecordDef def, Pawn pawn, int date, bool pinned = false)
    {
        return new HistoryRecord(def, pawn, def.label, date: date, pinned: pinned);
    }
}
