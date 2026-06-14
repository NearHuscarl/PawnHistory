# Raider Left Recorder

Raiders leaving can be as story-relevant as raiders arriving: the departure tells whether the raid lost its nerve, felt it had done enough damage, or stopped being hostile because relations changed. This recorder captures those explicit vanilla raid-leaving messages as pawn history records without folding in individual panic fleeing.

## Summary

`RaiderLeftRecorder` records one `RaidersLeft` history entry for each recordable raider when a raid lord transitions into `LordToil_ExitMap` through one of the three vanilla message reasons:

- relationship changed
- raiders gave up after time passed
- raiders were satisfied after colony damage

The implementation reuses `LordToilChangeEvent`, which already exposes the current toil, next toil, trigger, signal, and lord before the transition executes.

## Shipped Scope

- Added `RaidersLeft` history def and `PH_RaidersLeft` rulepack text.
- Added `HistoryRecordDefOf.RaidersLeft`.
- Added `RaiderLeftRecorder` with reason mapping from:
  - `Trigger_BecameNonHostileToPlayer`
  - `Trigger_TicksPassed`
  - `Trigger_FractionColonyDamageTaken`
- Added exactly three recorder tests, one for each leave reason.

## Design

The recorder filters to raid lord jobs and exit-map transitions so visitor, caravan, and other lord exits do not create raider records. The recorder does not inspect message text directly; it relies on the same trigger/toil combinations that vanilla uses to send `MessageRaidersLeaving`, `MessageRaidersGivenUpLeaving`, and `MessageRaidersSatisfiedLeaving`.

The test-only helper methods live inside the recorder. They mock the existing vanilla trigger state so the normal `Transition.CheckSignal` path still publishes the existing Harmony event naturally.

## Exclusions

PanicFlee is intentionally excluded. It is a broader faction auto-flee behavior reused outside raids and should get separate treatment if recorded later.

## Verification

Added in-game tests cover:

- relationship-changed leaving
- timeout/given-up leaving
- damage-satisfied leaving

Verified the Debug build with MSBuild and parsed the edited XML defs. The in-game tests require the RimWorld test runner, so they were added but not run from this shell.
