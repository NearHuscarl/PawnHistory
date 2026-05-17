# Record Priority Batching

## Summary

PawnHistory now supports def-owned priority ordering for records that are emitted from the same game tick but arrive through different vanilla callbacks. Most records still write immediately. Only records present in the priority table are delayed into a per-tick write batch, then flushed per pawn in priority order before the `HistoryRecord` objects are created.

This fixes recorder pairs whose vanilla event order is not the same as the narrative order the history tab should show, without mutating existing records after insertion.

## Shipped Scope

- Added prioritized record write batching in `CompHistoryManager`.
- Added an explicit dictionary priority table for the currently known same-tick ordering conflicts.
- Kept `TickDelayManager` as a generic scheduling API; the priority behavior lives entirely in the history write path.
- Removed the casualty recorder location hack that copied location from preceding `Crushed` or `FriendlyTrapHit` records.
- Preserved crushed and friendly-trap death locations through a recorder-owned reconciliation hook, after priority ordering has placed the source record before `Death`.

## Priority Rules

Lower priority values flush earlier. Related values are intentionally close so future record defs can be inserted into the same local band.

- `Crushed = 1000`
- `FriendlyTrapHit = 1000`
- `Death = 1010`
- `RelativeDeath = 1100`
- `TitleInherited = 1110`
- `BodyPartDestroyed = 2000`
- `DeathrestOrComa = 2010`
- `SkillLeveledDown = 2020`

Records not listed here bypass the queue and write immediately.

## Design

`CompHistoryManager.WriteRecord` checks `HistoryRecordPriority` before constructing a `HistoryRecord`. Non-priority records use the existing immediate write path. Priority records store the original `HistoryRecordWriteRequest` in a dictionary keyed by `GenTicks.TicksAbs`, then schedule one `Delay(0)` flush for that tick.

The flush removes only that tick's pending list, groups entries by pawn, orders each pawn's entries by priority and original request sequence, lets the source recorder reconcile the request, and calls the normal record write path. Because the pending structure is keyed by tick, a later tick cannot accidentally join an older batch if the scheduler has not flushed the older work yet.

The queue is per tick and the ordering is per pawn. Cross-pawn ordering is intentionally not part of the contract.

## Casualty Location

`CasualtyRecorder` no longer derives `Death` or `RelativeDeath` location from previously inserted records.

Crushed and friendly-trap deaths still preserve their event location even when the casualty subject is already despawned. `RecorderBase.ReconcilePriorityWriteRequest` is a default no-op hook. `CasualtyRecorder` overrides it to fill missing `Death` location from the previous same-pawn `Crushed` or `FriendlyTrapHit` record, and missing `RelativeDeath` location from the deceased pawn's already-written `Death` record.

## Invariants

- Non-priority record insertion remains immediate.
- Priority records are never inserted, removed, and reinserted.
- `HistoryRecord` creation for priority writes happens only during flush, so its date reflects the actual insertion tick.
- `TickDelayManager` remains generic and unchanged.
- `ClearAll` clears pending priority writes alongside the comp cache.

## Verification

Built successfully with:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false
```

The in-game recorder test suite is exposed through RimWorld debug actions, not a headless CLI runner in this repo.
