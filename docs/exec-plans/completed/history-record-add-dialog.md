# History Record Add Dialog

Players can now author a new history record directly from the history tab instead of being limited to editing or deleting existing entries. That matters most for pawns with incomplete history, because the player can now seed the first meaningful story beat without leaving the game or touching save data.

## Summary

Added a standalone `Add record` modal dialog to the pawn history tab. The history tab now stays visible for recordable pawns even when they have zero visible history records, and the new `+` launcher opens a forced-pause dialog that lets the player choose a record type, enter raw description text, review the current date, attach concerns through inline map autocomplete, optionally link a visible quest, and create a pinned manual record.

## Shipped Scope

- Kept the add flow out of the inline edit table state.
- Added a left-aligned `+` launcher in the history header while keeping pagination on the right.
- Hid the launcher when `Find.CurrentMap` is `null`, including world-pawn contexts.
- Added the missing `Custom` `HistoryRecordDef` so manual records have a dedicated default type.
- Added inline concern autocomplete modeled on `Dialog_MapSearch` search rules without opening another window.
- Added manual UI setup coverage and automated controller tests for create / blank-submit behavior.
- Fixed the existing history delete refresh regression while touching table-state updates.

## Design

The new flow follows the same state/view/controller split as the existing history page, but inside its own modal window:

- `AddRecordDialog`
- `AddRecordDialogState`
- `AddRecordDialogView`
- `AddRecordDialogController`
- `AddRecordDialogCommand`

`HistoryCardPage` integration stays thin:

- the page still owns history-table and pagination state
- the new launcher only opens the dialog
- successful create calls back into `HistoryTableController.ShowLatestPage(...)`

That callback keeps the existing table logic authoritative for "show the newest page after a record mutation" without leaking dialog form state into `HistoryTableState`.

The dialog architecture was tightened after the first pass:

- `AddRecordDialogState` is passive form data only
- `AddRecordDialogView` draws controls and emits commands, but does not mutate dialog state
- `AddRecordDialogController` owns state creation, autocomplete refresh, highlight movement, text updates, focus-intent consumption, concern add/remove, and record creation
- chip-strip scroll position stays in the window as pure view bookkeeping instead of polluting form state

Concern search intentionally does not reuse `Dialog_MapSearch` as a window. Instead it reuses its filtering model inline:

- current-map search roots come from `map.listerThings.AllThings`
- searchable contents, pawn inventory, apparel, and equipment are included
- hidden, fogged, destroyed, or otherwise skipped entries are filtered the same way
- selected concerns are removed from suggestion results

## Rules

- The history tab is visible whenever `RecorderManager.ShouldRecord(pawn)` is true, even with zero visible records.
- The `+` launcher is only shown when `Find.CurrentMap != null`.
- `Type` excludes debug defs and excludes `PawnGenerated`.
- `Custom` is the default type and is sorted first; remaining types are sorted by `LabelCap`.
- `Description.Trim()` must be non-empty on submit.
- Manual records are always pinned.
- Manual records always pass `location = null`.
- `Quest` shows `None` first, then visible quests newest first.
- The concerns section stays exactly two layout rows:
- row 1: search field with inline autocomplete popup
- row 2: horizontally scrollable selected-concern chip strip

## Verification

Added and kept:

- `Pagination.TestHistoryAddRecordUi` as a skipped in-game setup entry
- `Pagination.TestAddRecordCreateCommand`
- `Pagination.TestAddRecordRejectsBlankDescription`

Ran the approved Debug `MSBuild` build after the dialog, UI, controller, state, and test changes.
