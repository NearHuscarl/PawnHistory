# Runner Boundaries

Most recorder authors should not need to inspect the test runner implementation.

## Author-Facing by Default

Stay within:

- `TestScenario`
- builder APIs
- `Expect`
- `PawnHistoryAssertions`
- test attributes

## Runner Internals

These are framework internals, not primary author APIs:

- `TestManager.cs`: queueing, per-test setup, assertion completion, timeout handling, and cleanup.
- `TestReportManager.cs`: prints the summary and persists the last run report.
- `TestFailure.cs`: serializable failure types used by assertions, execution, and timeout reporting.
- `TestReport.cs`: serializable test report and report entry models.
- `TestContext.cs`: per-test state, assertion counting, and cleanup callbacks.

## When To Read Them

Inspect runner internals only when:

- compiling error in that specific internal file.
- you are extending the test framework itself rather than only using it.
