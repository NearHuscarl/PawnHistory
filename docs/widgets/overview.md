# Overview

- Widgets are rebuilt each frame. Keep mutable screen state on the owning window, tab, or model.
- Build a tree by returning a root `Widget` from `Build(UiContext ctx)`. Use `W` helpers or constructors. Use `StatelessWidget` for named composition-only nodes.
- `WidgetHost.Draw(rect, build, sizing)` renders the root. `WidgetWindow` and `WidgetTab` wrap that host for Verse windows and RimWorld tabs.
- Layout follows Flutter constraints: constraints go down, size goes up, draw uses the assigned rect. `Measure(...)` must stay side-effect free.
- Keys are automatic from draw position. Provide `key` for reorderable or movable stateful controls, or when code calls `ctx.RequestFocus(...)`.
- Use `ctx.AddOverlay(...)` for popup UI that must draw after the root tree. Convert child-local coordinates with `ctx.ToRoot(...)`.
- Pass `Theme` to the host/window/tab to change shared spacing and sizing defaults. Widgets read values from `ctx.Theme`.
