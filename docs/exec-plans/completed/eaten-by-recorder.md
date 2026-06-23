# Eaten By Recorder

Corpse consumption is a final, identity-shaping loss: the pawn is no longer merely dead, their body was eaten. RimWorld already surfaces this for player-notifiable human corpses through `MessageEatenByPredator`, but that notification gate excludes cases that still matter to PawnHistory, such as hostile human corpses and cannibal ingestion.

## Summary
Added a generalized `Eaten` history record that fires when a corpse's core body part is ingested. The record is written for both the eaten pawn and the eater whenever each pawn passes PawnHistory's `ShouldRecord` rule.

This intentionally does not depend on `PawnUtility.ShouldSendNotificationAbout`. PawnHistory uses its own retention rule: all humanlikes and bonded animals can carry records even when RimWorld would not send a player notification.

## Shipped Scope
- Added a typed `EatenEvent` published from `Corpse.IngestedCalculateAmounts` when `numTaken == 1`.
- Added `EatenRecorder` with separate eaten-pawn and eater-pawn POV descriptions.
- Added the `Eaten` record def, `HistoryRecordDefOf` entry, and `PH_Eaten` rulepack.
- Added a recorder-local test that drives the real `Thing.Ingested -> Corpse.IngestedCalculateAmounts` path.

## Design
The hook lives at `Corpse.IngestedCalculateAmounts` because this is where RimWorld chooses the body part consumed and where the vanilla `MessageEatenPredator` notification is emitted. Non-core bites set `numTaken` to `0`; core consumption sets `numTaken` to `1`, so the event only publishes for the final corpse-eating moment.

The recorder uses neutral eater terminology rather than predator terminology so the same record covers animal predation, cannibalism, and any other pawn ingesting a corpse. Concerns are symmetric: the eaten pawn concerns the eater, and the eater concerns the eaten pawn.

## Rules
- Do not call or reproduce `PawnUtility.ShouldSendNotificationAbout`.
- Do not alter `PredatorHuntingColonistRecorder`; it remains the warning/hunting-attempt record.
- Do not add quest, location, backfill, or notification side behavior for this event.

## Verification
- Ran Debug MSBuild successfully with zero warnings and zero errors.
- Added `EatenRecorder.Test` for the in-game debug test runner. It was not run from this shell session because recorder tests execute through RimWorld's debug actions.
