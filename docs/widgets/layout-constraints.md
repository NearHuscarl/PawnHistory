# LayoutConstraints

Min/max size contract passed from parent to child.

| Member | Default | Does |
| --- | --- | --- |
| `MinWidth` | ctor | Minimum width. |
| `MaxWidth` | ctor | Maximum width. |
| `MinHeight` | ctor | Minimum height. |
| `MaxHeight` | ctor | Maximum height. |
| `HasBoundedWidth/Height` | derived | True when max size is finite. |
| `HasInfiniteWidth/Height` | derived | True when max size is infinite. |
| `Tight(size)` | none | Creates exact width/height constraints. |
| `Loose(maxWidth, maxHeight)` | none | Creates `0..max` constraints. |
| `CopyWith(...)` | all `null` | Returns a copy with selected fields replaced. |
| `Constrain(size)` | none | Clamps a size into the min/max range. |
| `Deflate(horizontal, vertical)` | none | Subtracts padding-like space from the constraint range. |
