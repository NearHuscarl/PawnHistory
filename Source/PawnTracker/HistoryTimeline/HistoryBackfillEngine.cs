using PawnHistory.Source.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker;

internal static class HistoryBackfillEngine
{
    public static void BackdateGeneratorRecords(Pawn pawn, HistoryRecord anchorRecord)
    {
        var context = new HistoryBackfillContext(pawn, anchorRecord, pawn.HistoryRecords.ToList());

        var candidates = BuildCandidates(context);
        if (candidates.Count == 0)
            return;

        var orderedCandidates = TopologicalOrder(context, candidates);
        if (!TryPlaceWithRetries(context, orderedCandidates, out var placements))
            placements = BuildFallbackPlacements(context, orderedCandidates);

        foreach (var pair in placements)
            pair.Key.Record.date = pair.Value;
    }

    private static List<PlacementCandidate> BuildCandidates(HistoryBackfillContext context)
    {
        var candidates = new List<PlacementCandidate>();
        var siblingIndexes = new Dictionary<HistoryRecordDef, int>();

        foreach (var record in context.AllRecords)
        {
            if (record == context.AnchorRecord || record.date != context.AnchorTick)
                continue;

            if (!HistoryBackfillRegistry.TryGetDefinition(record.def, out var definition))
                continue;

            var siblingIndex = siblingIndexes.GetValueOrDefault(record.def, 0);
            siblingIndexes[record.def] = siblingIndex + 1;
            candidates.Add(new PlacementCandidate(record, definition, siblingIndex, candidates.Count));
        }

        return candidates;
    }

    private static List<PlacementCandidate> TopologicalOrder(HistoryBackfillContext context, List<PlacementCandidate> candidates)
    {
        var edges = candidates.ToDictionary(candidate => candidate, _ => new HashSet<PlacementCandidate>());
        var indegree = candidates.ToDictionary(candidate => candidate, _ => 0);

        foreach (var candidate in candidates)
        {
            foreach (var rule in candidate.Definition.HardRules.OfType<IDependencyBackfillRule>())
            {
                foreach (var successor in rule.GetSuccessors(context, candidate, candidates))
                {
                    if (successor == candidate)
                        continue;

                    if (!edges[candidate].Add(successor))
                        continue;

                    indegree[successor]++;
                }
            }
        }

        var queue = new Queue<PlacementCandidate>(candidates.Where(candidate => indegree[candidate] == 0).OrderBy(candidate => candidate.OriginalIndex));
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

        if (ordered.Count == candidates.Count)
            return ordered;

        return candidates.OrderBy(candidate => candidate.OriginalIndex).ToList();
    }

    private static bool TryPlaceWithRetries(HistoryBackfillContext context, List<PlacementCandidate> orderedCandidates, out Dictionary<PlacementCandidate, int> placements)
    {
        for (var attempt = 0; attempt < HistoryBackfillContext.MaxPlacementAttempts; attempt++)
        {
            if (TryPlace(context, orderedCandidates, randomize: true, out placements))
                return true;
        }

        placements = null;
        return false;
    }

