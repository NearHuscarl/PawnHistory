# Automatic Widget State Keys

The widget framework previously required callers to hand-author string keys for any widget that needed transient UI state. That made common controls like scroll views, text fields, and autocomplete fragile to compose because forgetting a key silently broke focus or scroll persistence.

This change teaches the framework to derive temporary state keys from a widget's position in the drawn widget tree. Stateful widgets now work by default, while explicit keys still exist for the cases where a screen intentionally wants to pin a piece of UI state to a named identity instead of tree position.

## Summary
Shipped draw-time widget key-path tracking with temporary in-memory `int` keys. `ScrollView`, `TextArea`, and `Autocomplete` now resolve their state from the current draw path when no explicit key is provided. Parent widgets participate in key traversal only during `Draw`, not `Measure`, so layout behavior stays unchanged.

## Shipped Scope
- Added `WidgetKey`, `WidgetIds`, and `WidgetTree`.
- Refactored `Widget` to carry a widget id plus an optional explicit key override.
- Added key-path tracking to `UiContext` and reset it at the root renderer before drawing.
- Moved scroll-position storage and focus-request tracking from string keys to `int` keys.
- Updated container widgets to draw children through `WidgetTree.DrawChild(...)`.
- Refactored `ScrollView`, `TextArea`, and `Autocomplete` to use automatic state keys.
- Removed explicit keys from `AddRecordDialog` for the main scroll view, description field, and concern autocomplete so the screen exercises the automatic path.

## Design
Automatic keys come from nested `HashCode.Combine(...)` calls over the active draw path:

1. The root host resets the key path.
2. Each parent pushes a child-specific segment before drawing that child.
3. A stateful widget reads `ctx.CurrentKey` when it has no explicit `WidgetKey`.

Explicit keys still override state identity. When a widget is constructed with a non-empty `WidgetKey`, its own state uses that key value directly, while its descendants still inherit a tree path segment derived from that explicit value.

`Measure(...)` intentionally does not mutate key state. The same widget can be measured multiple times for layout without affecting the state identity used during the real draw pass.

## Rules
- Key tracking is draw-only. No push/pop happens during `Measure(...)`.
- Widget state identity is temporary process-local UI state only. It is not designed for cross-session persistence.
- Parent widgets centralize push/pop through `WidgetTree.DrawChild(...)` instead of duplicating stack handling in each widget.
- Verse focus APIs still require a string control name, so `TextArea` and `Autocomplete` convert the resolved `int` state key to a control name only at the immediate IMGUI boundary.

## Exclusions
- No stable hashing was introduced.
- No reflection, runtime type inspection, caller metadata, GUIDs, static counters, or string path building were added.
- No layout measurement rules were changed.
- No unrelated widget behavior was refactored beyond the state-key migration.

## Verification
- Built `PawnHistory.csproj` in Debug with MSBuild after the refactor.
- Confirmed the framework no longer requires explicit keys at the `AddRecordDialog` call site for the exercised stateful widgets.
