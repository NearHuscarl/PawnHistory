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

- Whenever an implementation follows a plan, record that plan in full details in `docs/exec-plans/completed/<short-name>.md`.
- If work was first discussed in Plan Mode and the user then says to implement it, execute the work and add the completed plan file as part of the implementation.

## Intent

Plans should be concrete enough to execute, but they are not the same thing as stable architecture docs. Stable guidance belongs in `ARCHITECTURE.md`, `core-beliefs.md`, and the testing-tool docs.
