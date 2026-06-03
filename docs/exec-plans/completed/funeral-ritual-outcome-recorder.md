# Funeral Ritual Outcome Recorder

Funerals are one of the few Ideology rituals centered on a pawn who is no longer alive. A generic ritual outcome entry loses that focus because it can only say that the organizer delivered a ritual outcome. The history entry now records the funeral as being for the dead pawn, so the organizer's history can still point back to the person being mourned.

## Summary
Added funeral-specific support to `RitualOutcomeRecorder`. Funeral outcomes now use a dedicated ritual comp that adds the deceased pawn as the `DeadPawn` grammar rule and as the record concern.

## Shipped Scope
- extended `RitualOutcomeCompletedEvent` with the ritual obligation target pawn
- added `RitualOutcomeComp_Funeral`
- added funeral-specific ritual outcome rulepack text for both `Funeral` and `FuneralNoCorpse`
- added `RitualBuilder.Funeral(...)` for recorder tests
- added recorder-local Ideology tests covering the occupied-grave and no-corpse funeral paths

## Design
The event still follows the existing ritual outcome hook. Funeral data comes from `LordJob_Ritual.obligation.targetA`, which is where the base game stores the pawn who triggered the funeral obligation. The event normalizes either a pawn target or a corpse target into `TargetPawn`.

The funeral comp only matches when the ritual def is `Funeral` or `FuneralNoCorpse` and the target pawn exists. If a funeral-like outcome lacks a target pawn, the recorder falls back to the generic ritual outcome text rather than emitting an unresolved rule.

## Rules
- the history record is written to the funeral host, which is the assigned moralist speaker
- the deceased pawn is a concern, not an additional record recipient
- the funeral text uses a literal "funeral" so generated ritual names do not make the test seed-sensitive
- the standard and no-corpse funeral precepts share the same history text shape

## Verification
- Added `RitualOutcomeComp_Funeral.Test` and `RitualOutcomeComp_Funeral.TestNoCorpse`, gated by Ideology.
- The occupied-grave test creates a funeral precept, assigns the organizer as moralist, buries the deceased pawn, runs the funeral through `RitualBuilder`, and asserts the organizer receives a `RitualOutcome` record with the deceased pawn as the concern.
- The no-corpse test creates the hidden no-corpse funeral precept, destroys the corpse to trigger the real `MemberCorpseDestroyed` obligation, runs the funeral at an empty grave, and asserts the same deceased-pawn concern.
- Ran Debug MSBuild successfully with zero warnings and zero errors.
