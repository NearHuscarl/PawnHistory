# IdeoBuilder

Creates a fresh player-style ideology for the current test, then applies queued mutations.

## Setup

- `scenario.Ideo()`: start an ideology builder.
- `AddPrecept(PreceptDef preceptDef)`: queue a precept to add to the created ideology.

## Execution

- `Execute()`: generate a fixed player-style ideology, register it with the ideo manager, apply queued precepts, and return it.
