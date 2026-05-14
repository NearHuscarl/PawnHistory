# Testing Tools

Start here for recorder tests. Prefer these docs over reading `Source/PawnTracker/Test/*.cs`.

Read in this order:
1. [TestScenario](test-scenario.md): scenario entry points, helper extensions, framework knobs.
2. [Scenario Builders](scenario.md): builder index for pawns, maps, incidents, quests, letters, and travel flows.
3. [Assertions](assertions.md): `Expect`, `SimpleAssertions<T>`, `PawnHistoryAssertions`.
4. [Test Attributes](test-attributes.md): author-facing test attributes.
5. [Runner Boundaries](runner-boundaries.md): what counts as test DSL vs runner internals.

Use the author-facing DSL first:
- `TestScenario`
- builder APIs returned by `TestScenario`
- `Expect(...)` assertions
- recorder test attributes

Only inspect runner internals when the docs do not explain behavior or when debugging the test framework itself.
