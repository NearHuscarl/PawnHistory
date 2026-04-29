# Psylink Level Gained Recorder

Implemented a Royalty-gated `PsylinkLevelGainedRecorder` that records one history entry per psylink level gain.

## Notes

- The Harmony hook is `Hediff_Psylink.TryGiveAbilityOfLevel(...)`, not `MakeLetterTextNewPsylinkLevel(...)`, to avoid duplicate records during bestowing letter composition.
- The event payload carries the gaining pawn, the new psylink level, and the newly learned psycast when one was added by that level-up.
- The record text has two main cases: first psylink gained and later psylink level gained.
- If a level gain does not add a new psycast, the record is still written and omits the ability clause.
- Added two recorder-local Royalty tests covering first psylink gain and a later level gain.
