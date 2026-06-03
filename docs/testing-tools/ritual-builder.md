# RitualBuilder

Queues ritual interactions for an organizer pawn, then runs them through the real ability and ritual path.

## Setup

- `scenario.Ritual(Pawn organizer)`: start a ritual builder for the organizer pawn.
- `Outcome(RitualOutcomePossibility outcome)`: force the ritual outcome stored on `TestScenario`.
- `ThroneSpeech(List<Pawn> spectators)`: run a throne-speech ritual and apply its outcome using the given spectators.
- `ConversionRitual(Pawn convertee, List<Pawn> spectators)`: run a conversion ritual on the target pawn through the ideogram ritual-gizmo path.
- `Execution(Pawn prisoner, List<Pawn> spectators = null)`: run a public execution ritual using the organizer as executioner and the target prisoner as the forced ritual victim. Requires the organizer's ideoligion to include the `Execution` precept and the map to have an ideogram bound to that ideology.
- `Funeral(Pawn deceased, List<Pawn> spectators, bool noCorpse)`: run a funeral ritual for a pawn with an active funeral obligation and a matching grave. Requires the organizer's ideoligion to include the `Funeral` precept, and the organizer is assigned as the moralist speaker.
- `AnimaTreeLinking(Thing animaTree, List<Pawn> spectators)`: run an anima tree linking ritual on the given tree using the selected organizer as the linker. The organizer and spectators still need valid natural-focus setup and enough tracked anima grass on the tree to pass the real ritual checks.

## Execution

- `Execute()`: run every queued ritual action in order.
