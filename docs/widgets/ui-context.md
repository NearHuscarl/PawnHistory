# UiContext

Per-host render state and shared services.

| Member | Default | Does |
| --- | --- | --- |
| `UiContext(Theme theme = null)` | `theme = null` | Creates context state; falls back to `new Theme()`. |
| `Theme` | context-owned | Shared spacing and sizing values. |
| `CurrentKey` | auto | Current automatic widget state key. |
| `GetScrollPosition(key)` | none | Reads stored scroll position. |
| `SetScrollPosition(key, position)` | none | Stores scroll position. |
| `RequestFocus(key)` | none | Queues focus by integer key. |
| `RequestFocus(string key)` | none | Queues focus by named key. |
| `ConsumeFocus(key)` | none | Returns and clears a pending focus request. |
| `ToRoot(rect/position)` | none | Converts child-local coordinates to root coordinates. |
| `ControlId(key)` | none | Converts a state key to a control id string. |
| `AddOverlay(draw)` | none | Queues popup/overlay drawing after the root tree. |
