# Enslaved Recorder

Prisoner enslavement is a distinct ideology-era turning point. Once a prisoner becomes a slave, the player-facing story is no longer just "a warden talked to them" but that the pawn was forced into a new role inside the colony. That change belongs in history for both people directly involved.

## Summary

Added an Ideology-gated `Enslaved` history record for successful `EnslaveAttempt` interactions. The recorder only fires when the prisoner actually becomes a slave, writes one shared description on both the new slave and the enslaving colonist, and replaces vanilla's recruit-acceptance tail with repo-owned past-tense wording.

## Shipped Scope

- Added `EnslavedEvent`.
- Patched the single `PlayLog.Add(...)` interaction hook and detected success from the pawn already being a slave by that point in the call flow.
- Added `EnslavedRecorder`, `HistoryRecordDefOf.Enslaved`, and an Ideology-only `PH_Enslaved` rulepack.
- Added one recorder-local Ideology test that drives the real `JobDefOf.PrisonerEnslave` path.

## Design

- The patch mirrors the existing `PrisonerRecruitedEvent` pattern instead of introducing extra transient state.
- Success detection stays narrow: the play-log patch only publishes when the `EnslaveAttempt` recipient is already `IsSlaveOfColony`.
- Publication still uses the matching `PlayLogEntry_Interaction` so the recorder can reuse the real in-game lead sentence before appending a past-tense outcome sentence.

## Rules

- Only successful prisoner-to-slave conversions record history.
- Reduce-will `EnslaveAttempt` interactions do not record anything.
- The slave's record concerns the enslaver.
- The enslaver's record concerns the slave.
- The final history text keeps the first vanilla interaction sentence and appends `[Slave] was enslaved by [Enslaver].`

## Verification

- Added one `[RequiresIdeology]` recorder-local test that:
  - creates a prison room and prisoner
  - forces `prisoner.guest.will = 0f`
  - sets the prisoner's interaction mode to `Enslave`
  - starts a colonist on the real `PrisonerEnslave` job
  - asserts that both pawns receive the `Enslaved` record with mirrored concerns and the expected description template
- Planned to run the approved Debug `MSBuild` build after the implementation changes.
