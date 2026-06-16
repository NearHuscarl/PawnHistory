# Rebuff Recorder

Rejected romance attempts are small social turns, but they matter because they explain later mood, distance, and relationship history between two pawns. The recorder captures the moment from RimWorld's real romance interaction path so a pawn's history can show both the attempt and the rejection without relying on debug-only or tale-only signals.

## Summary
Added a `Rebuff` history record for failed `RomanceAttempt` interactions. The record is written for both participants, and each record concerns the other pawn in the interaction.

The description starts with the same interaction-log sentence used by the base game, then adds a generated history-style scene using `PH_Rebuff`.

## Shipped Scope
- Added a typed `RebuffEvent` published from `PlayLog.Add` when a romance attempt includes `Sentence_RomanceAttemptRejected`.
- Added `RebuffRecorder` with one-day repeat suppression per pawn and reciprocal concern links.
- Added `HistoryRecordDefOf.Rebuff`, the `Rebuff` XML def, and the `PH_Rebuff` relationship rulepack.
- Added deterministic test support through `NearDebugSettings.ForceRomanceRejection`.

## Design
The event is interaction-log based, not tale based. RimWorld adds `Sentence_RomanceAttemptRejected` to the `PlayLogEntry_Interaction` only when the romance attempt fails, so the patch uses that sentence pack as the event signal and avoids inferring rejection from relationship state.

`RebuffRecorder` follows the relationship-recorder pattern used by `NewLoverRecorder`: it strips the extra outcome sentence from the play-log text, resolves the record description through a rulepack, then writes symmetric records for the initiator and recipient. `DaysToRecordAgain` is set to one day and applied explicitly because this recorder does not inherit the tale-recorder duplicate guard.

## Verification
Added recorder-local tests for:
- a forced rejected romance attempt creating `Rebuff` records for both pawns with reciprocal concerns
- repeated rejected romance attempts on the same day creating only one `Rebuff` record per pawn

Verified with a Debug MSBuild build.
