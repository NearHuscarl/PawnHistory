# History Record Test Assertion Refactor

## Summary

- Merged the string-based history assertion API into `ToHaveHistoryRecord(string descriptionTemplate, HistoryRecordDef recordDef = null, bool exactMatch = false, int index = -1)`.
- Removed the specialized concern, quest, and position assertion helpers from `PawnHistoryAssertions`.
- Migrated recorder tests onto either:
  - string + def assertions for description-only checks
  - `ExpectedHistoryRecord` for multi-field checks such as concerns, quest, and location

## Key Test Updates

- Added explicit `HistoryRecordDefOf.*` coverage to recorder tests that previously asserted description only.
- Rewrote ordered-history checks to use the merged overload with explicit `index`.
- Converted concern / quest / location-bearing tests to `ExpectedHistoryRecord`.
- Added missing quest assertions to quest-backed recorder tests where the quest instance was available.
- Added missing concern assertions for deterministic recorders such as animal taming, trading, prison breaks, gifts, weapon bonding, and quest discovery.

## Verification

- `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`
  - Result: success, 0 warnings, 0 errors
- Recorder test runner verification is still manual from RimWorld:
  - `Pawn History -> Run All Tests`
  - No shell-accessible recorder-test runner is exposed by the repo.
