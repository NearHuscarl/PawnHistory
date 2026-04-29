# Testing Tools

This folder is the first-read reference for recorder authors using the in-game test API.

If you are writing or updating recorder tests, read the referenced API notes before opening `Source/PawnTracker/Test/*.cs`. Only read the code when the docs do not answer the question.

Read in this order:
1. [test-scenario.md](test-scenario.md)
2. [scenario.md](scenario.md)
3. [assertions.md](assertions.md)
4. [test-attributes.md](test-attributes.md)
5. [runner-boundaries.md](runner-boundaries.md)

## Scope

These docs cover the public testing surface you are expected to use when writing recorder tests:

- `TestScenario`
- builder APIs returned by `TestScenario`
- `Expect`, `SimpleAssertions<T>`, and `PawnHistoryAssertions`
- test attributes used by recorder methods

## Boundary

Treat `Source/PawnTracker/Test/` as having two layers:

- Author-facing DSL: use this first and prefer it by default.
- Runner internals: inspect only when the API docs do not explain behavior or when debugging the test framework itself.

Runner internals are summarized in [runner-boundaries.md](runner-boundaries.md) so you can usually avoid source-diving.
