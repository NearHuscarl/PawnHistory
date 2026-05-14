# MapBuilder

Queues room and structure work on a map, then applies it in `Execute()`.

## Setup

- `scenario.Map(Map map = null)`: start a map builder for the given map or `Find.CurrentMap`.
- `GenerateAncientTemple(int width, int height)`: generate an ancient temple layout around map center.
- `BuildRoom(CellRect rect, string tag = null, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null)`: queue a room at an exact rect and optionally tag it.
- `BuildRoom(int width, int height, string tag = null, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null)`: queue a centered room of the given size.
- `Beside(string tag, Rot4 side, int w, int h)`: compute a new room rect beside a tagged room, or around the camera position if the tag is missing.

## Room Conversions

- `AsBarrack(List<Pawn> assignedPawns)`: add beds and claim them for the given pawns.
- `AsBarrack(int bedCount = 3)`: add unclaimed player beds to the last room.
- `AsHospital(int bedCount, List<Building_Bed> beds = null)`: add medical beds and ultratech medicine to the last room.
- `AsPrison(int prisonerCount, int bedCount = 1, List<Pawn> prisoners = null)`: add prisoner beds and spawn prisoner pawns into the room.
- `AsBank(int silvers = 5000)`: add silver, trade beacon, comms console, and power to the room.
- `AsThroneRoom(Pawn owner)`: add throne-room furniture and claim the throne for the owner.
- `AsShrine()`: place an ideogram in the room interior.

## Contents And Ownership

- `WithThing(ThingDef thingDef, int totalCount = 10, Faction faction = null)`: spawn a stack of the given thing in the room interior.
- `WithCasket(ThingDef thingDef, ThingDef stuff = null, bool occupied = true, Pawn pawn = null)`: place a casket or grave, optionally filling it with a given or generated pawn.
- `ClaimAllBuildings()`: claim all claimable non-colonist buildings on the map for the player.

## Hazards

- `CollapseRoofAndCrush(Pawn pawn)`: force a thick-roof collapse on the pawn's current cell.

## Execution

- `Execute()`: run every queued map action in order.

`BuildRoom(...)` updates `TestManager.Scenario.LastRoomRect`, and tagged rooms are stored in `TestManager.Scenario.TaggedRooms`.
