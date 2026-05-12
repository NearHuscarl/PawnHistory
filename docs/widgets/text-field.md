# TextField

Controlled text input.

| Member | Default | Does |
| --- | --- | --- |
| `value` | none | Current text value; `null` is treated as empty. |
| `onChange` | none | Called with edited text; caller owns state updates. |
| `onSubmit` | `null` | Called on Enter or keypad Enter. |
| `onCancel` | `null` | Called on Escape. |
| `onClickOutside` | `null` | Called when focused and clicked outside. |
| `width` | `200f` when unbounded | Fixed width; bounded fields use max width when omitted. |
| `height` | `null` | Fixed height override. |
| `minHeight` | `32f` | Minimum height. |
| `maxHeight` | `null` | Maximum height clamp. |
| `multiline` | `false` | Uses `Widgets.TextArea` instead of `Widgets.TextField`. |
| `enabled` | `true` | Enables or disables editing. |
| `font` | `GameFont.Small` | Font used for measure and draw. |
| `focusCursorToEnd` | `false` | Moves the caret to the end when focus is requested. |
| `key` | `null` | Explicit focus/state identity. |
