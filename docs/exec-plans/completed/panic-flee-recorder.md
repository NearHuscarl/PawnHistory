# Panic Flee Recorder

When a hostile group loses enough fighters, RimWorld moves its lord into `LordToil_PanicFlee`. That is a group-level story moment: the remaining fighters stop prosecuting the raid and begin fleeing together. Recording the lord-toil transition keeps the history entry tied to the group panic-flee event instead of individual mental-state noise.

## Summary
Handled lord panic flee with a dedicated `PanicFleeRecorder`. The recorder listens to `LordToilChangeEvent`, records only transitions into `LordToil_PanicFlee`, and writes the existing `MentalBreak` history record using the `name==PanicFlee` grammar rule.

## Shipped Scope
- Added a `name==PanicFlee` grammar rule to `PH_MentalBreak` with the description shape: `[PAWN][AndOthers] from [Faction] broke and fled in panic.`
- Added `PanicFleeRecorder` with one recorder-local test that kills raid pawns until RimWorld's auto-flee transition fires.
- Restored `MentalBreakRecorder` to its original flow; panic flee no longer depends on the mental-break recorder.
- Filters dead pawns out of the `WithOthers` group.

## Design
The implementation uses the lord transition as the source of truth. RimWorld changes the raid lord into `LordToil_PanicFlee` after enough violent pawn losses. `PanicFleeRecorder` handles that transition directly, filters the lord pawns to living pawns, and passes that living list to `WithOthers` so dead raiders are not counted as fleeing companions.

The recorder deliberately uses `HistoryRecordDefOf.MentalBreak` and the `PH_MentalBreak` `name==PanicFlee` rule. This avoids adding a new history def while still giving the panic-flee transition its own description.

The test creates a hostile edge-walk-in raid and kills all but two raiders through the normal pawn death path. It asserts that the surviving raiders eventually receive the panic-flee history description. This keeps the test deterministic without publishing directly to `GameEventBus` or calling the transition by hand.

## Exclusions
- Does not add a separate `PanicFlee` history record def; panic flee is stored as `HistoryRecordDefOf.MentalBreak`.
- Does not record direct `MentalStateDefOf.PanicFlee` starts.
- Does not record `PanicFleeFire`.

## Verification
Built with Debug MSBuild after implementation.
