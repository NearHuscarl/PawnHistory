# Agent Notes

## Overview

`PawnHistory` is a RimWorld 1.6 mod that records notable pawn events and exposes them through history UI.

Primary flow:
1. Harmony patch publishes typed event from `Source/PawnTracker/Events/`.
2. Recorder in `Source/PawnTracker/Recorders/` subscribes in `Register()`.
3. Recorder filters with `ShouldRecord(...)`, resolves text, and appends `HistoryRecord`.
4. Storage/UI live under `Source/PawnTracker/` and `Source/WorldPawn/`.

Key folders:
- `Source/PawnTracker/Events/`: Harmony patches and event record types.
- `Source/PawnTracker/Recorders/`: recorder implementations.
- `Source/PawnTracker/Test/`: in-game test framework, builders, assertions, reporting.
- `Defs/`: XML defs for history records, UI tables, buttons, and rule packs.
- `Languages/English/Keyed/`: localization keys.
- `Assemblies/`: compiled output consumed by RimWorld.

Startup: `Source/PawnTracker/PawnTracker.cs`.

## Build

- Classic non-SDK `.csproj`
- Target framework: `.NET Framework 4.7.2`
- LangVersion: `14.0`
- Output: `Assemblies\PawnHistory.dll`
- Local Harmony assembly reference
- Rider uses MSBuild
- Shell build: `C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe PawnHistory.csproj /t:Build /p:Configuration=Debug`

## Environment

- Windows project: prefer PowerShell-native commands over `rg`
- Do not inspect RimWorld DLLs unless needed
- If inspection is needed, use `Mono.Cecil` on `RimWorldWin64_Data/Managed/Assembly-CSharp.dll`

## Testing

- Always write test, using the internal test API
- Tests are public `Test...` methods on recorder classes, discovered by reflection in `Source/PawnTracker/RecorderManager.cs`
- Supported signatures:
  - `Test(TestScenario scenario)`
  - `Test(TestScenario scenario, int count)`
- Keep tests next to the recorder they validate
- Prefer scenario builders over manual setup
- Trigger the real game code path that reaches the Harmony patch; do not publish `GameEventBus` directly from tests
- Use `DebugValuesAttribute` for parameterized tests
- `ToHaveHistoryRecord(...)` already asserts the matching record when given a def; avoid using `ToHaveHistoryRecordOf(...)

## Implementation rules

- Prefer modern C# already used in the repo: `record`, collection expressions, concise APIs
- Code must be readable. Transpiler in harmony is forbidden.
- Keep recorder logic small and event-focused
- `CreateRecord(...)` input must be domain-specific; map generic upstream events in `Register()`
- Call `ShouldRecord(...)` inside `CreateRecord(...)` before writing
- Prefer rulepacks through `descriptionMaker`; resolve with `Resolve()`
- Use literal feature naming: `XyzEvent`, `XyzRecorder`, `HistoryRecordDefOf.Xyz`
- If transient patch state is needed, use a dedicated `XyzContext` in the event file
- Use RimWorld `DefOf` first; use `Source/DefLookup.cs` only when no suitable `DefOf` exists
- Put reflected field/method access in `Source/Accessor.cs`; do not use raw `AccessTools` outside it
- Use the simplest viable Harmony state
- Reset context in `Finalizer()`

## When adding an event/recorder

1. Update XML `HistoryRecordDef` in `Defs/`
2. Add the `HistoryRecordDefOf` field
3. Check for an existing event/patch first
4. Add the Harmony patch and typed event if needed
5. Implement `RecorderBase<TEvent>`
6. Add recorder-local tests when coverage is needed

## Important behavior

- Recorder discovery is reflection-based over non-abstract `RecorderBase` subclasses
- Tests are reflection-based and require the supported signatures exactly
- `RecorderManager.ShouldRecord(Pawn)` records human likes and bonded animals only

## Safety

- Harmony patches are global; keep them narrow and deterministic
- Pawn references may be dead, despawned, or world-only
