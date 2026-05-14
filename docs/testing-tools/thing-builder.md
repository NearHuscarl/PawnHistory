# ThingBuilder

Creates things, optionally spawns them, and runs per-thing processors.

## Setup

- `scenario.Thing(ThingDef def, ThingDef stuffDef = null)`: start a builder for a thing def and optional stuff.
- `At(IntVec3 pos)`: set the placement cell used when spawning.
- `Def(ThingDef def)`: replace the thing def before creation.
- `Stack(int count)`: set the total stack count before it is split into valid stacks.
- `MadeOf(ThingDef stuffDef)`: set or replace the stuff def.
- `Map(Map map)`: choose the target map for spawning.
- `Faction(Faction faction)`: assign a faction to created things when allowed.
- `PlaceMode(ThingPlaceMode placeMode)`: choose direct vs near placement behavior.
- `Quality(QualityCategory quality)`: set quality on things with `CompQuality`.
- `Do(Action<Thing> action)`: run a processor on each created thing before spawn.

## Creation

- `Create<T>(bool spawn = true)`: create a typed list and optionally spawn it.
- `Create(bool spawn = true)`: create an untyped list and optionally spawn it.
- `CreateSingle(bool spawn = true)`: create and return the first thing.
- `CreateSingle<T>(bool spawn = true)`: create and return the first typed thing.

## Extensions

- `PoisonFood(Pawn cook)`: mark created food as poisoned by the given cook.
- `AnyBook()`: replace the thing def with a random book def.
