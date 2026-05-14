# CaravanBuilder

Creates a caravan, chooses one arrival flow, then runs post-arrival processors.

## Setup

- `scenario.Caravan(List<Pawn> pawns)`: start a caravan builder from the given pawns.
- `Position(PlanetTile tile)`: set the caravan destination tile.
- `Do(Action<Caravan> action)`: queue a processor to run after the caravan arrives or is created.
- `OnMapGenerated(Action<MapGeneratedEvent> action)`: subscribe once to the generated target map for this caravan trip.
- `KillAllEnemies(Action<MapGeneratedEvent> onFinished = null)`: kill spawned free humanlike enemies on the generated target map, then optionally run a callback.

## Arrival Flows

- `Camp()`: move to a nearby tile and set up camp.
- `Attack(Settlement settlement)`: arrive and attack a settlement.
- `Enter(MapParent mapParent)`: arrive and enter a map parent.
- `Visit(Settlement settlement)`: arrive and visit a settlement peacefully.
- `VisitEscapeShip(MapParent mapParent)`: arrive at an escape-ship map parent.
- `VisitPeaceTalks(PeaceTalks peaceTalks)`: arrive at peace talks.
- `VisitSite(Site site)`: arrive at a site.
- `OfferGifts(Settlement settlement)`: arrive and perform gift-mode trade with a settlement.
- `FulfillTradeRequest(Settlement settlement)`: fulfill an active settlement trade request.
- `Trade(Settlement settlement)`: arrive and perform normal settlement trade.
- `Give(List<Thing> things)`: add items to caravan inventory before or after arrival.

## Execution

- `Execute()`: create the caravan, fire the arrival action immediately when one is set, run queued processors, and return the caravan.
