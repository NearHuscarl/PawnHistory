using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class ThingUtility
{
    public static ThingWithComps SpawnedThing(this Thing thing)
    {
        var pawn = thing as Pawn;
        if (pawn != null && !pawn.Spawned)
            return pawn.Corpse;
        return pawn;
    }

    public static Vector2 LocationOnMap(this Thing thing)
    {
        PlanetTile tile;
        if (thing.Spawned)
            tile = thing.Tile;
        else if (thing.ParentHolder is Corpse corpse) // https://rentry.co/ndkxyoxe
            tile = corpse.Tile;
        else
        {
            Log.Warning($"[PawnHistory] Cannot get location of {thing}");
            tile = PlanetTile.Invalid; // fallback to player home location
        }

        return Find.WorldGrid.LongLatOf(tile);
    }
}
