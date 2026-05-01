# Plans

Execution planning lives under `docs/exec-plans/`.

## Layout

- `docs/exec-plans/active/`: active work, roadmap slices, and known future implementations.
- `docs/exec-plans/completed/`: archived plans that are still worth keeping as historical execution notes.
- `docs/exec-plans/tech-debt-tracker.md`: bugs, refactors, cleanup items, and structural debt.

## Movement Rules

- Put work in `active/` when it still describes future or ongoing implementation.
- Move a plan to `completed/` only when the work actually shipped or the plan remains useful as a retrospective artifact.
- Put cross-cutting cleanup, defects, and refactors in `tech-debt-tracker.md` instead of scattering them across feature plans.

## Implementation Plan Records

- Whenever an implementation follows a plan, write a real implementation record in `docs/exec-plans/completed/<short-name>.md`.
- If work was first discussed in Plan Mode and the user then says to implement it, execute the work and add the completed plan file as part of the implementation.
- Start with a short domain-facing explanation of the purpose and why it matters.
- Include a `## Summary` section that explains the shipped behavior.
- Include scoped sections such as `## Shipped Scope`, `## Design`, `## Rules` when they help future agents understand the implementation.
- Call out important exclusions, invariants, and player-facing semantics when those were part of the decision.
- Record what was actually verified.
- Do not rely on source diffs alone to preserve reasoning; the completed note should stand on its own months later.

## Intent

Plans should be concrete enough to execute, but they are not the same thing as stable architecture docs. Stable guidance belongs in `ARCHITECTURE.md`, `core-beliefs.md`, and the testing-tool docs.
