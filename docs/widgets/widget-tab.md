# WidgetTab

RimWorld `ITab` adapter for widgets.

| Member | Default | Does |
| --- | --- | --- |
| `WidgetTab(Theme theme = null)` | `theme = null` | Creates the internal `WidgetHost`. |
| `RootSize` | `RootSizing.FillParent` | Root sizing mode used by `FillTab`. |
| `RootRect` | `new(0f, 0f, size.x, size.y)` | Root draw rect inside the tab. |
| `Build(ctx)` | abstract | Returns the root widget. |
