# Scar Healed Recorder

RimWorld already heals old permanent wounds through `TryHealRandomPermanentWound`, but before this change `PawnHistory` never wrote that recovery into a pawn's history. That left luciferium and scarless-gene recovery visible in health state but absent from the narrative timeline.

## Summary
Added a `ScarHealed` history record that fires from `HediffComp_HealPermanentWounds.TryHealRandomPermanentWound`. The event carries the pawn, the healed scar hediff, the healed part, and the raw `string cause` provided by vanilla. The recorder writes a single pawn-facing recovery entry using that cause directly.

## Shipped Scope
- Added `ScarHealedEvent` and a single Harmony patch on `TryHealRandomPermanentWound`.
- Added `ScarHealedRecorder`.
- Added `HistoryRecordDefOf.ScarHealed`, the XML record def, and the matching rule pack.
- Added two recorder-local tests covering luciferium and scarless-gene healing.

## Design
The implementation is intentionally narrow:
1. Snapshot the pawn's permanent injury scars before `TryHealRandomPermanentWound` runs.
2. After the vanilla call, find the scar that was removed.
3. Publish `ScarHealedEvent` with the removed scar and the vanilla `cause` string.
4. Record only healed permanent injury scars with body parts.

This avoids caller-specific context tracking and keeps the patch on the literal game entrypoint that performs the healing.

## Rules
- Only healed permanent injury scars are recorded.
- Chronic diseases and other non-scar permanent conditions are ignored.
- The record text uses the vanilla `cause` string directly instead of inventing a separate reason enum.
- The record is written only for pawns that pass the normal `ShouldRecord(...)` filter.

## Verification
- Built the project successfully with the approved Debug MSBuild command.
- Added recorder-local tests for:
  - luciferium healing
  - scarless-gene healing
