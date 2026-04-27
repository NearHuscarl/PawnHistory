# Assertions

`Expect` is the entrypoint for recorder assertions. It returns `PawnHistoryAssertions`, which supports positive, negative, synchronous, and eventual checks.

## Expect

- `Expect.Assertions(int count)`: declare the expected assertion count for the running test.
- `Expect.That(Pawn pawn)`: assert against one pawn.
- `Expect.ThatAll(IEnumerable<Pawn> pawns)`: every pawn must satisfy the assertion.
- `Expect.ThatAny(IEnumerable<Pawn> pawns)`: at least one pawn must satisfy the assertion.

All of these require an active test context. They are meant to be called inside recorder tests only.

## Assertion Modifiers

- `Not()`: invert the next assertion.
- `Eventually(int timeoutTicks = 3000, int pollIntervalTicks = 25)`: poll until the assertion passes or times out.

Use `Eventually(...)` when the recorder is reached by delayed jobs, letters, travel, or other multi-tick flows.

## History Assertions

- `ToHaveHistoryRecord(string descriptionTemplate, HistoryRecordDef recordDef = null, bool exactMatch = false, int ticksAgo = 0)`
- `ToHaveHistoryRecord(string descriptionTemplate, int index, bool exactMatch = false)`
- `ToHaveHistoryRecordOf(HistoryRecordDef def, int index = -1)`
- `ToHaveHistoryRecordCount(int expected)`
- `ToHaveHistoryRecordPosition(IntVec3 position, HistoryRecordDef recordDef, int ticksAgo = 0)`
- `ToHaveHistoryRecordConcern(Thing concern, HistoryRecordDef recordDef, int ticksAgo = 0)`
- `ToHaveHistoryRecordQuest(Quest quest, HistoryRecordDef recordDef, int ticksAgo = 0)`

Preferred usage:

- Use `ToHaveHistoryRecord(...)` with `recordDef` when you care about both text shape and def identity.
- Use `ToHaveHistoryRecordQuest(...)` when the recorder must attach the right generated quest.
- Use `ToHaveHistoryRecordConcern(...)` for attached pawns, things, or other concern objects.
- Use `ToHaveHistoryRecordOf(...)` only when the def alone is the meaningful assertion.

## Description Matching

Description assertions strip tags and compare structural text, not literal raw strings, unless `exactMatch` is enabled.

This is why rulepack-driven assertions should usually compare templates rather than resolved pawn names verbatim.
