# Pawn Generation Timeline Simulator

When RimWorld generates a 40-year-old pawn, it gives them all their traits, scars, and titles at the exact same moment (the moment they spawn). If you look at their history log, it would look like they had a very busy 1.2 seconds of life.

The HistoryBackfillEngine takes those events and "smears" them back across the pawn's biological life so their history looks like a natural narrative.

## Summary
Replaced the old generation-tick backdating logic with a registry-driven backfill engine. Rules now describe what is legally or plausibly true for a record, while the simulator chooses an earlier date relative to the `PawnGenerated` anchor using weighted random sampling.

Phase 1 is generator-only. The simulator only backdates records that are emitted during `PawnGenerator.TryGenerateNewPawnInternal` through the current RimWorld generation flow and this mod's event hooks. It no longer applies a generic "backdate every same-tick non-arrival record" rule.

## Shipped Scope
Managed generator-time defs:
- `TitleGained`
- `PsylinkLevelGained`
- `BodyPartScarred`
- `BodyPartDestroyed`
- `MechlinkInstalled`

Explicitly excluded in phase 1:
- `LeaderChanged`
- `SkillLeveledUp`
- `DrugAddicted`
- `Disease`
- `GrowthMoment`
- pregnancy-related and other non-generator state that does not currently emit matching history records during pawn generation

## Design
`HistoryTimelineSimulator` remains the public entrypoint, but it now delegates to a small backfill module:
- `HistoryBackfillRegistry`
- `HistoryBackfillDefinition`
- `HistoryBackfillContext`
- `PlacementCandidate`
- `TimelineWindow`
- `IHardBackfillRule`
- `IDependencyBackfillRule`
- `ISoftBackfillRule`
- `IGlobalBackfillRule`
- `HistoryBackfillEngine`

The engine:
1. Resolves the true anchor from `CompHistory.PawnGeneratedRecord` when available.
2. Selects only same-anchor-tick records whose defs are explicitly registered.
3. Builds sibling-aware candidates.
4. Builds dependency edges from hard ordering rules and topologically orders the candidates.
5. Samples dates in day buckets before the anchor using `Rand`, age curves, density weighting, and intra-day jitter.
6. Retries placement a bounded number of times and validates hard/global rules after each full pass.
7. Falls back to the latest feasible pre-anchor placement when retries fail.

## Phase 1 Rules
Implemented hard and soft rules for the managed defs:
- `TitleGained`: minimum age 13, maximum count 1, Royalty-only gate, must precede generated psylinks by at least one day, adulthood-weighted age curve.
- `PsylinkLevelGained`: minimum age 13, Royalty-only gate, sibling order with minimum one-day gaps, later-adulthood age curve shifted by sibling index.
- `BodyPartScarred`: minimum age 7, sibling cooldown of 45 days, health density group weighting, rising age curve through adolescence and adulthood.
- `BodyPartDestroyed`: minimum age 7, sibling cooldown of 90 days, shared health density weighting, older-skewed age curve.
- `MechlinkInstalled`: minimum age 13, maximum count 1, Biotech-only gate, adult-skewed age curve.

Global behavior:
- `PawnGenerated` stays pinned at the anchor tick.
- arrival-category records stay at the anchor tick.
- unregistered same-tick records stay unchanged.
- health-prehistory records are biased away from same-day and same-month clustering.
- repeated sibling records use sampled, non-uniform spacing instead of deterministic intervals.

## Verification
Updated recorder-local simulator tests to cover:
- pinned `PawnGenerated`
- arrival records staying at anchor
- unregistered same-tick records staying at anchor
- royal record ordering and non-anchor backdating
- health cooldown and density behavior
- mechlink backdating
- same-seed determinism with different-seed variation
- registry audit for the phase 1 managed-def set

Ran the approved Debug MSBuild build successfully after the simulator and test changes.
