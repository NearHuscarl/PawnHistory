# ShuttleBuilder

Loads an existing `TransportShip`, then optionally launches it away.

## Setup

- `scenario.Shuttle(TransportShip transportShip)`: start a shuttle builder for an existing transport ship.
- `Load(IEnumerable<Thing> things)`: queue things to move into the shuttle container.
- `Load(IEnumerable<Pawn> pawns)`: queue pawns to move into the shuttle container.
- `Load(Thing thing)`: queue one thing.
- `Load(Pawn pawn)`: queue one pawn.

## Execution

- `Launch(bool sendAway = true)`: move the queued contents into the shuttle transporter and optionally send the ship away.
