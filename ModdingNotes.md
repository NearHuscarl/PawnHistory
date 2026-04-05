## Pitfalls

- If your text is uncolored, make sure to resolve it if the text is `TaggedString`. When you assign a `TaggedString` (returned by .Formatted())
  to a variable explicitly typed as string, C# calls an implicit conversion operator. In RimWorld's code, this conversion is "lossy." It strips
  the color tags away and essentially return only the raw string.

```c#
// good
TaggedString colorizedText = rawText.Formatted(subjectPawn.NameShortColored);
string colorizedText = rawText.Formatted(subjectPawn.NameShortColored).resolve();
// bad
string colorizedText = rawText.Formatted(subjectPawn.NameShortColored);
```

- Dead `Pawn` is not spawned object, Use corpse to locate its position on the map instead.

```c#
// pawn.Dead = true
// pawn.Spawned = false
pawn.Tile; // bad
pawn.Corpse.Tile; // okay
```

- Dead `Pawn` is considered destroyed object, even though it is still 'physically' on the map as a `Corpse` object.
  If you want to save a reference of dead `Pawn`, use the 3rd param, otherwise it is set to `null`.

```c#
Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
```

- `IExposable` instances can be created in 2 different ways, make sure the data is initialized properly using this pattern.

```c#
public class CompCustom : ThingComp
{
    public List<ExposableItem> exposableItems;

    // 1. Created by the game code
    public CompCustom() => EnsureInitialized();

    private void EnsureInitialized()
    {
        exposableItems ??= [];
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        // 2. Loaded from the save game
        Scribe_Collections.Look(ref exposableItems, "exposableItems", LookMode.Deep);
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;

        EnsureInitialized();
    }
}
```

## Random Notes

GenTicks.TicksAbs

- The absolute tick count since the game was first created (the world's "epoch")
- Never resets, keeps incrementing regardless of what map you're on
- Use this for: timestamping events, calculating real elapsed time, anything needing a universal clock

Find.TickManager.TicksGame

- Ticks elapsed since the current game session started
- Use this for: gameplay logic tied to the current run, cooldowns, durations relative to "now" in the game
