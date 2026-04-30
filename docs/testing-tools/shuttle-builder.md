# ShuttleBuilder

Loads an existing shuttle or quest transport ship, then optionally sends it away.

## Constructor

- `ShuttleBuilder(TransportShip transportShip)`: start a builder for an existing spawned shuttle or quest transport ship.

## Setup

- `Load(IEnumerable<Thing> things)`: load things into the shuttle transporter.
- `Load(IEnumerable<Pawn> pawns)`: load pawns into the shuttle transporter.
- `Load(Thing thing)`: load one thing.
- `Load(Pawn pawn)`: load one pawn.

## Execution

- `Execute(bool sendAway = true)`: finish loading and optionally send the shuttle away through the real transport-ship path.
