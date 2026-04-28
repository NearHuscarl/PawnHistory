# ThingBuilder

Creates and optionally spawns things, then applies processors to each created stack.

## Constructor

- `ThingBuilder(ThingDef def, ThingDef stuffDef = null)`: start a builder for a thing, optionally made from specific stuff.

## Placement And Composition

- `At(IntVec3 pos)`: choose the placement cell.
- `Stack(int count)`: set stack count before splitting into stacks.
- `MadeOf(ThingDef stuffDef)`: override the material.
- `Map(Map map)`: choose the target map.
- `Faction(Faction faction)`: assign a faction.
- `PlaceMode(ThingPlaceMode placeMode)`: choose direct or near placement.

## Mutation

- `Quality(QualityCategory quality)`: set quality when the thing has `CompQuality`.
- `Do(Action<Thing> action)`: run a processor on each created thing.

## Creation

- `Create<T>() where T : Thing`: create and place the things/books.
- `Create()`: create things as `Thing`.
- `CreateSingle()`: create one thing.
- `CreateSingle<T>() where T : Thing`: create one thing as a typed result.
- `CreateAndPutInto<T>(Pawn pawn) where T : Thing`: create things and put them into a pawn inventory.
- `CreateAndPutInto(Pawn pawn)`: create things as `Thing` and put them into a pawn inventory.

## Extension

- `ThingBuilderExtensions.PoisonFood(ThingBuilder builder, Pawn cook)`: mark food as poisoned by the cook.
