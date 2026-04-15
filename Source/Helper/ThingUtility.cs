using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class ThingUtility
{
    public static PlanetTile? WorldLocation(this Thing thing)
    {
        PlanetTile? tile = null;
        var spawnedThing = thing.SpawnedParentOrMe;

        if (spawnedThing is { Spawned: true })
            tile = thing.Tile;
        else
            Log.Warning($"[PawnHistory] Cannot get location of {thing}");

        return tile;
    }
}
