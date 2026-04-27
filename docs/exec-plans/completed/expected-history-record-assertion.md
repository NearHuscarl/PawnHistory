# Expected History Record Assertion

Implemented a `ToHaveHistoryRecord(ExpectedHistoryRecord expected)` test assertion overload for matching a history record by any subset of supported fields.

## Notes

- `ExpectedHistoryRecord` fields are nullable; null means the field is not asserted.
- The matcher checks all exposable `HistoryRecord` fields: def, date, pawn, description, concerns, tile id, location, and quest.
- Convenience `Position` and `Map` fields are included for nested `RecordLocation` assertions.
- Description matching uses the existing structural text comparison behavior.
- Failure output prints a field-by-field expected and actual record summary, including the no-record case.
