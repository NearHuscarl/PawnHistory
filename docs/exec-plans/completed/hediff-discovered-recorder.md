# Hediff Discovered Recorder

Pawn histories should capture meaningful health discoveries when the game reveals a hidden condition. This work adds a generic discovery path for `HediffComp_Discoverable` so overdose is recorded through the same event surface future discoverable hediffs can use, instead of relying on overdose-specific recorder logic.

## Summary

The implementation records a history entry when vanilla `HediffComp_Discoverable.CheckDiscovered` changes its private discovery state from undiscovered to discovered. The recorder writes on the affected pawn, describes the discovered hediff with the existing hediff noun helper, and intentionally excludes infection-family hediffs because infection needs a separate recorder.

## Shipped Scope

- Added `HediffDiscoveredEvent` with the affected pawn, hediff, and body part.
- Added `HediffDiscoveredRecorder`, `HistoryRecordDefOf.HediffDiscovered`, a `HediffDiscovered` XML def, and `PH_HediffDiscovered` rulepack text.
- Added an overdose-focused recorder test that creates a visible `DrugOverdose` hediff and reaches discovery through `pawn.health.AddHediff`.
- Kept `WoundInfection` and `ScariaInfection` out of the generic recorder.

## Design

The Harmony patch targets `HediffComp_Discoverable.CheckDiscovered` because that is the narrow game method where vanilla decides a discoverable hediff has become known. The patch captures the private `discovered` field before and after the original method and publishes only on the false-to-true transition, avoiding duplicate records from later ticks, add hooks, or death notifications.

The recorder stays generic by receiving the hediff itself and formatting it through `LabelNounPretty()`. For overdose, this resolves to "a drug overdose"; for future discoverable conditions, the same path can produce useful condition text without changing the event contract.

## Rules and Exclusions

- Record only if `RecorderManager.ShouldRecord(pawn)` allows the pawn.
- Do not record `WoundInfection` or `ScariaInfection`; infection remains future work.
- Do not backfill discovery records, because discovery is a runtime transition.
- Do not publish from tests directly; tests must reach the Harmony patch through real health code.

## Verification

- Built with `MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false`.
- Result: build succeeded with 0 warnings and 0 errors.
