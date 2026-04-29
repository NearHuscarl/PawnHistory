using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

internal sealed class MinimumAgeRule(float minimumAgeYears) : IHardBackfillRule
{
    private readonly int minimumAgeTicks = Mathf.RoundToInt(minimumAgeYears * GenDate.TicksPerYear);

    public void ApplyWindow(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, ref TimelineWindow window)
    {
        window.ClampEarliest(HistoryBackfillContext.ClampToInt(context.BirthAbsTicks + minimumAgeTicks));
    }

    public bool Validate(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state)
    {
        var tick = state.GetPlacement(candidate) ?? candidate.Record.date;
        return tick == context.AnchorTick || tick >= HistoryBackfillContext.ClampToInt(context.BirthAbsTicks + minimumAgeTicks);
    }
}

internal sealed class MaximumCountRule(int maxCount) : IHardBackfillRule
{
    public void ApplyWindow(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, ref TimelineWindow window)
    {
    }

    public bool Validate(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state)
    {
        return state.CountCandidatesForDefinition(candidate.Record.def) <= maxCount;
    }
}

internal sealed class LogicalGateRule(Func<HistoryBackfillContext, PlacementCandidate, bool> predicate) : IHardBackfillRule
{
    public void ApplyWindow(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, ref TimelineWindow window)
    {
        if (predicate(context, candidate))
            return;

        window.ClampEarliest(context.AnchorTick);
    }

    public bool Validate(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state) => predicate(context, candidate);
}

internal sealed class OrderBeforeRule(int minimumGapTicks, params HistoryRecordDef[] laterDefinitions) : IHardBackfillRule, IDependencyBackfillRule
{
    private readonly HashSet<HistoryRecordDef> laterDefinitions = laterDefinitions.ToHashSet();

    public IEnumerable<PlacementCandidate> GetSuccessors(HistoryBackfillContext context, PlacementCandidate candidate, IReadOnlyList<PlacementCandidate> candidates)
    {
        return candidates.Where(other => other != candidate && laterDefinitions.Contains(other.Record.def));
    }

    public void ApplyWindow(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, ref TimelineWindow window)
    {
        foreach (var pair in state.Placements)
        {
            if (!laterDefinitions.Contains(pair.Key.Record.def))
                continue;

            window.ClampLatest(pair.Value - minimumGapTicks);
        }
    }

    public bool Validate(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state)
    {
        if (!state.TryGetPlacement(candidate, out var tick))
            return false;

        foreach (var pair in state.Placements)
        {
            if (!laterDefinitions.Contains(pair.Key.Record.def))
                continue;

            if (pair.Key == candidate)
                continue;

            if (tick + minimumGapTicks > pair.Value)
                return false;
        }

        return true;
    }
}

internal sealed class SiblingSequenceRule(int minimumGapTicks) : IHardBackfillRule, IDependencyBackfillRule
{
    public IEnumerable<PlacementCandidate> GetSuccessors(HistoryBackfillContext context, PlacementCandidate candidate, IReadOnlyList<PlacementCandidate> candidates)
    {
        var nextSibling = candidates
            .Where(other => other.Record.def == candidate.Record.def && other.SiblingIndex == candidate.SiblingIndex + 1)
            .OrderBy(other => other.OriginalIndex)
            .FirstOrDefault();

        if (nextSibling != null)
            yield return nextSibling;
    }

    public void ApplyWindow(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, ref TimelineWindow window)
    {
        var nextSibling = state.Candidates
            .FirstOrDefault(other => other.Record.def == candidate.Record.def && other.SiblingIndex == candidate.SiblingIndex + 1);

        if (nextSibling == null || !state.TryGetPlacement(nextSibling, out var nextTick))
            return;

        window.ClampLatest(nextTick - minimumGapTicks);
    }

    public bool Validate(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state)
    {
        if (!state.TryGetPlacement(candidate, out var tick))
            return false;

        var nextSibling = state.Candidates
            .FirstOrDefault(other => other.Record.def == candidate.Record.def && other.SiblingIndex == candidate.SiblingIndex + 1);

        if (nextSibling == null || !state.TryGetPlacement(nextSibling, out var nextTick))
            return true;

        return tick + minimumGapTicks <= nextTick;
    }
}

internal sealed class AgeCurveSoftRule(SimpleCurve curve) : ISoftBackfillRule
{
    public float GetWeight(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, int tick)
    {
        var age = context.BiologicalAgeAt(tick);
        return Mathf.Max(curve.Evaluate(age), 0.001f);
    }
}

internal sealed class ShiftedAgeCurveSoftRule(SimpleCurve curve, float yearsPerSibling) : ISoftBackfillRule
{
    public float GetWeight(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, int tick)
    {
        var age = context.BiologicalAgeAt(tick) - candidate.SiblingIndex * yearsPerSibling;
        return Mathf.Max(curve.Evaluate(age), 0.001f);
    }
}

internal sealed class DensityGlobalRule(string densityGroup) : IGlobalBackfillRule
{
    public float GetWeight(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, int tick)
    {
        if (candidate.Definition.DensityGroup != densityGroup)
            return 1f;

        var weight = 1f;
        foreach (var pair in state.GetPlacementsForDensityGroup(densityGroup))
        {
            if (pair.Key == candidate)
                continue;

            var gapDays = Mathf.Abs(tick - pair.Value) / (float)GenDate.TicksPerDay;
            if (gapDays < 1f)
                weight *= 0.02f;
            else if (gapDays < 7f)
                weight *= 0.12f;
            else if (gapDays < 30f)
                weight *= 0.35f;
            else if (gapDays < 90f)
                weight *= 0.75f;
        }

        return Mathf.Max(weight, 0.001f);
    }

    public bool Validate(HistoryBackfillContext context, PlacementState state)
    {
        var duplicateTicks = state.GetPlacementsForDensityGroup(densityGroup)
            .GroupBy(pair => pair.Value)
            .Any(group => group.Count() > 1);

        return !duplicateTicks;
    }
}
