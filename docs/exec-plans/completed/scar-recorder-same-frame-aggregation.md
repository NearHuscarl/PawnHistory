# Scar Recorder Same-Frame Aggregation

Some surgery failures can add more than one permanent scar to the same pawn in the same frame. Before this change, `BodyPartScarredRecorder` treated each scar event as a separate history write, so one botched operation could spam duplicate scar entries instead of reading like one outcome.

## Summary

`BodyPartScarredRecorder` now aggregates same-frame scar inputs inside the recorder and writes a single history record for that pawn and tick. The shared history write path gained an opt-in plural callback overload on `AddRecord(...)`, so recorders can receive all same-frame inputs for one prioritized record def without introducing event-specific aggregation logic into the event layer.

## Shipped Scope

- Restored `CompHistoryManager` to a compile-safe priority batching model.
- Added single-input and plural-input callback support for prioritized `AddRecord(...)` writes.
- Kept batching on the existing same-tick priority flush; no second scheduler was introduced.
- Updated `BodyPartScarredRecorder` to aggregate its same-frame `BodyPartScarredEvent` inputs into one `BodyPartScarred` history record.
- Added a recorder-local test covering botched surgery scars aggregating into one record.

## Design

The important boundary is that aggregation belongs to the recorder, not the event publisher:

- `GameEventBus` still delivers one `BodyPartScarredEvent` at a time.
- `BodyPartScarredRecorder.CreateRecord(...)` now queues the raw input through the plural `AddRecord(...)` callback path instead of writing immediately.
- `CompHistoryManager` stores those pending inputs in the existing priority queue and flushes them at the same tick boundary it already uses for other prioritized records.
- During flush, plural callback entries are grouped by pawn and record def, then resolved once with `List<TInput>`.

This keeps the generic infrastructure narrow:

- normal recorders can still use immediate writes or deferred single callbacks
- aggregating recorders opt in with the plural callback overload
- the event layer remains unchanged and does not need scar-specific aggregate event types

## Rules

- Aggregation is limited to prioritized writes that explicitly use the plural callback overload.
- Same-tick scar entries for the same pawn collapse into one `BodyPartScarred` record.
- The aggregated scar record keeps the existing description shape from the first scar input and suppresses later same-frame duplicates.
- Non-aggregating recorders keep their prior behavior.

## Exclusions

- No new event type was introduced for scar aggregation.
- No second `Delay(0)` stage was added outside the existing priority batching pipeline.
- No in-game test batch was run from the terminal; verification here is limited to the build and the added test method.

## Verification

Built successfully with:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false
```
