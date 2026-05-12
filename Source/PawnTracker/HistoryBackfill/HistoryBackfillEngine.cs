using PawnHistory.Source.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.DebugTools;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.HistoryBackfill;

internal static class HistoryBackfillEngine
{
    private const int MaxRandomPlacementAttempts = 2;
    private const int ExactDayScanThreshold = 90;
    private const int LargeWindowProbeCount = 16;
    private const int LocalRefinementRadiusDays = 14;

    private static HistoryBackfillContext Context
    {
        get => field ?? throw new ArgumentNullException(nameof(Context));
        set;
    }

    public static void BackdateGeneratorRecords(Pawn pawn, HistoryRecord anchorRecord)
    {
        Context = new HistoryBackfillContext(pawn, anchorRecord, pawn.HistoryRecords.ToList());
        var candidates = BuildCandidates();
        if (candidates.Count == 0)
            return;

        var result = ResolvePlacements(candidates, logWarnings: true);
        foreach (var pair in result.Placements)
            pair.Key.Record.date = pair.Value;

        var invalidRecords = result.AnchoredCandidates.Select(c => c.Record).ToHashSet();
        
        L.Debug($"Cannot find a valid date for the following records: {DebugUtility.FormatSequence(invalidRecords)}. They will be removed from HistoryRecords.");
        
        pawn.HistoryRecords.RemoveWhere(invalidRecords.Contains);
        pawn.HistoryRecords.Sort((a, b) => a.date.CompareTo(b.date));
    }

    private static HistoryBackfillPlacementResult ResolvePlacements(IReadOnlyList<PlacementCandidate> candidates, bool logWarnings)
    {
        var dependencyOrder = BuildDependencyOrder(candidates);
        var anchoredCandidates = new HashSet<PlacementCandidate>(dependencyOrder.UnorderedCandidates);
        var placeableCandidates = dependencyOrder.OrderedCandidates
            .Where(candidate => !anchoredCandidates.Contains(candidate))
            .ToList();

        PruneBaseInvalidCandidates(placeableCandidates, anchoredCandidates);

        var resolvedPlacements = new Dictionary<PlacementCandidate, int>(placeableCandidates.Count);
        if (placeableCandidates.Count > 0)
        {
            if (!TryPlaceWithRetries(placeableCandidates, out resolvedPlacements, out var retryAnchoredCandidates))
            {
                if (!TryPlace(placeableCandidates, randomize: false, anchoredCandidates, out resolvedPlacements))
                    resolvedPlacements = [];
            }
            else
                anchoredCandidates.UnionWith(retryAnchoredCandidates);
        }

        var finalPlacements = new Dictionary<PlacementCandidate, int>(candidates.Count);
        foreach (var candidate in candidates)
        {
            if (resolvedPlacements.TryGetValue(candidate, out var tick))
            {
                finalPlacements[candidate] = tick;
                continue;
            }

            finalPlacements[candidate] = Context.AnchorTick;
            anchoredCandidates.Add(candidate);
        }

        var anchoredList = candidates.Where(anchoredCandidates.Contains).ToList();
        if (logWarnings && anchoredList.Count > 0)
            LogUnsatisfiedBatch(anchoredList);

        return new HistoryBackfillPlacementResult(finalPlacements, anchoredList);
    }

    private static List<PlacementCandidate> BuildCandidates()
    {
        var candidates = new List<PlacementCandidate>();
        var siblingIndexes = new Dictionary<HistoryRecordDef, int>();

        foreach (var record in Context.AllRecords)
        {
            if (record == Context.AnchorRecord || record.date != Context.AnchorTick)
                continue;

            if (!HistoryBackfillRegistry.TryGetDefinition(record.def, out var definition))
                continue;

            var siblingIndex = siblingIndexes.GetValueOrDefault(record.def, 0);
            siblingIndexes[record.def] = siblingIndex + 1;
            candidates.Add(new PlacementCandidate(record, definition, siblingIndex, candidates.Count));
        }

        return candidates;
    }

