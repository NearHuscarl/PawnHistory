# Psychic Bond Recorder

Implemented a Biotech-gated psychic bond history record for the vanilla `InteractionWorker_RomanceAttempt.TryCreatePsychicBondBetween(Pawn, Pawn)` path.

## Changes

- Added `PsychicBondedEvent` with a Harmony postfix on the vanilla psychic bond method.
- Added `PsychicBondedRecorder` to write symmetric history records for both pawns when each pawn passes `ShouldRecord(...)`.
- Added `PsychicBonded` history-def wiring and a relationship rule pack entry.
- Added one Biotech recorder test that grants `GeneDefOf.PsychicBonding`, calls the real vanilla method, and asserts the record on both pawns.

## Notes

- No backfill updates were required because the event is interaction-driven, not generation-driven.
- The record text is intentionally symmetric: `[PAWN] and [BondedPawn] formed a psychic bond.`
