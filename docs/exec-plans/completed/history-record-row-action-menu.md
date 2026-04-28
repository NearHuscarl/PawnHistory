# History Record Row Action Menu

## Summary

- Added a row-level right-click action menu to the history record UI.
- Added a persisted `HistoryRecord.pinned` flag and a decorative pinned-row accent.

## Key Changes

- Right click on any history row now opens a `FloatMenu` with copy, edit, and pin or unpin actions.
- Copy uses the existing color-stripped description text and shows the same neutral feedback message through keyed translations.
- `Edit` is a no-op placeholder for now.
- Pinned rows draw a 2 px pink border on the right edge without affecting layout.
- Pin state is persisted through `HistoryRecord.ExposeData()` and does not yet affect prune, sorting, or filtering behavior.

## Verification

- `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`
  - Build is currently blocked by unrelated duplicate `TradeUtility_ReceiveQuestFromTrader_Patch` definitions in `Source/PawnTracker/Events/QuestDiscoveredEvent.cs`.
