# Priority Write Resolution

`CasualtyRecorder` used `TickDelayManager.Delay(0, ...)` to wait for RimWorld to finish populating combat-log body-part data before turning a casualty event into history records. That worked, but it split one recorder's write path across two schedulers and made the batching rules harder to trust, especially around same-tick death, downed, and title-inheritance ordering.

## Summary

Priority batching now supports a deferred request builder per record. `CompHistoryManager` still decides whether a write is immediate or batched from `HistoryRecordPriority`, but a prioritized record can now postpone construction of its final `HistoryRecordWriteRequest` until the flush.

This removes the casualty-specific `Delay(0)` hack by moving the affected record builders onto the same deferred batch path.

## Scope

- Added a deferred `WriteRecord(...)` path in `CompHistoryManager` that queues `Func<HistoryRecordWriteRequest>` for priority records.
- Added a matching protected `AddRecord(...)` overload in `RecorderBase`.
- Expanded `HistoryRecordPriority` so combat casualty records that need finished combat-log text are resolved during the batch:
  `Downed`, `Kill`, `BondedAnimalDeath`, plus the existing death-related entries that must preserve same-tick ordering.
- Reworked `CasualtyRecorder` so `Register()` queues each casualty-related record directly and resolves the combat log lazily through one memoized lookup.
- Moved `DeathrestOrComaRecorder` onto the same deferred request-builder path so its combat log and concerns are resolved during the flush.

## Design

The important constraint was keeping the original casualty behavior readable:

- The deferred path only postpones request construction; `CompHistoryManager` still owns batching and per-pawn priority order.
- `CasualtyRecorder` now performs its death-location carryover and same-tick `Downed` cleanup inside the deferred request builder, where it can observe already-flushed higher-priority records for that pawn.
- `CasualtyRecorder` does not carry resolver state through fake domain input records or a fake partial request shape.

Inside the casualty event handler, the recorder now captures a single memoized `CasualtyContext`. Every queued record builder reuses that context, so the battle log is scanned once per casualty event rather than once per emitted record.

## Behavioral Rules

- Non-priority records still bypass the pending queue.
- Priority ordering is still decided by `HistoryRecordPriority`.
- `Death` still resolves after `Downed` for the same pawn.
- `RelativeDeath` and `BondedAnimalDeath` are still queued before later same-tick records on those POV pawns, such as `TitleInherited`.
- `DeathrestOrComa` now resolves its combat-log prefix and appended concerns during the deferred flush rather than through a recorder-specific finalize hook.
- Same-tick `Downed` removal still happens during deferred death resolution, not in the event handler.

## Verification

Built successfully with:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false
```

No in-game recorder test pass was run from the terminal.
