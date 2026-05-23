# Surgery Recorder Comp Refactor

Surgery history records describe the same domain event family with several different concrete outcomes: implants, natural part installs, artificial part installs, and removals. The old implementation represented those variants as recorder subclasses, which made the shared botched-surgery behavior live in an inheritance base while each concrete recorder still duplicated the event subscription and record-writing shape.

This refactor moves surgery variants to record comps so the recorder owns the common surgery pipeline and each comp owns only the rule names and constants for its specific record type.

## Summary

`SurgeryRecorder` is now the single concrete recorder for surgery events. It subscribes to all supported surgery event subtypes, resolves the matching `SurgeryComp`, adds common grammar such as `[Doctor]`, and writes either the success record or the common `BotchedSurgery` record.

The previous generic surgery recorder base class was removed. Specific surgery behavior now lives in `SurgeryComp_Implant`, `SurgeryComp_InstallPart`, `SurgeryComp_ModPart`, and `SurgeryComp_RemovePart`.

## Shipped Scope

- Replaced `SurgeryRecorder<TInput>` inheritance with one `SurgeryRecorder : RecorderBase<SurgeryEvent>`.
- Added `SurgeryComp` as the record-comp extension point for surgery variants.
- Moved all variant-specific rule names and constants into the matching comp.
- Kept common rule setup in `SurgeryRecorder`, including `[Doctor]`, botched severity, injured parts, and bloodloss.
- Moved surgery tests onto the relevant comp classes and added assertions for record def, description shape, and exact doctor concern.
- Added a surgery outcome mock and static outcome catalog to force surgery success or failure through the real surgery path.

## Design

The recorder dispatches by matching the runtime `SurgeryEvent` subtype against registered comps. This keeps event subscription in one place while preserving exact `GameEventBus` subscriptions for the concrete event types, because the bus dispatches by exact event type.

Success records use the comp's record def and grammar builder. Botched records use the same comp to build the operation-specific `BotchedSurgery` fragment, then the recorder wraps that fragment in the shared botched-surgery rulepack.

## Testing Support

`ForcedSurgeryOutcome` is backed by a test mock on `SurgeryOutcomeEffectDef.GetOutcome` and reads the selected outcome from `TestScenario.SurgeryForcedOutcome`, matching the existing scenario-owned mock pattern. It calls the selected vanilla outcome's own `Apply(...)` once while a scoped `SurgeryOutcome_Failure.CanApply(...)` mock returns true for that selected outcome, returns the selected outcome, and skips the normal outcome scan. Failure effects stay owned by RimWorld's `SurgeryOutcome` classes.

`SurgeryOutcomes` exposes forceable vanilla outcomes by their `SurgeryOutcomeBase` order: success, death, catastrophic failure, ridiculous failure, IUD sterilization failure, vasectomy sterilization failure, and minor failure. The minor-only failure helper reads the failure at index 1 from `SurgeryOutcomeMinorFailure`.

## Verification

Verified with:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false
```

The build completed with 0 warnings and 0 errors.
