# PawnHistory

This mod expands RimWorld's storytelling aspect by tracking significant pawn events and the small tales of
their daily life on the rim, turning them into a clear, flavorful narrative records.

## Data flow

```
[Harmony Patch] ← EventContext (tracking more complex state to create event data)
    ↓
GameEventBus.Publish(GameEvent)
    ↓
(Event received)
    ↓
Recorder (map event → domain object) → Recorder.Register() → subscribes to events on start up
    ↓
CreateRecord(input) (filter, generate description) ← Backfill simulator (external trigger)
    ↓
AddRecord() (factory)
    ↓
History priority sort (for same-tick events)
    ↓
HistoryCompManager → HistoryRecord[]
```

## Implementing a recorder

A **Recorder** is the core logic unit responsible for:

- Subscribing to events
- Filtering relevant pawns
- Producing history records

Start by defining a new `HistoryRecordDef`:

```xml
<Defs>
  <!-- other defs... -->
	<HistoryRecordDef Class="PawnHistory.Source.PawnTracker.HistoryRecordDef">
		<defName>Anesthetized</defName>
		<label>anesthetized</label>
		<description>{PAWN} was put under {ANESTHETIC}.</description>
		<icon>EventIcons/Sleep</icon>
		<categories>
			<li>Health</li>
		</categories>
	</HistoryRecordDef>
</Defs>
```

Register it in code:

```c#
[DefOf]
public class HistoryRecordDefOf
{
    public static HistoryRecordDef Anesthetized;
    // ...
}
```

If the event does not already exist, create one using Harmony patch. Before adding a new event,
check the `Events/` folder for the full list of supported events first. Here is an example:

```cs
public record HediffAddedEvent(Pawn Pawn, Hediff Hediff, BodyPartRecord Part, DamageInfo? Dinfo) : GameEventBase;

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), [typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageResult)])]
internal class Pawn_HealthTracker_AddHediff_Patch
{
    static void Postfix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        var pawn = PawnRef(__instance);
        GameEventBus.Publish(new HediffAddedEvent(pawn, hediff, hediff.Part, dinfo));
    }
}
```

Create a Recorder for the event. The `Register()` method is called automatically at startup.
Use `ShouldRecord()` to filter for the kinds of pawns currently supported by the mod.

```c#
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class HediffRecorder : RecorderBase<HediffAddedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<HediffAddedEvent>(e =>
        {
            if (e.Hediff.def == HediffDefOf.Anesthetic)
                CreateRecord(e);
        });
    }

    public override void CreateRecord(HediffAddedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var desc = HistoryRecordDefOf.Anesthetized.Description(pawn)
            .AddRule("ANESTHETIC", hediff)
            .Format();

        AddRecord(HistoryRecordDefOf.Anesthetized, pawn, desc);
    }
}
```

![image](Images/anesthetic.png)

## Testing

PawnHistory is shipped with a custom test framework to ease up the work of setting up the scenario
and creating assertions, Core components:

- `XyzBuilder` like `MapBuilder`, `PawnBuilder`, `ThingBuilder`...: used together to construct a scenario for testing. they are often created through `TestScenario` class:

```c#
public class TestScenario
{
    public PawnBuilder Pawn(int count = 1) => new(count);
    public PawnBuilder Pawn(IEnumerable<Pawn> pawns) => new PawnBuilder().WithPawns(pawns);
    public PawnBuilder Pawn(Pawn pawn) => Pawn([pawn]);
    public GatheringBuilder Incident(GatheringDef def) => new(def);
    public IncidentBuilder Incident(IncidentDef def) => new(def);
    public MapBuilder Map(IntVec3? pos = null) => new(pos);
    public ThingBuilder Thing(ThingDef thingDef) => new(thingDef);
   // ...
```

- `TestManager`: Coordinates execution of multiple tests in sequence and creates a test report.
- `TestContext` (internal): context for the current running test: number of passed/failed assertions,
ongoing delayed assertions..
- `SkipTestAttribute`, `DebugValuesAttribute`, `RequiresAttribute`: Customization for test methods.
  - Additional attributes on top of `RequiresAttribute` for convenience: `[RequiresRoyalty]`,
  `[RequiresBiotech]`, `[RequiresIdeology]`, `[RequiresAnomaly]`, and `[RequiresOdyssey]` when a test depends on DLC content.
- `Expect`, `PawnHistoryAssertions`: Create test assertions.
- `RecorderManager`: Provides debug buttons in the devtool to run tests. You can run all tests, specific test, tagged 
tests, last-failed tests and dlc-specific tests.

## Writing test

To write a test case for your recorder, create a method that starts with `Test`:

```c#
internal class HediffRecorder : RecorderBase
{
    public override void Register() { /* ... */ }
    public override void CreateRecord(HediffAddedEvent e) { /* ... */ }

    public void Test(TestScenario scenario)
    {
        var patient = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .ThatMatches(p => p.health.hediffSet.hediffs.All(h => h.def != HediffDefOf.Anesthetic))
            .AddHediff(HediffDefOf.Anesthetic)
            .CreateSingle();

        Expect.That(patient).ToHaveHistoryRecord("[PAWN] was put under anesthetic.");
    }

    public void TestInvert(TestScenario scenario)
    {
        var patient = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .FullHeal()
            .CreateSingle();

        Expect.That(patient).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.Anesthetized);
    }
}
```

Use `Expect.Assertions(n)` when the test registers assertions later from a callback, delayed tick, or other deferred flow. It declares the exact number of assertions that must eventually run before the test can pass.

### Run tests

- Enable Development Mode
- Open: Debug Actions Menu
- Navigate to: Pawn History
  - → Run All Tests
  - → Run Tagged Tests
  - → Run Last Failed Tests
  - → Run Specific Tests... → `[RecorderClass_TestMethod]`: Run specific test defined in  `RecorderClass.TestMethod()` 

### Stop an ongoing test run

- Enable Development Mode
- Open: Debug Actions Menu
- Navigate to: Pawn History → Stop Test Run
