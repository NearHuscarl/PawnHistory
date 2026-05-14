# RitualBuilder

Queues ritual interactions for an organizer pawn, then runs them through the real ability and ritual path.

## Setup

- `scenario.Ritual(Pawn organizer)`: start a ritual builder for the organizer pawn.
- `Outcome(RitualOutcomePossibility outcome)`: force the ritual outcome stored on `TestScenario`.
- `ThroneSpeech(List<Pawn> spectators)`: run a throne-speech ritual and apply its outcome using the given spectators.
- `ConversionRitual(Pawn convertee)`: run a conversion ritual on the target pawn.

## Execution

- `Execute()`: run every queued ritual action in order.
