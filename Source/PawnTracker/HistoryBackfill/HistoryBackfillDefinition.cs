using System.Collections.Generic;
using System.Linq;

namespace PawnHistory.Source.PawnTracker.HistoryBackfill;

internal sealed class HistoryBackfillDefinition(HistoryRecordDef def, string densityGroup = null)
{
    public HistoryRecordDef Def { get; } = def;
    public string DensityGroup { get; } = densityGroup;
    public List<IHardBackfillRule> HardRules { get; } = [];
    public List<ISoftBackfillRule> SoftRules { get; } = [];
    public List<IGlobalBackfillRule> GlobalRules { get; } = [];

    public HistoryBackfillDefinition AddHard(params IHardBackfillRule[] rules)
    {
        HardRules.AddRange(rules.Where(rule => rule != null));
        return this;
    }

    public HistoryBackfillDefinition AddSoft(params ISoftBackfillRule[] rules)
    {
        SoftRules.AddRange(rules.Where(rule => rule != null));
        return this;
    }

    public HistoryBackfillDefinition AddGlobal(params IGlobalBackfillRule[] rules)
    {
        GlobalRules.AddRange(rules.Where(rule => rule != null));
        return this;
    }
}

internal interface IHardBackfillRule
{
    void ApplyWindow(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, ref TimelineWindow window);
    bool Validate(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state);
}

internal interface IDependencyBackfillRule
{
    IEnumerable<PlacementCandidate> GetSuccessors(HistoryBackfillContext context, PlacementCandidate candidate, IReadOnlyList<PlacementCandidate> candidates);
}

internal interface ISoftBackfillRule
{
    float GetWeight(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, int tick);
}

internal interface IGlobalBackfillRule
{
    float GetWeight(HistoryBackfillContext context, PlacementCandidate candidate, PlacementState state, int tick);
    bool Validate(HistoryBackfillContext context, PlacementState state);
}

internal sealed class PlacementCandidate(HistoryRecord record, HistoryBackfillDefinition definition, int siblingIndex, int originalIndex)
{
    public HistoryRecord Record { get; } = record;
    public HistoryBackfillDefinition Definition { get; } = definition;
    public int SiblingIndex { get; } = siblingIndex;
    public int OriginalIndex { get; } = originalIndex;

    public override string ToString() => $"{Record.def.defName}#{SiblingIndex}@{OriginalIndex}";
}

internal sealed class PlacementState(IReadOnlyList<PlacementCandidate> candidates)
{
    private readonly Dictionary<PlacementCandidate, int> placements = [];

    public IReadOnlyList<PlacementCandidate> Candidates { get; } = candidates;
    public IEnumerable<KeyValuePair<PlacementCandidate, int>> Placements => placements;

    public void Place(PlacementCandidate candidate, int tick)
    {
        placements[candidate] = tick;
    }

    public bool Remove(PlacementCandidate candidate)
    {
        return placements.Remove(candidate);
    }

    public bool TryGetPlacement(PlacementCandidate candidate, out int tick)
    {
        return placements.TryGetValue(candidate, out tick);
    }

    public int? GetPlacement(PlacementCandidate candidate)
    {
        return placements.TryGetValue(candidate, out var tick) ? tick : null;
    }

    public IEnumerable<KeyValuePair<PlacementCandidate, int>> GetPlacementsForDefinition(HistoryRecordDef def)
    {
        return placements.Where(pair => pair.Key.Record.def == def);
    }

    public IEnumerable<KeyValuePair<PlacementCandidate, int>> GetPlacementsForDensityGroup(string densityGroup)
    {
        return placements.Where(pair => pair.Key.Definition.DensityGroup == densityGroup);
    }

}

internal record HistoryBackfillPlacementResult(IReadOnlyDictionary<PlacementCandidate, int> Placements, IReadOnlyList<PlacementCandidate> AnchoredCandidates);