    private static Dictionary<PlacementCandidate, int> BuildFallbackPlacements(HistoryBackfillContext context, List<PlacementCandidate> orderedCandidates)
    {
        if (TryPlace(context, orderedCandidates, randomize: false, out var placements))
            return placements;

        var state = new PlacementState(orderedCandidates);
        foreach (var candidate in orderedCandidates.AsEnumerable().Reverse())
        {
            var window = BuildWindow(context, candidate, state);
            if (window.IsValid)
            {
                state.Place(candidate, window.LatestTick);
                continue;
            }

            if (window.EarliestTick >= context.AnchorTick)
            {
                state.Place(candidate, context.AnchorTick);
                continue;
            }

            state.Place(candidate, Math.Min(context.AnchorTick - 1, Math.Max(window.EarliestTick, context.BirthTick)));
        }

        return state.Placements.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private static bool TryPlace(HistoryBackfillContext context, List<PlacementCandidate> orderedCandidates, bool randomize, out Dictionary<PlacementCandidate, int> placements)
    {
        var state = new PlacementState(orderedCandidates);

        foreach (var candidate in orderedCandidates.AsEnumerable().Reverse())
        {
            var window = BuildWindow(context, candidate, state);
            if (!window.IsValid)
            {
                if (window.EarliestTick >= context.AnchorTick)
                {
                    state.Place(candidate, context.AnchorTick);
                    continue;
                }

                placements = null;
                return false;
            }

            if (!TrySampleTick(context, candidate, state, window, randomize, out var tick))
            {
                placements = null;
                return false;
            }

            state.Place(candidate, tick);
        }

        if (!Validate(context, orderedCandidates, state))
        {
            placements = null;
            return false;
        }

        placements = state.Placements.ToDictionary(pair => pair.Key, pair => pair.Value);
        return true;
    }

    private static TimelineWindow BuildWindow(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state)
    {
        var window = new TimelineWindow(context.BirthTick, context.AnchorTick - 1);

        foreach (var rule in candidate.Definition.HardRules)
            rule.ApplyWindow(context, candidate, state, ref window);

        return window;
    }

    private static bool TrySampleTick(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, TimelineWindow window, bool randomize, out int tick)
    {
        var buckets = BuildDayBuckets(window);
        if (buckets.Count == 0)
        {
            tick = 0;
            return false;
        }

        var weightedBuckets = new List<WeightedBucket>(buckets.Count);
        foreach (var bucket in buckets)
        {
            var sampleTick = bucket.SampleTick(randomize);
            var weight = GetWeight(context, candidate, state, sampleTick);
            weightedBuckets.Add(new WeightedBucket(bucket, sampleTick, weight));
        }

        if (randomize)
        {
            var totalWeight = weightedBuckets.Sum(bucket => Math.Max(bucket.Weight, 0f));
            if (totalWeight > 0f)
            {
                var pick = Rand.Value * totalWeight;
                foreach (var bucket in weightedBuckets)
                {
                    pick -= Math.Max(bucket.Weight, 0f);
                    if (pick > 0f)
                        continue;

                    tick = bucket.SampleTick;
                    return true;
                }
            }
        }

        var selectedBucket = weightedBuckets
            .OrderByDescending(bucket => bucket.Weight)
            .ThenByDescending(bucket => bucket.Bucket.LatestTick)
            .FirstOrDefault();

        if (selectedBucket.Bucket == null)
        {
            tick = 0;
            return false;
        }

        tick = randomize ? selectedBucket.Bucket.SampleTick(true) : selectedBucket.Bucket.LatestTick;
        return true;
    }

    private static float GetWeight(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, int tick)
    {
        var weight = 1f;

        foreach (var rule in candidate.Definition.SoftRules)
            weight *= Math.Max(rule.GetWeight(context, candidate, state, tick), 0f);

        foreach (var rule in candidate.Definition.GlobalRules)
            weight *= Math.Max(rule.GetWeight(context, candidate, state, tick), 0f);

        return weight;
    }

    private static bool Validate(HistoryBackfillContext context, IReadOnlyList<PlacementCandidate> candidates, PlacementState state)
    {
        foreach (var candidate in candidates)
        {
            foreach (var rule in candidate.Definition.HardRules)
            {
                if (!rule.Validate(context, candidate, state))
                    return false;
            }
        }

        foreach (var rule in candidates.SelectMany(candidate => candidate.Definition.GlobalRules))
        {
            if (!rule.Validate(context, state))
                return false;
        }

        return true;
    }

    private static List<DayBucket> BuildDayBuckets(TimelineWindow window)
    {
        var buckets = new List<DayBucket>();
        var firstDay = FloorToDay(window.EarliestTick);
        var lastDay = FloorToDay(window.LatestTick);

        for (var day = firstDay; day <= lastDay; day++)
        {
            var dayStart = checked(day * GenDate.TicksPerDay);
            var dayEnd = dayStart + GenDate.TicksPerDay - 1;
            var bucket = new DayBucket(Math.Max(window.EarliestTick, dayStart), Math.Min(window.LatestTick, dayEnd));
            if (bucket.IsValid)
                buckets.Add(bucket);
        }

        return buckets;
    }

    private static int FloorToDay(int tick)
    {
        return (int)Math.Floor(tick / (double)GenDate.TicksPerDay);
    }

    private readonly record struct WeightedBucket(DayBucket Bucket, int SampleTick, float Weight);

    private readonly record struct DayBucket(int EarliestTick, int LatestTick)
    {
        public bool IsValid => EarliestTick <= LatestTick;

        public int SampleTick(bool randomize)
        {
            return randomize && EarliestTick < LatestTick
                ? Rand.RangeInclusive(EarliestTick, LatestTick)
                : LatestTick;
        }
    }
}