    private static DependencyOrder BuildDependencyOrder(IReadOnlyList<PlacementCandidate> candidates)
    {
        var edges = candidates.ToDictionary(candidate => candidate, _ => new HashSet<PlacementCandidate>());
        var indegree = candidates.ToDictionary(candidate => candidate, _ => 0);

        foreach (var candidate in candidates)
        {
            foreach (var rule in candidate.Definition.HardRules.OfType<IDependencyBackfillRule>())
            {
                foreach (var successor in rule.GetSuccessors(Context, candidate, candidates))
                {
                    if (successor == candidate)
                        continue;

                    if (!edges[candidate].Add(successor))
                        continue;

                    indegree[successor]++;
                }
            }
        }

        var queue = new Queue<PlacementCandidate>(candidates
            .Where(candidate => indegree[candidate] == 0)
            .OrderBy(candidate => candidate.OriginalIndex));
        var ordered = new List<PlacementCandidate>(candidates.Count);

        while (queue.Count > 0)
        {
            var candidate = queue.Dequeue();
            ordered.Add(candidate);

            foreach (var successor in edges[candidate].OrderBy(next => next.OriginalIndex))
            {
                indegree[successor]--;
                if (indegree[successor] == 0)
                    queue.Enqueue(successor);
            }
        }

        var unordered = candidates
            .Where(candidate => !ordered.Contains(candidate))
            .OrderBy(candidate => candidate.OriginalIndex)
            .ToList();

        return new DependencyOrder(ordered, unordered);
    }

    private static void PruneBaseInvalidCandidates(List<PlacementCandidate> placeableCandidates, ISet<PlacementCandidate> anchoredCandidates)
    {
        if (placeableCandidates.Count == 0)
            return;

        var emptyState = new PlacementState(placeableCandidates);
        for (var i = placeableCandidates.Count - 1; i >= 0; i--)
        {
            var candidate = placeableCandidates[i];
            if (BuildWindow(candidate, emptyState).IsValid)
                continue;

            anchoredCandidates.Add(candidate);
            placeableCandidates.RemoveAt(i);
        }
    }

    private static bool TryPlaceWithRetries(
        List<PlacementCandidate> orderedCandidates,
        out Dictionary<PlacementCandidate, int> placements,
        out HashSet<PlacementCandidate> anchoredCandidates)
    {
        for (var attempt = 0; attempt < MaxRandomPlacementAttempts; attempt++)
        {
            var localAnchoredCandidates = new HashSet<PlacementCandidate>();
            if (!TryPlace(orderedCandidates, randomize: true, localAnchoredCandidates, out placements))
                continue;

            anchoredCandidates = localAnchoredCandidates;
            return true;
        }

        placements = null;
        anchoredCandidates = null;
        return false;
    }

    private static bool TryPlace(
        List<PlacementCandidate> orderedCandidates,
        bool randomize,
        ISet<PlacementCandidate> anchoredCandidates,
        out Dictionary<PlacementCandidate, int> placements)
    {
        var state = new PlacementState(orderedCandidates);

        foreach (var candidate in orderedCandidates.AsEnumerable().Reverse())
        {
            var window = BuildWindow(candidate, state);
            if (!window.IsValid || !TrySampleTick(candidate, state, window, randomize, out var tick))
            {
                anchoredCandidates.Add(candidate);
                continue;
            }

            state.Place(candidate, tick);
            if (ValidatePlacedState(state))
                continue;

            state.Remove(candidate);
            anchoredCandidates.Add(candidate);
        }

        if (!ValidatePlacedState(state))
        {
            placements = null;
            return false;
        }

        placements = state.Placements.ToDictionary(pair => pair.Key, pair => pair.Value);
        return true;
    }

