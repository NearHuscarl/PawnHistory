# WidgetWindow

Verse `Window` adapter for widgets.

| Member | Default | Does |
| --- | --- | --- |
| `WidgetWindow(Theme theme = null)` | `theme = null` | Creates the internal `WidgetHost`. |
| `RootSize` | `RootSizing.FillParent` | Root sizing mode used by `DoWindowContents`. |
| `Build(ctx)` | abstract | Returns the root widget. |
