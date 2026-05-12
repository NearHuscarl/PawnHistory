# Typed Tale Event Adapter

Tale records originate from RimWorld's broad `TaleRecorder.RecordTale` call, where extra data is passed as untyped `object[]` arguments. That made tale-backed recorders responsible for upstream argument order instead of consuming domain-shaped events.

## Summary
Added a `TaleEventAdapter` between the Harmony patch and recorder subscriptions, then completed the migration so every `HistoryTaleRecorder` now consumes a typed tale event published by `TaleDispatcher`.

## Shipped Scope
- `ReadBookRecorder` now subscribes to `ReadBookEvent(Pawn, Book)`.
- `AnimalHuntedRecorder` now subscribes to `AnimalHuntedEvent(Hunter, Prey)`.
- Added typed events and dispatchers for `Exhausted`, `MinedValuable`, `OnFire`, `PlayedGame`, `Stripped`, and `VisitedGrave`.
- Switched all remaining `HistoryTaleRecorder` subclasses to `GameEventBus.Subscribe<XyzEvent>(CreateRecord)`.
- Removed recorder-facing `TaleRecordedEvent` publication from the adapter path.
- Updated tale-recorder guidance in `docs/design-docs/core-beliefs.md`.

## Design
The adapter is the only module that reads `TaleRecordedEvent.Params`. It validates payload shape before publishing typed events, so malformed tale payloads do not reach recorder logic.

`TaleRecordedEvent` remains an internal adapter input between the Harmony patch and dispatcher table. Recorders no longer subscribe to it directly.

## Verification
- Searched the codebase to confirm all `HistoryTaleRecorder` subclasses now consume typed tale events.
- Searched for `Subscribe<TaleRecordedEvent>` to confirm recorder-facing raw tale subscriptions were removed.
- Ran the Debug MSBuild build successfully.
