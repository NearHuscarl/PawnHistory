# Flexible

Flex child wrapper for `Row` and `Column`.

| Member | Default | Does |
| --- | --- | --- |
| `child` | none | Wrapped child. |
| `flex` | `1` | Relative share of remaining main-axis space; values below `1` clamp to `1`. |
| `fit` | `FlexFit.Loose` | `Loose` lets the child use less than its share; `Tight` forces the share size. |
| `key` | `null` | Explicit identity for this wrapper. |
| `FlexFit.Tight` | n/a | Forces the allocated main-axis size. |
| `FlexFit.Loose` | n/a | Lets the child measure below the allocated main-axis size. |
