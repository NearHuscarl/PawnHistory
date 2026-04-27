# CaravanBuilder

Builds and drives a caravan through world travel and settlement interactions.

## Constructor

- `CaravanBuilder(List<Pawn> pawns)`: start a caravan builder for the given pawns.

## Setup

- `Position(PlanetTile tile)`: choose the destination tile.
- `Do(Action<Caravan> action)`: run a processor on the caravan.
- `OnMapGenerated(Action<MapGeneratedForCaravanEvent> action)`: run a callback when the target map is generated.

## Travel And Interaction

- `Camp()`: make the caravan set up camp nearby.
- `Attack(Settlement settlement)`: arrive and attack a settlement.
- `Enter(MapParent mapParent)`: arrive on a map parent.
- `Visit(Settlement settlement)`: arrive and visit a settlement.
- `VisitEscapeShit(MapParent mapParent)`: arrive at an escape ship.
- `VisitPeaceTalks(PeaceTalks peaceTalks)`: arrive at peace talks.
- `VisitSite(Site site)`: arrive at a site.
- `OfferGifts(Settlement settlement)`: perform gift-style trade.
- `FulfillTradeRequest(Settlement settlement)`: fulfill a settlement trade request.
- `Trade(Settlement settlement)`: trade with a settlement.
- `Give(List<Thing> things)`: add items to the caravan inventory.

## Execution

- `Execute()`: create and move the caravan through the chosen flow.

## Event Notes

- `MapGeneratedForCaravanEvent` is a nested record used by `OnMapGenerated(...)`.
