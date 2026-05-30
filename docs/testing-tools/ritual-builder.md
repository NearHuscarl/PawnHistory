# RitualBuilder

Queues ritual interactions for an organizer pawn, then runs them through the real ability and ritual path.

## Setup

- `scenario.Ritual(Pawn organizer)`: start a ritual builder for the organizer pawn.
- `Outcome(RitualOutcomePossibility outcome)`: force the ritual outcome stored on `TestScenario`.
- `ThroneSpeech(List<Pawn> spectators)`: run a throne-speech ritual and apply its outcome using the given spectators.
- `ConversionRitual(Pawn convertee)`: run a conversion ritual on the target pawn through the ideogram ritual-gizmo path.
- `Execution(Pawn prisoner, List<Pawn> spectators = null)`: run a public execution ritual using the organizer as executioner and the target prisoner as the forced ritual victim. Requires the organizer's ideoligion to include the `Execution` precept and the map to have an ideogram bound to that ideology, for example with `scenario.Map().AsShrine(organizer.Ideo)`.

## Execution

- `Execute()`: run every queued ritual action in order.
