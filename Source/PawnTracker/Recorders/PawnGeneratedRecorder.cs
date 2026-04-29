using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Linq;
using PawnHistory.Source.PawnTracker.HistoryBackfill;
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

        HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated);

        Expect.That(scar.date).Not().Equal(anchor);
        Expect.That(scar.date).ToBeLessThan(generated.date);
        Expect.That(generated.date).Equal(arrival.date);
        Expect.That(pawn.HistoryRecords).SequenceEqual([scar, arrival, generated]);
        Expect.That(generated.pinned).ToBeTrue();
    }

    // Unregistered same tick records stay at anchor
    public void TestUnregistered(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        ResetHistory(pawn);

        var leaderChanged = MakeRecord(HistoryRecordDefOf.LeaderChanged, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        pawn.HistoryRecords.AddRange([leaderChanged, generated]);

        HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated);

        Expect.That(pawn.HistoryRecords).SequenceEqual([leaderChanged, generated]);
        Expect.That(leaderChanged.date).Equal(generated.date);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestRoyalRecordsAreBackdatedInValidOrder(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var title = MakeRecord(HistoryRecordDefOf.TitleGained, pawn);
        var psylink1 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn);
        var psylink2 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        pawn.HistoryRecords.AddRange([title, psylink1, psylink2, generated]);

        RunWithSeed(12345, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));

        if (title.date + GenDate.TicksPerDay > psylink1.date || psylink1.date + GenDate.TicksPerDay > psylink2.date)
        {
            throw new InvalidOperationException(
                $"Expected title and psylinks to be strictly ordered with day gaps. title={title.date}, psylink1={psylink1.date}, psylink2={psylink2.date}");
        }

        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PawnGenerated, Date = anchor, Pinned = true });
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.TitleGained, Date = anchor });
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PsylinkLevelGained, Date = anchor }, index: 1);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PsylinkLevelGained, Date = anchor }, index: 2);
    }

    [SkipTest]
    public void TestHealthRecordsUseCooldownAndDensity(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var scar1 = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn);
        var scar2 = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn);
        var destroyed = MakeRecord(HistoryRecordDefOf.BodyPartDestroyed, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        pawn.HistoryRecords.AddRange([scar1, scar2, destroyed, generated]);

        RunWithSeed(9876, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));

        var dates = new[] { scar1.date, scar2.date, destroyed.date };
        if (dates.Distinct().Count() != dates.Length)
            throw new InvalidOperationException($"Expected health prehistory records to avoid same-tick clustering. dates={string.Join(", ", dates)}");

        if (scar1.date + GenDate.DaysToTicks(45f) > scar2.date)
            throw new InvalidOperationException($"Expected scar cooldown to be respected. scar1={scar1.date}, scar2={scar2.date}");

        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.BodyPartScarred, Date = anchor }, index: 0);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.BodyPartScarred, Date = anchor }, index: 1);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.BodyPartDestroyed, Date = anchor }, index: 2);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestOldPawnUsesConstantBudgetBackfillSampling(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        MakePawnBiologicallyOld(pawn, 80);
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var title = MakeRecord(HistoryRecordDefOf.TitleGained, pawn);
        var psylink1 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn);
        var psylink2 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn);
        var scar = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        pawn.HistoryRecords.AddRange([title, psylink1, psylink2, scar, generated]);

        RunWithSeed(20260429, () => HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated));

        if (title.date + GenDate.TicksPerDay > psylink1.date || psylink1.date + GenDate.TicksPerDay > psylink2.date)
        {
            throw new InvalidOperationException(
                $"Expected constant-budget placement to preserve royal ordering. title={title.date}, psylink1={psylink1.date}, psylink2={psylink2.date}");
        }

        var titleGapDays = (psylink1.date - title.date) / (float)GenDate.TicksPerDay;
        if (titleGapDays is < 3f or > 30f)
            throw new InvalidOperationException($"Expected title to stay reasonably close to the earliest psylink. gapDays={titleGapDays}");

        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.TitleGained, Date = anchor });
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PsylinkLevelGained, Date = anchor }, index: 1);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.PsylinkLevelGained, Date = anchor }, index: 2);
        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.BodyPartScarred, Date = anchor }, index: 3);
    }

    [SkipTest]
    [RequiresBiotech]
    public void TestMechlinkInstalledBackdates(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var mechlink = MakeRecord(HistoryRecordDefOf.MechlinkInstalled, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        pawn.HistoryRecords.AddRange([mechlink, generated]);

        HistoryTimelineSimulator.ProcessPawnGenerated(pawn, generated);

        Expect.That(pawn).Not().ToHaveHistoryRecord(new ExpectedHistoryRecord { Def = HistoryRecordDefOf.MechlinkInstalled, Date = anchor });
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestSameSeedIsDeterministicAndDifferentSeedChangesSpacing(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var anchor = GenTicks.TicksAbs;
        ResetHistory(pawn);

        var title = MakeRecord(HistoryRecordDefOf.TitleGained, pawn);
        var psylink1 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn);
        var psylink2 = MakeRecord(HistoryRecordDefOf.PsylinkLevelGained, pawn);
        var scar1 = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn);
        var scar2 = MakeRecord(HistoryRecordDefOf.BodyPartScarred, pawn);
        var destroyed = MakeRecord(HistoryRecordDefOf.BodyPartDestroyed, pawn);
        var generated = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
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

    [SkipTest]
    public void TestConstantBudgetSamplingCostIsBounded(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        MakePawnBiologicallyOld(pawn, 80);
        var anchor = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        var counter = new CountingSoftRule();
        var definition = new HistoryBackfillDefinition(HistoryRecordDefOf.LeaderChanged).AddSoft(counter);
        var candidate = CreateCandidate(HistoryRecordDefOf.LeaderChanged, pawn, definition, 0, 0);

        var result = ResolveSyntheticPlacements(pawn, anchor, candidate);
        var placedTick = result.Placements[candidate];

        if (placedTick >= anchor.date)
            throw new InvalidOperationException($"Expected bounded-cost candidate to backfill before anchor. placed={placedTick} anchor={anchor.date}");

        if (counter.EvaluationCount > 135)
            throw new InvalidOperationException($"Expected constant-budget sampling to stay bounded, but weight evaluations reached {counter.EvaluationCount}.");
    }

    [SkipTest]
    public void TestNarrowGapDependencyStaysExactForOldPawn(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        MakePawnBiologicallyOld(pawn, 80);
        var anchor = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        var preferredTick = anchor.date - GenDate.DaysToTicks(5f);

        var earlierDefinition = new HistoryBackfillDefinition(HistoryRecordDefOf.LeaderChanged)
            .AddHard(new OrderBeforeRule(GenDate.DaysToTicks(3f), HistoryRecordDefOf.SkillLeveledUp))
            .AddSoft(new PreferredTickSoftRule(preferredTick));
        var laterDefinition = new HistoryBackfillDefinition(HistoryRecordDefOf.SkillLeveledUp)
            .AddSoft(new PreferredTickSoftRule(preferredTick));

        var earlier = CreateCandidate(HistoryRecordDefOf.LeaderChanged, pawn, earlierDefinition, 0, 0);
        var later = CreateCandidate(HistoryRecordDefOf.SkillLeveledUp, pawn, laterDefinition, 0, 1);

        var result = ResolveSyntheticPlacements(pawn, anchor, earlier, later);
        var earlierTick = result.Placements[earlier];
        var laterTick = result.Placements[later];

        if (earlierTick + GenDate.DaysToTicks(3f) > laterTick)
        {
            throw new InvalidOperationException(
                $"Expected exact three-day ordering clamp to be respected. earlier={earlierTick}, later={laterTick}");
        }
    }

    public void TestCircularDependenciesStayAtAnchorWhileIndependentCandidateBackfills(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        MakePawnBiologicallyOld(pawn, 80);
        var anchor = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        var preferredTick = anchor.date - GenDate.DaysToTicks(20f);

        var firstDefinition = new HistoryBackfillDefinition(HistoryRecordDefOf.LeaderChanged)
            .AddHard(new OrderBeforeRule(GenDate.TicksPerDay, HistoryRecordDefOf.SkillLeveledUp));
        var secondDefinition = new HistoryBackfillDefinition(HistoryRecordDefOf.SkillLeveledUp)
            .AddHard(new OrderBeforeRule(GenDate.TicksPerDay, HistoryRecordDefOf.LeaderChanged));
        var independentDefinition = new HistoryBackfillDefinition(HistoryRecordDefOf.Birthday)
            .AddSoft(new PreferredTickSoftRule(preferredTick));

        var first = CreateCandidate(HistoryRecordDefOf.LeaderChanged, pawn, firstDefinition, 0, 0);
        var second = CreateCandidate(HistoryRecordDefOf.SkillLeveledUp, pawn, secondDefinition, 0, 1);
        var independent = CreateCandidate(HistoryRecordDefOf.Birthday, pawn, independentDefinition, 0, 2);

        var result = ResolveSyntheticPlacements(pawn, anchor, first, second, independent);
        if (result.Placements[first] != anchor.date)
            throw new InvalidOperationException($"Expected cyclic candidate A to stay at anchor. actual={result.Placements[first]} anchor={anchor.date}");
        if (result.Placements[second] != anchor.date)
            throw new InvalidOperationException($"Expected cyclic candidate B to stay at anchor. actual={result.Placements[second]} anchor={anchor.date}");
        if (result.Placements[independent] >= anchor.date)
            throw new InvalidOperationException($"Expected independent candidate to backfill before anchor. actual={result.Placements[independent]} anchor={anchor.date}");
    }

    [SkipTest]
    public void TestInvalidWindowLeavesOnlyThatCandidateAtAnchor(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        MakePawnBiologicallyOld(pawn, 80);
        var anchor = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        var preferredTick = anchor.date - GenDate.DaysToTicks(30f);

        var invalidDefinition = new HistoryBackfillDefinition(HistoryRecordDefOf.LeaderChanged)
            .AddHard(new LogicalGateRule((_, _) => false));
        var validDefinition = new HistoryBackfillDefinition(HistoryRecordDefOf.SkillLeveledUp)
            .AddSoft(new PreferredTickSoftRule(preferredTick));

        var invalid = CreateCandidate(HistoryRecordDefOf.LeaderChanged, pawn, invalidDefinition, 0, 0);
        var valid = CreateCandidate(HistoryRecordDefOf.SkillLeveledUp, pawn, validDefinition, 0, 1);

        var result = ResolveSyntheticPlacements(pawn, anchor, invalid, valid);
        if (result.Placements[invalid] != anchor.date)
            throw new InvalidOperationException($"Expected invalid-window candidate to stay at anchor. actual={result.Placements[invalid]} anchor={anchor.date}");
        if (result.Placements[valid] >= anchor.date)
            throw new InvalidOperationException($"Expected valid candidate to still backfill before anchor. actual={result.Placements[valid]} anchor={anchor.date}");
    }

    [SkipTest]
    public void TestDeterministicFallbackUsesWeightedPlacementInsteadOfLatestTick(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        MakePawnBiologicallyOld(pawn, 80);
        var anchor = MakeRecord(HistoryRecordDefOf.PawnGenerated, pawn);
        var preferredTick = anchor.date - 30 * GenDate.TicksPerYear;

        var invalidDefinition = new HistoryBackfillDefinition(HistoryRecordDefOf.LeaderChanged)
            .AddGlobal(new AlwaysInvalidGlobalRule());
        var weightedDefinition = new HistoryBackfillDefinition(HistoryRecordDefOf.SkillLeveledUp)
            .AddSoft(new PreferredTickSoftRule(preferredTick));

        var invalid = CreateCandidate(HistoryRecordDefOf.LeaderChanged, pawn, invalidDefinition, 0, 0);
        var weighted = CreateCandidate(HistoryRecordDefOf.SkillLeveledUp, pawn, weightedDefinition, 0, 1);

        var result = ResolveSyntheticPlacements(pawn, anchor, invalid, weighted);
        var weightedTick = result.Placements[weighted];
        if (result.Placements[invalid] != anchor.date)
            throw new InvalidOperationException($"Expected invalid candidate to stay at anchor during fallback. actual={result.Placements[invalid]} anchor={anchor.date}");
        if (weightedTick >= anchor.date - GenDate.TicksPerYear)
            throw new InvalidOperationException($"Expected weighted fallback placement to land meaningfully before anchor. weighted={weightedTick} anchor={anchor.date}");

        if (weightedTick == anchor.date - 1)
            throw new InvalidOperationException($"Expected deterministic fallback to use weighted placement instead of the raw latest tick. weighted={weightedTick}");
    }

    private static void ResetHistory(Pawn pawn) => CompHistoryManager.GetComp(pawn).ClearAll();

    private static void ResetDates(int anchor, params HistoryRecord[] records)
    {
        foreach (var record in records)
            record.date = anchor;
    }

    private static void MakePawnBiologicallyOld(Pawn pawn, int biologicalYears)
    {
        pawn.ageTracker.AgeBiologicalTicks = biologicalYears * GenDate.TicksPerYear;
    }

    private static HistoryBackfillPlacementResult ResolveSyntheticPlacements(Pawn pawn, HistoryRecord anchor, params PlacementCandidate[] candidates)
    {
        var allRecords = candidates.Select(candidate => candidate.Record).Append(anchor).ToList();
        var context = new HistoryBackfillContext(pawn, anchor, allRecords);
        return HistoryBackfillEngine.ResolvePlacementsForTesting(context, candidates);
    }

    private static PlacementCandidate CreateCandidate(
        HistoryRecordDef def,
        Pawn pawn,
        HistoryBackfillDefinition definition,
        int siblingIndex,
        int originalIndex)
    {
        var record = MakeRecord(def, pawn);
        return new PlacementCandidate(record, definition, siblingIndex, originalIndex);
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

    private static HistoryRecord MakeRecord(HistoryRecordDef def, Pawn pawn)
    {
        return new HistoryRecord(def, pawn, def.label);
    }

    private sealed class CountingSoftRule : ISoftBackfillRule
    {
        public int EvaluationCount { get; private set; }

        public float GetWeight(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, int tick)
        {
            EvaluationCount++;
            return 1f;
        }
    }

    private sealed class PreferredTickSoftRule(int preferredTick) : ISoftBackfillRule
    {
        public float GetWeight(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, int tick)
        {
            var distanceDays = Math.Abs(tick - preferredTick) / (float)GenDate.TicksPerDay;
            return 1f / (1f + distanceDays);
        }
    }

    private sealed class AlwaysInvalidGlobalRule : IGlobalBackfillRule
    {
        public float GetWeight(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, int tick)
        {
            return 1f;
        }

        public bool Validate(HistoryBackfillContext context, PlacementState state)
        {
            return false;
        }
    }
}
