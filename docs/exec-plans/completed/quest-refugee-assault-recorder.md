# Quest Refugee Assault Recorder

When hospitality refugees turn hostile, the history should belong to the refugees themselves rather than an external political actor. The important player-facing story is that the sheltered group became assailants, and why that assault happened.

## Summary

Added a Royalty-gated `QuestRefugeeAssault` history record driven from `QuestPart_RefugeeInteractions.AssaultColony(HistoryEventDef reason)`. The implementation classifies every current assault trigger into an explicit local reason enum, writes the record on every refugee pawn, and keeps the visible hospitality quest attached to the record.

## Shipped Scope

- Added `QuestRefugeeAssaultEvent` and `QuestRefugeeAssaultReason`.
- Patched the narrow `AssaultColony(...)` entrypoint instead of the broader signal router.
- Added `QuestRefugeeAssaultRecorder`, a new `HistoryRecordDef`, and a dedicated world rulepack.
- Phase 1 verification covers the `reason == null` betrayal path only.

Phase 1 does not add recorder-local tests for:
- `Death`
- `Arrested`
- `SurgeryViolation`
- `PsychicRitualTarget`

## Design

- The patch snapshots the refugee list and parent quest before vanilla mutates faction and lord state.
- `HistoryEventDef` is normalized into a recorder-facing enum:
  - `null` -> `Betrayal`
  - `QuestPawnLost` -> `Death`
  - `QuestPawnArrested` -> `Arrested`
  - `PerformedHarmfulSurgery` -> `SurgeryViolation`
  - `WasPsychicRitualTarget` -> `PsychicRitualTarget`
- The recorder writes one record per refugee and attaches the full refugee group as concerns.
- The record stays attached to the parent `Hospitality_Refugee` quest rather than any downstream hostility mechanics.

## Rules

- Record ownership is the full refugee group.
- The concern list contains all refugees, including the current record owner, so the history entry keeps the entire assault context together.
- The betrayal branch is identified strictly by `reason == null`.
- Description text varies by normalized assault reason, even though only betrayal is verified in phase 1.

## Verification

- Added one deterministic Royalty recorder-local test that:
  - executes a real `Hospitality_Refugee` quest
  - resolves the quest's `QuestPart_RefugeeInteractions`
  - invokes the real `AssaultColony(null)` transition through `Accessor`
  - asserts that all refugees receive `QuestRefugeeAssault`
  - asserts the description template, concerns, and attached quest
- Planned to run the approved Debug `MSBuild` build after the implementation changes.
