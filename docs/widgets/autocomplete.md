# Autocomplete

Controlled query field with popup options.

| Member | Default | Does |
| --- | --- | --- |
| `controller` | none | Persistent query, option, and highlight state. |
| `findOptions` | none | Produces options for the current query. |
| `onSelected` | none | Called when an option is chosen. |
| `drawOption` | none | Draws one popup row. |
| `height` | `ctx.Theme.TextFieldHeight` | Field height when omitted. |
| `popupRowHeight` | `26f` | Height of each popup row. |
| `maxPopupRows` | `6` | Maximum visible option rows. |
| `key` | `null` | Explicit focus/state identity. |
| `AutocompleteController.Query` | `string.Empty` | Current query text. |
| `AutocompleteController.Options` | empty list | Current option list. |
| `AutocompleteController.HighlightedIndex` | `-1` | Current highlighted option index. |
| `SetQuery(query, options)` | none | Replaces query and option list. |
| `Clear()` | none | Clears query, options, and highlight. |
| `MoveHighlight(delta, visibleCount)` | none | Moves the highlight within visible options. |
| `Highlight(index, visibleCount)` | none | Sets the highlighted option. |
| `TryGetHighlighted(visibleCount, out option)` | none | Returns the current highlighted option, if any. |
