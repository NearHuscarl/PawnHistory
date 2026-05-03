# Refugee Pod Crash Baby Arrival Description

Biotech's baby transport-pod crash is a joiner-style arrival, but it carries a distinct story beat that the generic quest-arrival rules could not express: sometimes the pod also contains the baby's deceased parent. Capturing that difference keeps the history log aligned with what the player actually saw in the quest letter and on the map.

## Summary
`QuestPawnArrived` now covers `RefugeePodCrash_Baby` through a dedicated quest-arrival comp. The recorder writes a baby-specific arrival description by default and upgrades to a second description when the same pod also contains a deceased parent. The record still belongs only to the baby, and the parent-present branch adds the corpse as a concern. The test coverage now uses fixed seeds around the real quest path instead of a test-only quest patch.

## Shipped Scope
- Added a new `QuestPawnArrivedComp` for `RefugeePodCrash_Baby`.
- Added Biotech `QuestScriptDefOf` coverage in `Extra.cs`.
- Added rulepack entries for the default and deceased-parent arrival variants.
- Added recorder-local Biotech tests for the live quest path, using fixed seeds to exercise the two vanilla branches.

## Design
The implementation stays inside the existing `QuestPawnArrivedRecorder` extension point instead of introducing a separate event or recorder. This quest is still a normal quest-pawn arrival; it only needs extra interpretation of the `QuestPart_DropPods` contents.

The comp inspects the quest's drop-pod payload, looks for a corpse whose inner pawn matches the baby's mother or father, and exposes that result in two ways:
- `hasDeadParent` grammar constant for rulepack selection
- corpse concern when present

This keeps the event model unchanged and localizes the quest-specific behavior to the same comp mechanism already used by other quest-arrival variants.

For tests, the implementation now keeps the quest flow fully vanilla. Each recorder test wraps `scenario.Quest(...).Execute()` in a fixed `Rand.PushState(seed)` / `Rand.PopState()` block and then checks the actual drop-pod contents before asserting the history record. This removes the custom quest mock entirely at the cost of depending on seed stability across upstream quest-generation changes.

## Rules
- Default description: `A baby named [PAWN] from [Faction] crashed nearby in a transport pod.`
- Parent-present description: `A baby named [PAWN] from [Faction] crashed nearby in a transport pod. [His] deceased parent's body was in the pod as well.`
- No adoption language appears in this arrival record.
- No parent history record is created; the arrival remains baby-owned only.
- No pawn-generation backfill behavior changes, because this is a live arrival event rather than pre-arrival history.

## Verification
- Added a seed-scoped Biotech test for the no-parent branch through the real `RefugeePodCrash_Baby` quest path.
- Added a seed-scoped Biotech test for the parent-corpse branch through the same real quest path.
- Built the project in Debug configuration after the change.
