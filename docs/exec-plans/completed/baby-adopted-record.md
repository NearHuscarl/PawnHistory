# Baby Adopted Record

Implemented a Biotech-gated `BabyAdopted` history record for the vanilla `Designator_Adopt.DesignateThing(...)` path.

## Notes

- Added `BabyAdoptedEvent` with a Harmony patch on the literal adopt designator action.
- The event publishes only when a non-player baby or newborn becomes part of `Faction.OfPlayer`.
- Added `BabyAdoptedRecorder` to write a single history record on the adopted baby only.
- Parent concerns are resolved from direct `PawnRelationDefOf.Parent` relations, preferring mother first and father second.
- The description always uses `[PlayerFaction]` because vanilla adoption does not expose an adopter pawn.
- Added recorder-local Biotech tests for both the parented and parentless adoption cases using the real `Designator_Adopt` path.