    private static TimelineWindow BuildWindow(PlacementCandidate candidate, PlacementState state)
    {
        var window = new TimelineWindow(Context.BirthTick, Context.AnchorTick - 1);

        foreach (var rule in candidate.Definition.HardRules)
            rule.ApplyWindow(Context, candidate, state, ref window);

        return window;
    }

    private static bool TrySampleTick(PlacementCandidate candidate, PlacementState state, TimelineWindow window, bool randomize, out int tick)
    {
        var dayCount = CountDays(window);
        if (dayCount <= 0)
        {
            tick = 0;
            return false;
        }

        if (dayCount <= ExactDayScanThreshold)
            return TrySampleTickExact(candidate, state, window, randomize, out tick);

        return TrySampleTickLargeWindow(candidate, state, window, randomize, out tick);
    }

    private static bool TrySampleTickLargeWindow(PlacementCandidate candidate, PlacementState state, TimelineWindow window, bool randomize, out int tick)
    {
        if (!TrySelectSampledProbe(candidate, state, window, randomize, out var focusTick))
        {
            tick = 0;
            return false;
        }

        var radiusTicks = GenDate.DaysToTicks(LocalRefinementRadiusDays);
        var focusedWindow = window.ShrinkTo(focusTick - radiusTicks, focusTick + radiusTicks);
        return TrySampleTickExact(candidate, state, focusedWindow, randomize, out tick);
    }

    private static bool TrySampleTickExact(PlacementCandidate candidate, PlacementState state, TimelineWindow window, bool randomize, out int tick)
    {
        var firstDay = FloorToDay(window.EarliestTick);
        var lastDay = FloorToDay(window.LatestTick);
        if (firstDay > lastDay)
        {
            tick = 0;
            return false;
        }

        var weightedWindows = new List<WeightedWindow>(lastDay - firstDay + 1);
        for (var day = firstDay; day <= lastDay; day++)
        {
            var startTick = checked(day * GenDate.TicksPerDay);
            var endTick = checked(day * GenDate.TicksPerDay + (GenDate.TicksPerDay - 1));
            var bucket = window.ShrinkTo(startTick, endTick);
            weightedWindows.Add(new WeightedWindow(bucket, GetWeight(candidate, state, bucket.RepresentativeTick())));
        }

        if (weightedWindows.Count == 0)
        {
            tick = 0;
            return false;
        }

        if (randomize && weightedWindows.TryRandomElementByWeight(w => w.Weight, out var weightedWindow))
        {
            tick = weightedWindow.Window.SampleTick(randomize: true);
            return true;
        }

        var bestWindow = weightedWindows
            .OrderByDescending(w => w.Weight)
            .ThenByDescending(w => w.Window.LatestTick)
            .First();

        var bestBucket = bestWindow.Window;
        tick = randomize ? bestBucket.SampleTick(randomize: true) : bestBucket.LatestTick;
        return true;
    }

    private static bool TrySelectSampledProbe(PlacementCandidate candidate, PlacementState state, TimelineWindow window, bool randomize, out int selectedTick)
    {
        var probeTicks = BuildProbeTicks(window, randomize);
        if (probeTicks.Count == 0)
        {
            selectedTick = 0;
            return false;
        }

        var weightedProbes = probeTicks
            .Select(probeTick => new WeightedProbe(probeTick, GetWeight(candidate, state, probeTick)))
            .ToList();

        if (randomize && weightedProbes.TryRandomElementByWeight(w => w.Weight, out var weightedProbe))
        {
            selectedTick = weightedProbe.Tick;
            return true;
        }

        var bestProbe = weightedProbes
            .OrderByDescending(w => w.Weight)
            .ThenByDescending(w => w.Tick)
            .FirstOrDefault();

        selectedTick = bestProbe.Tick;
        return true;
    }

