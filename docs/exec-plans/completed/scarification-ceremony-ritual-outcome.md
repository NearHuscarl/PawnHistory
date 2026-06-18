# Scarification Ceremony Ritual Outcome

Scarification ceremonies are Ideology rituals where a cutter marks a target with a ritual scar. The event matters to both pawns: the target receives the permanent mark, and the doer performs the public act. Recording the same ceremony on both histories keeps the story visible from either pawn without inventing separate narratives.

## Summary
Added ScarificationCeremony support to the shared ritual outcome recorder. The implementation follows the BlindingCeremony shape: one ritual outcome record is written for the doer and one for the target, using the shared `RitualOutcome` history definition and reciprocal concerns.

## Shipped Scope
- Records scarification ritual outcomes on both the cutter and the target.
- Adds a ScarificationCeremony-specific rulepack entry: the target was scarified by the doer during the outcome-quality ritual.
- Extends the ritual test builder with a `ScarificationCeremony(...)` path through the real ideogram ritual begin window and outcome application.
- Adds missing `DefOf` references for the scarification ritual, outcome effect, and minor scarification precept used by the test setup.

## Design
`RitualOutcomeComp_Scarification` matches the Ideology `ScarificationCeremony` precept and resolves the existing ritual role ids `doer` and `target`, which the base game ritual behavior defines for scarification.

The existing ritual outcome Harmony patch already covers the ceremony because its outcome effect uses `RitualOutcomeEffectWorker_FromQuality`. No new patch target was required.

## Rules
- The recorder uses the shared `RitualOutcome` record def rather than creating a scarification-specific history record.
- The doer is treated as the ceremony host so common ritual grammar and spectator handling match the pawn leading the ceremony.
- The scarification outcome record is separate from scar hediff recording; this work records the ritual outcome, not the physical scar application path.

## Verification
- Added an Ideology-gated recorder-local test that runs ScarificationCeremony through `RitualBuilder` and asserts doer and target records with reciprocal concerns.
- Built the project successfully with Debug MSBuild.
