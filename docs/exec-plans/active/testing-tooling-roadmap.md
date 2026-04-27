# Testing Tooling Roadmap

Backlog for the internal test framework and authoring ergonomics.

## Framework Work

- Add retriable settings for flaky tests
- Consider an `ITestable` abstraction so testing is not coupled only to `RecorderBase`.
- Remove the test scanning code from `RecorderManager` to `Tests/`.

## Documentation and Authoring

- Document `DescriptionBuilder` and rulepack usage with test-oriented examples.
- Document tag-based test filtering and any future `TaggedTestAttribute` conventions.
- Add a workflow for running only the last failed tests.

## Fit And Finish

- Keep recorder-author docs ahead of implementation details so authors do not need to crawl test internals for common setups.
