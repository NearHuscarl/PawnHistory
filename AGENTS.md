# Agent Notes

Read in this order before changing code:
1. [ARCHITECTURE.md](ARCHITECTURE.md)
2. [docs/testing-tools/README.md](docs/testing-tools/README.md)
3. [docs/design-docs/core-beliefs.md](docs/design-docs/core-beliefs.md)
4. [docs/PLANS.md](docs/PLANS.md)
5. [docs/PRODUCT_SENSE.md](docs/PRODUCT_SENSE.md)
6. [docs/QUALITY_SCORE.md](docs/QUALITY_SCORE.md)
7. [docs/exec-plans/active/README.md](docs/exec-plans/active/README.md) and [docs/exec-plans/tech-debt-tracker.md](docs/exec-plans/tech-debt-tracker.md)

## What Lives Where

- [ARCHITECTURE.md](ARCHITECTURE.md): stable codebase map, build facts, runtime flow, and discovery behavior.
- [docs/design-docs/core-beliefs.md](docs/design-docs/core-beliefs.md): coding rules, testing rules, safety constraints, and style expectations.
- [docs/testing-tools/](docs/testing-tools/): recorder-author API references for the in-game test DSL.
- [docs/PLANS.md](docs/PLANS.md): how active plans, completed plans, and tech debt tracking are organized.
- [docs/PRODUCT_SENSE.md](docs/PRODUCT_SENSE.md): what is worth recording and how history entries should read.
- [docs/QUALITY_SCORE.md](docs/QUALITY_SCORE.md): the rubric for judging recorder work.
- [docs/exec-plans/](docs/exec-plans/): active backlog, technical debt, and archived execution notes.

## Implementation Plan Records

When a planned implementation is executed, write a proper implementation record into `docs/exec-plans/completed/` so future agents can find the reasoning later. See [docs/PLANS.md](docs/PLANS.md).

Use [docs/exec-plans/completed/pawn-generation-timeline-simulator.md](docs/exec-plans/completed/pawn-generation-timeline-simulator.md) as the quality bar:

- explain the behavior and why it matters before listing mechanics
- include a meaningful `## Summary`
- add high-signal sections like scope, design, rules, and verification when relevant
- capture exclusions, invariants, and what was actually tested

Do not leave behind a minimal bullet dump that only restates files changed.

## Agent skills

### Issue tracker

Issues are tracked in GitHub for this repository. See `docs/agents/issue-tracker.md`.

### Triage labels

Use the default triage labels: `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, and `wontfix`. See `docs/agents/triage-labels.md`.

### Domain docs

Treat this repo as single-context. Read the root `CONTEXT.md` and `docs/adr/` when they exist. See `docs/agents/domain.md`.
