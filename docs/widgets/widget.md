# Widget

Base class for all widget nodes.

| Member | Default | Does |
| --- | --- | --- |
| `Widget(string key = null)` | `key = null` | Creates a widget with optional explicit state identity. |
| `Measure(ctx, constraints)` | none | Returns the constrained size for the current render pass. |
| `DoMeasure(ctx, constraints)` | abstract | Implements measurement. |
| `Draw(ctx, rect)` | abstract | Draws into the assigned rect. |
