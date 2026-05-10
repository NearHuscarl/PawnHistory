# IMGUI Widget Demo Add Record Dialog

The add-record dialog is the smallest history UI surface that exercises text entry, stable focus, buttons, menus, scroll regions, and lightweight layout. That makes it a good proving ground for a declarative IMGUI layer without forcing the history table itself into a broad rewrite.

## Summary

Added a minimal two-phase declarative IMGUI widget system under `Source/PawnTracker/Ui/Widgets/` and rewrote `AddRecordDialog` to use it as the first real in-mod demo. The dialog now builds a widget tree directly in C#, keeps its state local to the window, and no longer uses the previous add-dialog view/controller/command split.

## Shipped Scope

- Added small widget primitives for measure/draw composition:
- `IWidget`
- `LayoutConstraints`
- `UiContext`
- `Widget`
- `EdgeInsets`
- `VStack`
- `HStack`
- `Padding`
- `SizedBox`
- `Spacer`
- `Label`
- `Button`
- `TextArea`
- `ScrollView`
- `WidgetRenderer`
- Kept layout rect-based and immediate-mode. No `GUILayout`, retained widget state, or generalized flex system was introduced.
- Rewrote `AddRecordDialog` into a single file that owns:
- draft state creation
- direct UI mutations for form fields
- concern autocomplete and highlight movement
- record submission validation and creation
- local custom widgets used only by this dialog
- Removed the add-dialog-specific controller, command, state, and view files.
- Updated the existing add-record recorder tests to target the new draft/create path instead of the removed controller API.

## Design

The widget layer stays intentionally narrow:

- widgets are rebuilt every draw
- state lives outside widget instances
- layout uses a measure pass followed by draw against explicit `Rect`s
- `UiContext` only carries shared UI bookkeeping such as keyed scroll offsets and focus requests

The dialog rewrite uses the generic primitives for the page skeleton and only adds dialog-local custom widgets where the generic system would be awkward:

- fixed-width labeled rows
- menu-section wrappers
- concern chips
- concern suggestion rows

That split keeps the reusable layer boring while still letting the dialog express a Flutter-like tree:

- root `VStack`
- field rows
- inline `TextArea`
- horizontal chip strip inside `ScrollView`
- footer `HStack`

## Rules

- The widget namespace is `PawnHistory.Source.PawnTracker.Ui.Core`, even though the files live under `Ui/Widgets/`.
- This avoids colliding with `Verse.Widgets`, which existing IMGUI screens already reference heavily.
- `TextArea` uses stable control keys and routes focus through `UiContext`.
- `Escape` cancels and unfocuses only when an explicit cancel action exists.
- `Enter` submits only when an explicit submit action exists.
- `Shift+Enter` remains available for multiline newline entry.
- The add-record dialog remains a demo consumer only. The history table and pagination views were not migrated to the widget API in this change.

## Verification

Verified by:

- updating `PaginationRecorder` add-record tests to use the new draft/create flow
- running the approved Debug `MSBuild` build successfully after the widget and dialog rewrite

Explicitly not verified in this change:

- in-game visual polish beyond compile-safe layout behavior
- broader migration of existing history-page surfaces onto the widget system
