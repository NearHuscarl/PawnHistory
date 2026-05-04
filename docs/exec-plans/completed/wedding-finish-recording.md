# Wedding Finish Recording

Wedding spectator attendance is only reliable when the ceremony finishes. Recording spectators as they join the wedding lord made the count depend on a partial join sequence instead of the final attendance set.

## Summary
Wedding recording now follows the party finish pattern. The wedding start record remains immediate and couple-only. The finish record is emitted from the `LordToil_End` transition and stores one concrete finish reason: `Success`, `PawnKilled`, `DangerousMap`, or `Unknown`.

On success, spectator records are written as `WeddingFinished` records from the final wedding-goer snapshot. The couple remains excluded from attendee records because they already receive the start record. Interrupted weddings also write `WeddingFinished` records, with separate text for pawn death, nearby threats, and unknown interruptions.

## Shipped Scope
- Removed `WeddingRecorder`'s `JoinedLordEvent` subscription.
- Replaced `WeddingCancelledInput` with `WeddingFinishedInput`.
- Moved spectator attendance text from `PH_WeddingJoined` to `PH_WeddingFinished` under `reason==Success`.
- Renamed the history def and rulepack from `WeddingCancelled` to `WeddingFinished`.
- Updated wedding tests to assert successful spectator attendance and cancellation reasons against `WeddingFinished`.

## Rules
- Wedding start remains couple-only and immediate.
- Wedding spectator records are written only when the wedding succeeds, using `HistoryRecordDefOf.WeddingFinished`.
- Cancelled weddings do not write spectator attendance records; they write wedding-finished interruption records for current wedding-goers with the specific finish reason.
- Final spectator count comes from the `LordToil_End` transition snapshot.

## Verification
Ran the approved Debug MSBuild build successfully:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false
```
