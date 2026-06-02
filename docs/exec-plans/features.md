# FEAT

## History backfill

When RimWorld generates a 40-year-old pawn, it gives them all their injuries, scars, and titles at the exact same moment
during pawn generation. If you look at their history log, it would look like they had a very busy 1.2 seconds of life.

A `HistoryBackfillEngine` takes those events and "smears" them back across the pawn's biological life so their
history looks like a natural narrative.

## History priority

When several history records happen at the same time, history priority keeps them in a sensible order so the log
can be read in a logical chronological order.

## History card action menu

Right-click a history row to open its action menu.

- `Pin`/`Unpin`: Whether to pin current history record. Pinned record will never be removed.
- `Edit`: edits the current row's description using an inline text editor text.
  - Allow editing color tags in plain text.
  - `Enter` saves a trimmed non-empty description, `Shift+Enter` inserts a newline, and `Esc` cancels without changing the record.
- `Delete record` removes the selected history record permanently.
- `Copy description` copies the rendered description to the clipboard (color tags stripped).
