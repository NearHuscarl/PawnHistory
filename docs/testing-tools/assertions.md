# Assertions

`Expect` is the entrypoint for recorder assertions. It returns `PawnHistoryAssertions`, which supports positive, negative, synchronous, and eventual checks.

## Expect

- `Expect.Assertions(int count)`: declare the expected assertion count for the running test.
- `Expect.That(Pawn pawn)`: assert against one pawn.
- `Expect.ThatAll(IEnumerable<Pawn> pawns)`: every pawn must satisfy the assertion.
- `Expect.ThatAny(IEnumerable<Pawn> pawns)`: at least one pawn must satisfy the assertion.
- `Expect.That(value)` / `Expect.That(sequence)`: generic entrypoints for scalar and sequence assertions via `SimpleAssertions<T>`.

All of these require an active test context. They are meant to be called inside recorder tests only.

## Assertion Modifiers

- `Not()`: invert the next assertion.
- `Eventually(int timeoutTicks = 3000, int pollIntervalTicks = 25)`: poll until the assertion passes or times out.

Use `Eventually(...)` when the recorder is reached by delayed jobs, letters, travel, or other multi-tick flows.

## Simple Assertions

Use `Expect.That(value)` when the test is checking plain values instead of history-record shape.

Supported `SimpleAssertions<T>` methods:

- `Equal(...)` / `NotEqual(...)`
- `Same(...)` / `NotSame(...)`
- `SequenceEqual(...)`
- `ToContain(...)`
- `ToBeLessThan(...)`
- `ToBeGreaterThan(...)`
- `ToBeTrue()` / `ToBeFalse()`
- `ToBeNull()` / `ToBeNotNull()`

Preferred usage:

- Use `SimpleAssertions<T>` for dates, counts, booleans, object identity, and simple sequences.
- Keep `PawnHistoryAssertions` for `ToHaveHistoryRecord(...)`, `ToHaveHistoryRecordOf(...)`, and other history-specific checks.

## History Assertions

- `ToHaveHistoryRecord(string descriptionTemplate, HistoryRecordDef recordDef = null, bool exactMatch = false, int index = -1)`
- `ToHaveHistoryRecord(ExpectedHistoryRecord expected)`
- `ToHaveHistoryRecordOf(HistoryRecordDef def, int index = -1)`
- `ToHaveHistoryRecordCount(int expected)`

Preferred usage:

- Use `ToHaveHistoryRecord(...)` with `recordDef` when you care about both text shape and def identity.
- Use `index` only when the exact history-record slot matters.
- Use `ToHaveHistoryRecord(new ExpectedHistoryRecord { ... })` when one record must match several fields such as def, description, concerns, position, map, location, or quest. Null fields are ignored.
- Use `ToHaveHistoryRecordOf(...)` only when the def alone is the meaningful assertion.

## Description Matching

Description assertions strip tags and compare structural text, not literal raw strings, unless `exactMatch` is enabled.

This is why rulepack-driven assertions should usually compare templates rather than resolved pawn names verbatim.
