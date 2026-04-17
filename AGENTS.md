# Agent Notes

## Project overview

`PawnHistory` is a RimWorld 1.6 mod that records notable pawn events and exposes them through history UI.

Primary flow:

1. Harmony patches detect gameplay activity and publish typed events through `GameEventBus`.
2. Recorder classes under `Source/PawnTracker/Recorders/` subscribe in `Register()`.
3. Recorders filter supported pawns with `ShouldRecord(...)`, build resolved text, and append `HistoryRecord` entries.
4. Storage and UI live under `Source/PawnTracker/` and `Source/WorldPawn/`.

Key folders:

- `Source/PawnTracker/Events/`: Harmony patches and event record types.
- `Source/PawnTracker/Recorders/`: recorder implementations.
- `Source/PawnTracker/Test/`: in-game test framework, builders, assertions, reporting.
- `Defs/`: XML defs for history records, UI tables, buttons, and rule packs.
- `Languages/English/Keyed/`: localization keys.
- `About/About.xml`: mod metadata and dependencies.
- `Assemblies/`: compiled output consumed by RimWorld.

Startup entry point is `Source/PawnTracker/PawnTracker.cs`. It patches Harmony, injects comps, and initializes recorders by reflection.

## Build

Project facts from `PawnHistory.csproj`:

- classic non-SDK `.csproj`
- target framework: `.NET Framework 4.7.2`
- language version: `14.0`
- output path: `Assemblies\`
- local Harmony assembly reference
- NuGet package: `Krafs.Rimworld.Ref` `1.6.4633`

Build in Rider. This repo is set up around Rider invoking MSBuild for the project rather than a guaranteed portable CLI flow.

Observed Rider build characteristics:

- Rider delegates to MSBuild.
- Shell builds can use `C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug`.

Build artifact:

- `Assemblies/PawnHistory.dll`

## Environment

- This project is worked on in Windows. Prefer PowerShell-native commands for file discovery, searching, and inspection instead of `rg`.
- Do not inspect RimWorld DLLs unless requested.
  - to inspect assembly on request, use `Mono.Cecil` on `RimWorldWin64_Data/Managed/Assembly-CSharp.dll`; NuGet reference assemblies may fail under normal runtime reflection.

## Testing

This repo does not use an external unit test runner. Tests are public methods named `Test...` inside recorder classes, discovered by reflection in `Source/PawnTracker/RecorderManager.cs`.

Supported signatures:

- `Test(TestScenario scenario)`
- `Test(TestScenario scenario, int count)`

Useful test framework entry points:

- `Source/PawnTracker/Test/TestManager.cs`
- `Source/PawnTracker/Test/TestScenario.cs`
- `Source/PawnTracker/Test/Expect.cs`
- `Source/PawnTracker/Test/PawnHistoryAssertions.cs`

Testing conventions:

- Keep tests next to the recorder they validate.
- Prefer scenario builders (`Pawn`, `Map`, `Incident`, `Thing`) over manual setup.
- For integration-style recorder tests, trigger the real RimWorld game code path that reaches the Harmony patch; do not call `GameEventBus.Publish()` directly from tests.
- If a test needs parameters, use `DebugValuesAttribute` rather than custom menus.
- Running tests and interpreting failures in RimWorld is a human step; keep agent guidance focused on writing and adjusting test code.

## Code style and implementation patterns

Observed conventions in the codebase:

- Use file-scoped namespaces.
- Prefer modern C# features already present in the repo: `record`, collection expressions `[]`, concise APIs.
- Keep recorder logic small and event-focused.
- Naming is literal and feature-based: `XyzEvent`, `XyzRecorder`, `HistoryRecordDefOf.Xyz`.
- Recorder tests live in the same class as the recorder.
- Prefer RimWorld `DefOf` classes for named defs; use `Source/DefLookup.cs` only for named defs that do not have a suitable `DefOf` entry.
- Put reflected field/method accessors in `Source/Accessor.cs`; prefer cached `AccessTools` delegates there over ad hoc Harmony `Traverse` usage.

When adding a new event/recorder:

1. Add or update the XML `HistoryRecordDef` in `Defs/`.
2. Add the corresponding `DefOf` field in `HistoryRecordDefOf`.
3. Check `Source/PawnTracker/Events/` for an existing event or patch before adding anything new.
4. If the event does not exist yet, add the Harmony patch and publish the typed event.
5. Implement a recorder inheriting `RecorderBase<TEvent>`.
6. Call `ShouldRecord(...)` before writing history.
7. Prefer rulepacks via `descriptionMaker` for extendability; resolve the final text with `Resolve()`.
8. Add recorder-local `Test...` methods when the feature needs coverage.

Important behavior:

- Recorder discovery is reflection-based over all non-abstract `RecorderBase` subclasses.
- Tests are reflection-based and require the exact supported signatures.
- `RecorderManager.ShouldRecord(Pawn)` currently records humanlikes and bonded animals only.

## RimWorld-specific pitfalls


## Security and safety considerations

- Harmony patches are global. Keep patches narrow and deterministic.
- Preserve save compatibility. Any new persisted data under comps or records must be safely exposable and resilient to old saves.
- Avoid assumptions about pawn lifetime. World pawns, corpses, and destroyed references are common edge cases in this mod.
- Local machine paths are baked into the project file for Harmony. Do not replace them blindly without confirming the developer environment.
- The mod runs inside the game process. Logging, debug actions, and test helpers should fail safely and avoid breaking live saves.
- Network access is not part of the mod's runtime model; keep features offline and deterministic.
