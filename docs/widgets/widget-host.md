# WidgetHost

Root renderer for a widget tree.

| Member | Default | Does |
| --- | --- | --- |
| `WidgetHost(Theme theme = null)` | `theme = null` | Creates a host with persistent `UiContext`; falls back to `new Theme()`. |
| `Context` | host-owned | Shared render state, overlays, focus, scroll, theme. |
| `Theme` | `Context.Theme` | Exposes the host theme. |
| `Draw(rect, build, sizing)` | `sizing = RootSizing.FillParent` | Renders the root widget into `rect`. |
| `RootSizing.FillParent` | n/a | Uses the full root rect. |
| `RootSizing.HugContent` | n/a | Measures first, then shrinks root rect to content size. |
