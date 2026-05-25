# Surgery Event Data Sources

Surgery history needs to stay easy to extend because RimWorld represents many player-facing procedures through the same `Recipe_Surgery` flow. The implementation now keeps the Harmony capture path shared while letting each supported surgery shape provide its own event-layer data in a separate file.

## Summary

Surgery capture now publishes one common `SurgeryEvent`. The event carries shared surgery facts: recipe, patient, doctor, body part, outcome, and new injuries from the operation. Recipe-specific facts are attached as typed `SurgeryEventData` records created by event-layer data source classes.

Recorder comps no longer depend on separate typed surgery events or duplicate patch context. They match the typed `SurgeryEvent.Data` payload and continue to own record selection, grammar rules, and concerns.

## Shipped Scope

- Replaced per-surgery context classes with shared surgery event handling while preserving the original four `ApplyOnPawn` patch targets.
- Added `SurgeryEventDataSource` discovery for event-layer files that match `RecipeDef` and create typed surgery data.
- Converted install implant, install natural part, install artificial part, and remove body part support to typed `SurgeryEventData` payloads.
- Updated surgery recorder comps to consume typed data payloads while preserving existing record definitions, grammar, concerns, botched-surgery handling, and tests.
- Kept `RecipeWorker` out of the published `SurgeryEvent` contract; the event exposes `RecipeDef`.

## Design

`SurgeryEvent.cs` owns common capture through one Harmony patch with a `TargetMethods()` whitelist for the same four surgery recipe methods as before. `SurgeryContext` only keeps the published event plus the pre-surgery injury snapshot needed to compute botched-surgery injuries.

Specific surgery files define two things:

- a typed `SurgeryEventData` record containing fields needed by recorder logic
- a `SurgeryEventDataSource` with `Match(RecipeDef)` and `Create(...)`

The shared event path discovers data sources once with `AllSubclassesNonAbstract()`, then uses the same comp-style matching shape already used by recorders. Runtime event data is selected from `RecipeDef` and the current surgery facts, not from the published event's grammar layer.

The recorder layer receives already-prepared event facts. It does not reconstruct the event and the event layer does not know description grammar.

## Extension Rule

For a new supported `Recipe_Surgery` case, add a new event-layer data file and a recorder comp:

- event file: create a `SurgeryEventData` subtype and a `SurgeryEventDataSource` that matches the relevant `RecipeDef`
- recorder file: create or update a `SurgeryComp_*` that matches the data subtype and builds the record

Do not add a new Harmony patch unless the procedure does not flow through `Recipe_Surgery`.

## Verification

Built with:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug /p:UseSharedCompilation=false
```

Result: build succeeded with 0 warnings and 0 errors.
