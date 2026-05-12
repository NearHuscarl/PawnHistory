# Record Writing Extraction

History records are the persistent facts behind the pawn history UI. Before this change, `RecorderBase.AddRecord(...)` both wrote records and knew that a `PawnGenerated` record should trigger timeline backfill. That made record storage look like a simple helper while hiding one event-specific post-write behavior inside the base recorder module.

## Summary

Record writing now lives behind `HistoryRecordWriter`, and pawn-generation timeline simulation is triggered explicitly by `PawnGeneratedRecorder` after a generated record is successfully written.

## Shipped Scope

- Added `HistoryRecordWriter` and `HistoryRecordWriteRequest` as the internal write path for recorder-created history records.
- Changed `RecorderBase.AddRecord(...)` to return the written `HistoryRecord`.
- Moved the `PawnGenerated` timeline simulator call out of `RecorderBase` and into `PawnGeneratedRecorder`.
- Updated `CasualtyRecorder`'s `AddRecord(...)` override to preserve its death-location behavior while returning the written record.
- Added a pawn-generation regression test that exercises `CreateRecord(...)` and confirms backfill still runs from the explicit post-write call.

## Design

`HistoryRecordWriter.Write(...)` constructs the `HistoryRecord`, resolves the pawn's `CompHistory`, appends to the comp's backing record list, and returns the created record. If no history comp is attached, it logs a warning and returns `null`.

Recorder authors can keep calling `AddRecord(...)` as before. Specialized flows that need post-write behavior can now capture the returned record and act explicitly instead of adding new conditionals to `RecorderBase`.

## Verification

Ran the approved Debug MSBuild build:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false
```

The build succeeded with 0 warnings and 0 errors.
