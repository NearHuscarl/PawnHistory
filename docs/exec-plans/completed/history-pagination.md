# History Pagination

The history UI now paginates long record lists without changing the underlying story order. That keeps dense pawn histories readable while preserving the original "older first, newer later" chronology the mod records.

## Summary

The history tab and the world-pawn history card both render a top pagination bar above the history table. The pager shows first/previous/page-input/next/last controls, uses a page size of `12`, opens a newly viewed pawn on the last page, and scrolls that first last-page view to the bottom when it overflows so the newest records are visible immediately.

The current implementation uses one table controller with two states:

- `HistoryCardPage` owns long-lived UI state for one history surface
- `HistoryTableState` stores the table state
- `PaginationState` stores the pager state
- `PaginationView` draws the pager and returns pager commands
- `HistoryTableController` owns table and pagination decisions
- `HistoryTableView` draws the table, rows, tooltips, quest links, and row interactions

## Shipped Scope

- Added the history pagination bar above the table
- Preserved history record order and existing row interactions
- Split the history UI into page host, immediate-mode views, explicit table/pager state, and one table controller
- Replaced the committed numeric field abstraction with reusable input validators
- Kept the skipped manual pagination test under `PaginationRecorder`

## Design

Each host owns its own `HistoryCardPage` instance so state is not shared between the pawn tab and the world-pawn info card.

`HistoryCardPage` is orchestration only:

- owns `HistoryTableState`
- owns `PaginationState`
- owns `HistoryTableController`
- keeps `ScrollPosition` local to the page/view path
- syncs external pawn context before drawing the pager
- passes returned pager commands back into the controller before drawing the table

`PaginationView` handles only immediate-mode mechanics:

- draws the right-aligned controls
- updates the raw page text buffer
- blocks non-digit edits
- detects Enter on the focused input
- returns pager commands for the current frame

`HistoryTableController` owns the table-level logic:

- filters visible history records
- computes the current page slice
- handles first / previous / next / last / submitted page-number commands
- updates `PaginationState`
- updates `HistoryTableState.VisibleRecords`
- decides one-shot table effects such as initial bottom-scroll on first open

`HistoryTableView` remains responsible for the actual table rendering:

- header layout
- row measurement
- scroll view drawing
- row tooltips and interactions
- applying and clearing the one-shot bottom-scroll effect

## Rules

- History order is never sorted or reversed by pagination.
- Page numbers are 1-based.
- Page size is `12`.
- Invalid page submit restores the committed page text.
- Navigation buttons that would lead to an invalid page are disabled.
- The first view for a pawn opens on the last page.
- That initial last page scrolls to the bottom if it overflows.

## Verification

Verified by:

- preserving the skipped manual pagination test with 120 mixed-height rows
- running the approved Debug `MSBuild` build after the current refactor

Manual verification still matters for:

- unchanged pager layout
- Enter-to-submit behavior
- disabled button state at bounds
- initial last-page bottom-scroll behavior
