# DropPodBuilder

Builds transport pods, launches them, and optionally runs a processor after the payload resolves into a caravan.

## Setup

- `scenario.DropPod(List<Thing> things)`: start a pod launch from item payload.
- `scenario.DropPod(List<Pawn> pawns)`: start a pod launch from pawn payload.
- `Position(PlanetTile tile)`: set the destination tile.
- `Do(Action<Caravan> action)`: queue a processor to run after landing resolves into a caravan.

## Arrival Flows

- `Visit(Site site)`: land at a site.
- `Attack(Settlement settlement)`: land and attack a settlement.
- `Enter(MapParent mapParent)`: land inside a map parent using its drop spot.
- `Visit(Settlement settlement)`: land and visit a settlement peacefully.
- `Trade(Settlement settlement)`: land and perform settlement trade.
- `ArriveAsGifts(Settlement settlement)`: land as gifts for a settlement.
- `GiveToCaravan(Caravan caravan)`: transfer the payload to an existing caravan.
- `FormCaravan(PlanetTile tileInput)`: land and form a caravan on the target tile.

## Execution

- `Execute(bool launch = true)`: spawn launchers and pods, load the payload, and optionally launch immediately.
