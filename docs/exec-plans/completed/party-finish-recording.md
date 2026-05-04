# Party Finish Recording

Party attendance is only meaningful once the party has actually ended. Recording each pawn at join time made the attendee count depend on a transient partial list instead of the final set RimWorld uses when applying the party outcome.

## Summary
Party recording now has two recorder inputs: party started and party finished. The started record still writes the organizer's party-start history immediately. The finished record is emitted from the lord transition to `LordToil_End` and classifies the end as either `Timeout` or `Interrupted`, while preserving the specific interruption reason.

On timeout, attendee records are written as `PartyFinished` records from the final `lord.ownedPawns` snapshot, so each attendee description uses the same partygoer count RimWorld had when the party finished. Interrupted parties also write `PartyFinished` records instead of the old cancelled record, with separate text for pawn death, organizer leaving, nearby threats, and unknown interruptions.

## Shipped Scope
- Removed PartyRecorder's `JoinedLordEvent` subscription.
- Replaced `PartyCancelledInput` with `PartyFinishedInput`, carrying both finish reason and interruption reason.
- Moved attendee description text from the start rulepack to `PH_PartyFinished` with `reason==Timeout`.
- Renamed the history def and rulepack from `PartyCancelled` to `PartyFinished`.
- Added an active timeout test that asserts final attendee recording and count text.
- Removed `SkipTest` from the Party tests and added scenario-specific assertions for dangerous-map, organizer-left, and pawn-killed interruptions.

## Rules
- Party start remains organizer-only and immediate.
- Party attendee records are written only when the party times out normally, using `HistoryRecordDefOf.PartyFinished`.
- Interrupted parties do not write attendee records; they write a party-finished interruption record for current partygoers with the specific interruption reason.
- Final partygoer count comes from the `LordToil_End` transition snapshot.

## Verification
Ran the approved Debug MSBuild build successfully:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false
```
