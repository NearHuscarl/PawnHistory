# Executed Recorder

Colony execution is one semantic event in this mod even when RimWorld reaches it through different job drivers. The victim's status still matters for player-facing history, but that distinction belongs in the event route and existing rule inputs, not in parallel event and recorder types.

## Summary

Normalized colony execution recording behind one `ExecutedEvent` and one `ExecutedRecorder`. The recorder now covers prisoner execution, guilty colonist execution, and slave execution through a single semantic flow, while still writing separate `PrisonerExecuted` and `SlaveExecuted` history records and keeping the shared `PH_Executed` rulepack on the existing `route` and `guilty` constants.

## Shipped Scope

- Added `ExecutedEvent` for prisoner, guilty colonist, and slave executions.
- Added `ExecutedRecorder` that dispatches to `PrisonerExecuted` or `SlaveExecuted`.
- Added the Ideology-only `SlaveExecuted` history def.
- Renamed `PH_PrisonerExecuted` to `PH_Executed` and reused it for both execution defs.
- Removed the split execution-specific event and recorder pair.
- Added one recorder-local Ideology test for slave execution.

## Design

- Kept separate record defs for prisoner and slave execution because the player-facing history distinction still matters.
- Normalized the event surface into a single `ExecutedEvent` instead of parallel event types.
- Preserved the existing rulepack contract by keeping `route` and `guilty` on the event payload rather than inventing new constants.
- Reused one context helper for all three job drivers so publication still happens immediately before the real execution and preserves record order relative to the death record.

## Rules

- `PrisonerExecuted` and `SlaveExecuted` remain separate history defs.
- Both records use the same `PH_Executed` rulepack.
- `ExecutedEvent` is the only execution event published by these colony execution drivers.
- `route` and `guilty` carry the wording choice for prisoner, guilty colonist, and slave cases.
- The recorder writes mirrored concerns on victim and executioner, and the event record lands before the pawn's death record.

## Verification

- Added one `[RequiresIdeology]` recorder-local slave test that:
  - creates a colony slave
  - sets the slave interaction mode to `Execute`
  - starts a colonist on the real `SlaveExecution` job
  - asserts mirrored `SlaveExecuted` records on the warden and the slave
  - asserts the slave still receives the trailing `Death` record
- Kept recorder-local tests for guilty prisoner, innocent prisoner, and guilty colonist execution paths under the unified recorder.
- Ran the approved Debug `MSBuild` build successfully after the implementation changes.
