# Tech Debt Tracker

Cross-cutting defects, refactors, and cleanup items.

## Bugs

- Prisoner capture is not recorded when the prisoner is incapacitated and added to a caravan.
- Test multiple maps to verify whether `CasualtyRecorder` uses the correct battle log.
- `RulesForPawn` bug, works: "a elephant" -> "an elephant", doesn't work: "<color>a elephant</color>" -> "<color>a elephant</color>"
- Kill handling should cover turret and animal cases (handle `Thing` rather than `Pawn`).
- Kill record point of view is wrong for the killer.
- Double check to see if `XyzContext`'s state currently lives only in memory instead of through exposable persistence.

## Refactors

- Use `__state` and remove Finalizer/Context for single patch
- Remove `PH_Vars` usage and replace color-tagged string hacks with cleaner rendering or formatting paths.

## Design Pressure

- Decide whether colony-level quest wins such as defeat-all-enemies completion should record on every participant or use a narrower ownership model.
- Decide whether name-change proposal letters belong in history or remain UI-only.
