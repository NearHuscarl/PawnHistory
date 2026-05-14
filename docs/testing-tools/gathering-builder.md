# GatheringBuilder

Starts a joinable gathering from a `GatheringDef`.

## Setup

- `scenario.Incident(GatheringDef def)`: start a gathering builder for the given gathering def.

## Execution

- `Execute()`: start the gathering and return its result.

## Result

- `GatheringBuilderResult.Lord`: the created gathering lord.
- `GatheringBuilderResult.Organizers`: the organizer pawns extracted from the started gathering.