    private static float GetWeight(PlacementCandidate candidate, PlacementState state, int tick)
    {
        var weight = 1f;

        foreach (var rule in candidate.Definition.SoftRules)
            weight *= Math.Max(rule.GetWeight(Context, candidate, state, tick), 0f);

        foreach (var rule in candidate.Definition.GlobalRules)
            weight *= Math.Max(rule.GetWeight(Context, candidate, state, tick), 0f);

        return weight;
    }

    private static bool ValidatePlacedState(PlacementState state)
    {
        var placedCandidates = state.Placements.Select(pair => pair.Key).ToList();
        return Validate(placedCandidates, state);
    }

    private static bool Validate(IReadOnlyList<PlacementCandidate> candidates, PlacementState state)
    {
        foreach (var candidate in candidates)
        {
            foreach (var rule in candidate.Definition.HardRules)
            {
                if (!rule.Validate(Context, candidate, state))
                    return false;
            }
        }

        foreach (var rule in candidates.SelectMany(candidate => candidate.Definition.GlobalRules))
        {
            if (!rule.Validate(Context, state))
                return false;
        }

        return true;
    }

    private static void LogUnsatisfiedBatch(IReadOnlyList<PlacementCandidate> anchoredCandidates)
    {
        var defs = anchoredCandidates
            .Select(candidate => candidate.Record.def?.defName ?? "<null>")
            .Distinct()
            .OrderBy(defName => defName);

        Log.Warning($"[PawnHistory] History backfill left generator records at anchor for {Context.Pawn.LabelShortCap}: {string.Join(", ", defs)}");
    }

    private static int CountDays(TimelineWindow window)
    {
        if (!window.IsValid)
            return 0;

        return FloorToDay(window.LatestTick) - FloorToDay(window.EarliestTick) + 1;
    }

    private static List<int> BuildProbeTicks(TimelineWindow window, bool randomize)
    {
        if (!window.IsValid)
            return [];

        var probes = new HashSet<int>
        {
            window.EarliestTick,
            window.EarliestTick + (window.LatestTick - window.EarliestTick) / 2,
            window.LatestTick,
        };

        var span = (long)window.LatestTick - window.EarliestTick;
        var interiorProbeCount = LargeWindowProbeCount - probes.Count;
        for (var i = 0; i < interiorProbeCount; i++)
        {
            var sliceStart = window.EarliestTick + HistoryBackfillContext.ClampToInt(i * (span + 1L) / Math.Max(1, interiorProbeCount));
            var sliceExclusiveEnd = window.EarliestTick + HistoryBackfillContext.ClampToInt((i + 1L) * (span + 1L) / Math.Max(1, interiorProbeCount));
            var sliceEnd = Math.Min(window.LatestTick, Math.Max(sliceStart, sliceExclusiveEnd - 1));
            var probeTick = randomize && sliceStart < sliceEnd
                ? Rand.RangeInclusive(sliceStart, sliceEnd)
                : sliceStart + (sliceEnd - sliceStart) / 2;
            probes.Add(probeTick);
        }

        return probes.OrderBy(tick => tick).ToList();
    }

    private static int FloorToDay(int tick) => (int)Math.Floor(tick / (double)GenDate.TicksPerDay);

    private readonly record struct WeightedProbe(int Tick, float Weight);
    private readonly record struct WeightedWindow(TimelineWindow Window, float Weight);

    private record DependencyOrder(IReadOnlyList<PlacementCandidate> OrderedCandidates, IReadOnlyList<PlacementCandidate> UnorderedCandidates);
}

internal static class TimelineWindowExtensions
{
    public static int RepresentativeTick(this TimelineWindow window)
    {
        return window.EarliestTick + (window.LatestTick - window.EarliestTick) / 2;
    }

    public static int SampleTick(this TimelineWindow window, bool randomize)
    {
        return randomize && window.EarliestTick < window.LatestTick
            ? Rand.RangeInclusive(window.EarliestTick, window.LatestTick)
            : window.LatestTick;
    }
}
