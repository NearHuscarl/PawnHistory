# Burn And Smoke Circle Ritual Outcome Tests

Burn circles and smoke circles are Ideology consumable date rituals. The player-facing history value is the communal ritual outcome: every participant took part in a memorable ritual around a sacrificial target, while the consumed building is setup context rather than the subject of a pawn history record.

## Summary

Added deterministic recorder coverage for BurnCircle and SmokeCircle ritual outcomes through the existing festival-style `RitualOutcome` path. Both rituals now have test helpers that start the real ritual dialog and outcome flow, and both tests assert every participant receives an attendee-form history record.

## Shipped Scope

- Added `Extra.RitualPatternDefOf.BurnCircle` and `Extra.RitualPatternDefOf.SmokeCircle`.
- Added `Extra.ThingDefOf.Effigy` and `Extra.ThingDefOf.Burnbong` for recorder-test setup.
- Added `RitualBuilder.BurnCircle(...)` and `RitualBuilder.SmokeCircle(...)`.
- Added recorder-local tests in `RitualOutcomeComp_Festival`.
- Documented the new ritual builder helpers.

Explicitly excluded:
- separate BurnCircle or SmokeCircle ritual outcome comps
- ritual outcome event changes
- rulepack changes
- attaching the consumed building as a concern

## Design

BurnCircle and SmokeCircle are `RitualPatternDef`s filled into the `DateRitualConsumable` precept. They are not standalone precepts. The tests therefore create a `DateRitualConsumable` precept filled with the relevant pattern, then select the ritual by `Precept_Ritual.sourcePattern` through the existing pattern-based ritual builder path.

The existing `RitualOutcomeComp_Festival` remains the correct recorder owner because `DateRitualConsumable` already records all participants with generic attendee wording. The new coverage confirms that behavior for the two consumable circle variants without adding new runtime semantics.

## Rules

- Organizer and joiners receive the same `RitualOutcome` attendee record.
- The record description remains `[PAWN] attended an unforgettable [Ritual] with 2 others.`
- The consumed effigy or burnbong is not attached to pawn history.

## Verification

- Added in-game recorder tests for BurnCircle and SmokeCircle through the real ritual begin-dialog and outcome path.
- Built with `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`; build succeeded with 0 warnings and 0 errors.
- Did not run the RimWorld in-game test harness from this shell session.
