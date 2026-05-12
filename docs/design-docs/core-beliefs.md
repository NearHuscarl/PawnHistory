# Core Beliefs

## Recorder Design

- Prefer modern C# already used in the repo: `record`, collection expressions, and concise APIs.
- Each recorder and event class must live in its own file.
- Keep recorder logic small and event-focused.
- `CreateRecord(...)` input should be domain-specific. Map generic upstream data in `Register()`.
- For tale-based recorders, put `TaleDef` and `Params` parsing in a typed `TaleDispatcher`.
- Call `ShouldRecord(...)` inside `CreateRecord(...)` before writing.
- Prefer rulepacks through `descriptionMaker` and finish with `Resolve()`.
- Use literal feature naming: `XyzEvent`, `XyzRecorder`, `HistoryRecordDefOf.Xyz`.
- If transient patch state is needed, use a dedicated `XyzContext` in the event file.
- Use RimWorld `DefOf` first. Use `Source/Extra.cs` only when no suitable `DefOf` exists, via `Extra.XyzDefOf` lookups.
- Put reflected field and method access in `Source/Accessor.cs`. Do not use raw `AccessTools` outside it.
- Use the simplest viable Harmony state.
- Reset patch context in `Finalizer()`.
- Transpilers and IL manipulation are out of bounds.

## Testing

- Always write tests with the internal test API.
- Tests belong next to the recorder they validate.
- Supported signatures are:
  - `Test(TestScenario scenario)`
  - `Test(TestScenario scenario, int count)`
- Prefer scenario builders over manual setup.
- Trigger the real game code path that reaches the Harmony patch.
- Do not publish `GameEventBus` directly from tests.
- Use `DebugValuesAttribute` for parameterized tests.
- Prefer `ToHaveHistoryRecord(...)` over `ToHaveHistoryRecordOf(...)` when a matching def already narrows the assertion.

## Workflow

When adding an event or recorder:
1. Update the XML `HistoryRecordDef` in `Defs/`.
2. Add the `HistoryRecordDefOf` field.
3. Check for an existing event or patch first.
4. Add the Harmony patch and typed event if needed.
5. Implement `RecorderBase<TEvent>`.
6. Add recorder-local tests when coverage is needed.
7. override `RecorderBase.GetBackfillDefinitions()` if event emits during pawn generation.

For tale-based recorders specifically:
1. Add a typed `XyzEvent` and `XyzDispatcher` under `Source/PawnTracker/Events/`.
2. Subscribe the recorder directly with `GameEventBus.Subscribe<XyzEvent>(CreateRecord)`.

## Safety

- Harmony patches should stay local in scope and easy to reason about.
- Do not rely on pawn references staying spawned or alive.
- Stable guidance belongs in docs, not in random source comments.
