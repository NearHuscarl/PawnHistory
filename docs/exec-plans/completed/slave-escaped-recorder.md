# Slave Escaped Recorder

Slave rebellions and slave escape attempts are important turning points in a pawn's story. They are high-signal, player-readable moments that explain both a slave's resistance and the group dynamics around it, so they belong in history alongside prison breaks.

## Summary
Added `SlaveRebellionRecorder` and its event pipeline using the same architectural shape as `PrisonBreakRecorder`:
- one typed event
- one static context
- two publish paths
- one recorder that dispatches into natural-start and sparked-start record builders

The shipped behavior records on each escaping slave, distinguishes rebellion versus escape, and derives single/local/grand-style wording from the participant list through a normalized `n` constant.

## Shipped Scope
- Added `SlaveRebellionEvent`, `SlaveEscapeReason`, and `SlaveRebellionContext`.
- Hooked the natural `SlaveRebellionUtility.StartSlaveRebellion(...)` path.
- Hooked the interaction-driven `SparkSlaveRebellion` path through `Pawn_InteractionsTracker.TryInteractWith()` and `PlayLog.Add()`.
- Added `SlaveRebellionRecorder`, `HistoryRecordDefOf.SlaveRebellion`, the Ideology-gated history def, and the Ideology-gated rulepack.
- Added recorder-local tests that cover all 12 permutations of:
  - trigger route: natural or sparked
  - action: rebellion or escape
  - presentation branch: `n=1`, `n<all`, `n=all`

## Design
The implementation intentionally follows `PrisonBreakRecorder` instead of introducing a broader abstraction:
- the event payload carries the initiator, escaping slaves, trigger reason, escape/rebellion flag, trigger-time slave count, and optional interaction log text
- the static context bridges the multi-step interaction path so the recorder sees one normalized event
- the recorder splits into:
  - natural start: initiator gets the "started" variant, other slaves get the "joined" variant
  - sparked start: each escaping slave gets the shared "as a result" variant

To support the `n` rulepack constant correctly, the patch captures the eligible slave count before RimWorld mutates the rebellion state. `n` then resolves as:
- `1`
- `<all`
- `all`

## Test Support
The recorder tests are deterministic and recorder-local. They build a small fixed pawn set, call `CreateRecord(...)` directly, and assert the resolved history output inline for each permutation. That keeps the test surface focused on recorder behavior and avoids random seed search or brittle world setup.

## Verification
- Added recorder-local assertions for all 12 route/action/`n` permutations.
- Built the mod successfully with the approved Debug MSBuild command.
- Did not execute the in-game recorder test suite in this session because the repo does not expose a standalone non-game runner for the test DSL.
