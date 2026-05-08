# Slave Emancipated Recorder

Slave emancipation is a real status change, not just the absence of slavery. Whether a warden formally frees someone or the colony releases a born slave at childhood, the pawn's role in the colony changes in a way players can read as part of that pawn's story.

## Summary

Added an Ideology-gated `SlaveEmancipated` history record that covers both supported emancipation paths with one def and one rulepack. Warden-driven emancipation writes a mirrored record on both the emancipated pawn and the warden, while child emancipation writes on the child only because the colony choice has no named pawn actor.

## Shipped Scope

- Added `SlaveEmancipatedEvent` with `Warden` and `BabyToChild` causes.
- Patched `GenGuest.EmancipateSlave(...)` to record successful warden emancipation.
- Patched the `ChoiceLetter_BabyToChild` "Emancipate" option to record born-slave emancipation on growth up.
- Added `SlaveEmancipatedRecorder`, `HistoryRecordDefOf.SlaveEmancipated`, and the Ideology-only `PH_SlaveEmancipated` rulepack.
- Added two recorder-local tests for the two supported emancipation paths.

## Design

- The implementation mirrors `EnslavedRecorder`: one def, one recorder, and cause-based rulepack branching.
- Warden emancipation uses a prefix/postfix pair on `GenGuest.EmancipateSlave(...)` so the event only publishes when the pawn really transitioned from slave to non-slave.
- The baby-to-child path stays tied to the letter option the player actually clicks. That keeps the recorder aligned with player intent and avoids catching unrelated `SlaveRelease(...)` calls.

## Rules

- Use one history def for all slave emancipation, with `cause` selecting the final text.
- Record on both the emancipated pawn and the warden when a warden performs emancipation.
- Record only on the child for baby-to-child emancipation because there is no named emancipator pawn.
- Do not publish an emancipation event unless the pawn actually stops being a slave.

## Verification

- Added one `[RequiresIdeology]` recorder-local test that:
  - creates a colony slave
  - sets the slave interaction mode to `Emancipate`
  - starts a colonist on the real `JobDefOf.SlaveEmancipation` path
  - asserts mirrored `SlaveEmancipated` records on the warden and the freed pawn
- Added one `[RequiresBiotech]` + `[RequiresIdeology]` recorder-local test that:
  - creates a born slave child with a forced birthday
  - executes the real baby-to-child letter path with the "Emancipate" choice
  - asserts the child receives the `SlaveEmancipated` record with the colony wording
- Planned to run the approved Debug `MSBuild` build after the implementation changes.
