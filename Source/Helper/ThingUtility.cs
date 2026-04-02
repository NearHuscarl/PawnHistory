using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class ThingUtility
{
    public static PlanetTile WorldLocation(this Thing thing)
    {
        PlanetTile tile;
        var spawnedThing = thing.SpawnedParentOrMe;

        if (spawnedThing != null && spawnedThing.Spawned)
            tile = thing.Tile;
        else
        {
            Log.Warning($"[PawnHistory] Cannot get location of {thing}");
            tile = PlanetTile.Invalid; // fallback to player home location
        }

        return tile;
    }
}
