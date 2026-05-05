# History Record Inline Editing

Players sometimes need to correct or refine a generated history description without rebuilding the record or editing save data by hand. This change adds a direct in-card editing flow so the history log can be corrected in place while preserving the rest of the record.

## Summary

The history card action menu now includes `Edit` and `Delete record`. Editing turns the selected record's description cell into an inline text editor. The editor uses the same description-cell rect that the row already calculated, starts from the raw stored description string, and saves back `text.Trim()` only when the result is non-empty. Deletion removes the selected record from the pawn's history immediately.

Editing is intentionally modal at the history-card level. While a row is being edited, the rest of the history card stops accepting clicks so the user cannot jump to records, change pages, open quest links, or start competing row actions until the edit is saved or canceled.

## Shipped Scope

- Added an `Edit` menu item for history rows.
- Added a working `Delete record` menu item for history rows.
- Added inline raw-text editing for `HistoryRecord.description`.
- Added `Esc` cancel behavior that discards unsaved changes.
- Added plain `Enter` save behavior with rejection for empty trimmed text.
- Kept `Shift+Enter` on the existing `Widgets.TextArea(...)` path instead of adding custom multiline handling.
- Added a skipped tagged manual setup test under `PaginationRecorder`.
- Documented the action menu behavior in [README.md](/C:/Program%20Files%20(x86)/Steam/steamapps/common/RimWorld/Mods/PawnHistory/README.md).

## Design

The implementation follows the repo's immediate-mode presenter split.

- `HistoryTableState` owns transient UI state for the edit session:
  - active editing record
  - raw edit buffer
  - one-shot focus intent
- `HistoryTableView` keeps immediate-mode UI plumbing:
  - opening the row action menu
  - drawing the text area inside the existing description-cell rect
  - intercepting `Esc` and plain `Enter`
  - blocking row and quest-button clicks while editing
- `HistoryTableController` only owns the meaningful commit path:
  - validate `text.Trim()`
  - reject empty trimmed text with RimWorld's standard reject message
  - persist the trimmed description back onto the record
  - remove the selected history record and clamp pagination after deletion

This keeps UI-only state transitions such as `BeginEditing(...)` and `ClearEditingSession()` out of the controller while still routing the actual save semantics through one place.

## Rules

- The edit field is seeded from `record.description` exactly as stored.
- Existing tags and color markup remain visible and editable as plain text.
- Save writes back exactly `text.Trim()`, with no extra formatting or normalization.
- Delete removes the selected record from `CompHistory`.
- Empty trimmed saves are rejected and leave the edit session intact.
- Successful saves are silent.
- Editing is cleared when the shown pawn changes.
- Plain `Enter` is consumed before the text area can insert a newline, even when save is rejected.
- While editing, row height follows the raw edit buffer so multiline input can grow the editor cell.

## Exclusions

- No extra success toast was added after save or delete.
- No additional hover-behavior cleanup was added beyond blocking active clicks.
- No automated assertions were added because this is UI behavior, but a skipped tagged manual setup test was added for in-game verification.

## Verification

- Ran `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`
- Build succeeded with 0 warnings and 0 errors.
- Added `Pagination.TestHistoryInlineEditUi` as a skipped `[TestTag("Manual")]` setup entry for grouped manual runs.
- In-game manual verification was not run in this implementation pass.
