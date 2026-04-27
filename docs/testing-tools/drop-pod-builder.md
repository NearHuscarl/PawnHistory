# DropPodBuilder

Launches transport pods and optionally runs an arrival action afterward.

## Constructor

- `DropPodBuilder(List<Thing> things)`: start a drop-pod builder for the given payload.

## Setup

- `Position(PlanetTile tile)`: choose the destination tile.
- `Do(Action<Caravan> action)`: run a processor after arrival.

## Arrival Flows

- `Visit(Site site)`: arrive at a site.
- `Attack(Settlement settlement)`: attack a settlement.
- `Enter(MapParent mapParent)`: enter a map parent through a landing cell.
- `Visit(Settlement settlement)`: visit a settlement.
- `Trade(Settlement settlement)`: trade with a settlement.
- `ArriveAsGifts(Settlement settlement)`: arrive as gifts.
- `GiveToCaravan(Caravan caravan)`: transfer items to an existing caravan.
- `FormCaravan(PlanetTile tileInput)`: form a caravan on the target tile.

## Execution

- `Execute(bool launch = true)`: spawn launchers and launch the transport pods.
