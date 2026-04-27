# GatheringBuilder

Starts a joinable gathering and returns the resulting lord plus organizers.

## Result Type

- `GatheringBuilderResult(Lord lord, List<Pawn> organizers)`: captures the created lord and the organizers involved.

## Constructor

- `GatheringBuilder(GatheringDef def)`: start a gathering builder for the given gathering def.

## Execution

- `Execute()`: try to start the gathering and return the result.

## Notes

- The builder handles gathering and marriage ceremony organizer extraction internally.
