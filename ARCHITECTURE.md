# Architecture

## Overview

`PawnHistory` is a RimWorld 1.6 mod that records notable pawn events and exposes them through history UI.

Primary flow:
1. A Harmony patch publishes a typed event from `Source/PawnTracker/Events/`.
2. A recorder in `Source/PawnTracker/Recorders/` subscribes in `Register()`.
3. The recorder filters with `ShouldRecord(...)`, resolves description text, and appends `HistoryRecord`.
4. Storage and UI live under `Source/PawnTracker/` and `Source/WorldPawn/`.

Startup entrypoint: `Source/PawnTracker/PawnTracker.cs`.

## Key Folders

- `Source/PawnTracker/Events/`: Harmony patches and typed event records.
- `Source/PawnTracker/Recorders/`: recorder implementations.
- `Source/PawnTracker/Test/`: in-game test tooling, builders, assertions, and runner internals.
- `Source/WorldPawn/`: world-pawn UI and related behavior.
- `Defs/`: XML defs for history records, UI tables, buttons, and rule packs.
- `Languages/English/Keyed/`: localization keys.
- `Assemblies/`: compiled output consumed by RimWorld.

## Build

- Project type: classic non-SDK `.csproj`
- Target framework: `.NET Framework 4.7.2`
- LangVersion: `14.0`
- Output: `Assemblies\PawnHistory.dll`
- Harmony reference: local assembly reference
- Preferred shell build:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' PawnHistory.csproj /t:Build /p:Configuration=Debug
```

## Environment

- Windows project: prefer PowerShell-native commands over Unix-oriented tooling.
- Prefer inspecting the decompiled RimWorld source tree at `%USERPROFILE%\Desktop\rimworld\rimworld-2025`.
- Base game C# source lives under `%USERPROFILE%\Desktop\rimworld\rimworld-2025\Source\`.
- DLC defs live under `%USERPROFILE%\Desktop\rimworld\rimworld-2025\[DlcName]\Defs\`.
- Do not inspect RimWorld DLLs. If source does not exist, stop the task.

## Discovery Behavior

- Recorder discovery is reflection-based over non-abstract `RecorderBase` subclasses.
- Test discovery is reflection-based over public `Test...` methods with supported signatures.
- `RecorderManager.ShouldRecord(Pawn)` only records humanlikes and bonded animals.

## Runtime Constraints

- Harmony patches are global. Keep them narrow and deterministic.
- Pawn references may be dead, despawned, or world-only.
- Rulepacks and defs are part of the runtime contract, not optional decoration.
