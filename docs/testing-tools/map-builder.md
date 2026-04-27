# MapBuilder

Creates rooms, fixtures, and other map structures around the current map.

## Constructor And Entry Points

- `MapBuilder(IntVec3? center = null)`: build around a center point.
- `At(IntVec3 pos)`: create a builder anchored at a position.
- `AtMouse()`: create a builder anchored at the current mouse cell.

## Room And Map Setup

- `GenerateAncientTemple(int width, int height)`: generate an ancient temple layout.
- `BuildRoom(CellRect rect, string tag = null, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null)`: create a generic room.
- `BuildRoom(int width, int height, string tag = null, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null)`: create a centered generic room.
- `AsBarrack(List<Pawn> assignedPawns)`: turn the last built room into a barrack and claim beds.
- `AsBarrack(int bedCount = 3)`: turn the last built room into a barrack with the given bed count.
- `AsHospital(int bedCount, List<Building_Bed> beds = null)`: build a hospital room and optionally collect beds.
- `AsPrison(int prisonerCount, int bedCount = 1, List<Pawn> prisoners = null)`: build a prison room and optionally collect prisoners.
- `AsBank(int silvers = 5000)`: build a trade-room style bank.
- `AsThroneRoom(Pawn owner)`: build a throne room for the given pawn.

## Contents And Hazards

- `WithThing(ThingDef thingDef, int totalCount = 10, Faction faction = null)`: add items to the current room.
- `WithCasket(ThingDef thingDef, ThingDef stuff = null, bool occupied = true)`: spawn a casket, optionally occupied.
- `CollapseRoofAndCrush(Pawn pawn)`: drop a roof on the pawn.

## Spatial Helpers

- `Beside(string tag, Rot4 side, int w, int h)`: place a room beside a tagged room.

## Execution

- `Execute()`: run all queued map actions.
