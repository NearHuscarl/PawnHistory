# EdgeInsets

Per-edge padding values.

| Member | Default | Does |
| --- | --- | --- |
| `Left` | ctor | Left inset. |
| `Top` | ctor | Top inset. |
| `Right` | ctor | Right inset. |
| `Bottom` | ctor | Bottom inset. |
| `EdgeInsets(float all)` | none | Applies the same inset to all sides. |
| `Only(...)` | omitted edges `0` | Creates edge-specific insets. |
| `Horizontal` | derived | `Left + Right`. |
| `Vertical` | derived | `Top + Bottom`. |
