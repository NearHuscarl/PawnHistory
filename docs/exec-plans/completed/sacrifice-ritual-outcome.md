# Sacrifice Ritual Outcome

Sacrifice rituals are player-facing ideology events where the important history is not that a pawn attended a generic festival, but that one pawn killed another pawn or bonded animal as the ritual victim. The recorder needs to preserve that relationship from both sides: the executioner should remember the victim, and the victim should remember the executioner when the victim is a pawn worth tracking.

## Summary

Added a sacrifice-specific ritual outcome comp that records sacrifice outcomes separately from generic festival attendance. The comp records only the executioner and the victim, gives each recorded pawn the other pawn as the concern, and leaves ordinary spectators unrecorded.

Sacrifice is identified by the ritual outcome effect def `Sacrifice`, not by the precept def. This matters because the base-game sacrifice patterns fill the generic `Festival` precept, so matching only on ritual/precept would collapse sacrifices into festival attendance.

## Shipped Scope

Implemented support for the base-game sacrifice variants:
- `SacrificePrisoner`
- `SacrificeAnimal`

Variant audit result: there are 2 base-game sacrifice ritual-pattern variants, prisoner and animal. No third sacrifice pattern was found in the installed Ideology definitions.

## Design

`RitualOutcomeCompletedEvent` now carries `OutcomeEffectDef` alongside the ritual precept def. `RitualOutcomeRecorder` exposes that value to grammar as `outcomeEffect`, allowing rulepack entries to target `outcomeEffect==Sacrifice`.

The sacrifice behavior lives in `RitualOutcomeComp_Sacrifice` rather than a separate recorder. It uses the shared ritual outcome recorder pipeline, because the event source, grammar builder, spectator grammar, and record creation rules are the same as other ritual outcome histories. The comp only changes matching, record pawns, concerns, and role-specific grammar.

`RitualOutcomeComp_Festival` explicitly excludes the `Sacrifice` outcome effect so a sacrifice-filled festival does not also produce generic festival attendance records.

## Rules

Sacrifice records use one shared description:

`[Victim] was sacrificed by [Executioner] during [Outcome_indefinite] [Ritual][InFrontOfOthers].`

Recorded pawns:
- executioner
- victim

Concerns:
- executioner record concerns the victim
- victim record concerns the executioner

The implementation keeps the normal `ShouldRecord` filtering. Prisoner victims are recorded as pawns. Animal victims are recorded only when they satisfy the existing pawn-history significance rules, such as being bonded; the animal test creates a bonded animal to verify the intended two-sided record without bypassing those rules.

## Test Coverage

Added recorder-local tests for both shipped variants:
- prisoner sacrifice
- animal sacrifice

The tests force a sacrifice outcome, assert the executioner and victim records, verify the reciprocal concern pawn, and verify spectators do not receive a ritual outcome record.

## Verification

Ran the approved Debug MSBuild build after implementation. The in-game recorder tests were added but were not executed from the shell; they remain available through the project's existing test harness.
