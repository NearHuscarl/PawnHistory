# Miscarried History Records

Human miscarriage is a major life event, but before this change `PawnHistory` stopped at recording pregnancy start. That left a sharp narrative gap for tracked pawns: the mod could say a pawn became pregnant, but not that the pregnancy was later lost or why.

## Summary
Added a new Biotech-only `Miscarried` history record for human pawns. The recorder writes a blunt carrier-only entry when a human pregnancy ends in miscarriage, and it distinguishes between the two base-game miscarriage causes: starvation and poor health.

## Shipped Scope
- Added `MiscarriedEvent`, `MiscarryReason`, and `MiscarriedRecorder`.
- Added the `HistoryRecordDefOf.Miscarried` field and a new `HistoryRecordDef`.
- Added a rulepack that resolves one description from a single `reason` constant.
- Added recorder-local tests for both starvation-driven and poor-health-driven miscarriage records.

## Design
The implementation scopes reason capture to `Hediff_Pregnant.TickInterval()`, where RimWorld decides whether the pregnancy fails. While that root path is active, the mod reads the vanilla miscarriage message text with `MatchesTranslationTemplate(...)`, stores the matching reason, and consumes it in `Hediff_Pregnant.Miscarry()`. This keeps the record aligned with RimWorld's own player-facing cause text without treating the message as the root trigger.

## Rules
- Human pregnancies only.
- Record only on the carrier pawn.
- Respect the existing `ShouldRecord(...)` gate.
- Add no `concerns`.
- Use one history def with a `reason` constant rather than splitting into separate defs.
- Categorize the record under both `Life` and `Health`.

## Exclusions
- No parent-side or partner-side miscarriage records.
- No animal miscarriage records.
- No synthetic `Unknown` miscarriage reason.

## Verification
- Added recorder-local tests covering:
  - starvation
  - poor health
- Tests drive `Hediff_Pregnant.TickInterval(1000)` with deterministic MTB forcing instead of calling `Messages.Message(...)` directly.
- Ran the approved Debug MSBuild build successfully after the source, def, and rulepack changes.
- Recorder tests were added but not executed in-game in this session.
