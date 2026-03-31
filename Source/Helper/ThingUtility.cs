using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class ThingUtility
{
    public static Thing GetSpawnedHolderOrSelf(this IThingHolder thingHolder)
    {
        if (thingHolder == null) return null;

        var current = thingHolder;

        // 1. If it's a dead pawn, try to target the corpse
        if (thingHolder is Pawn pawn && pawn.Dead && pawn.Corpse != null)
            current = pawn.Corpse;

        // 2. If it's directly on the ground, return it
        if (current is Thing thing && thing.Spawned)
            return thing;

        // 3. Optional: Handle things inside containers (e.g. inside a shelf or a colonist's pocket)
        // If it's not spawned, it might be in an inventory. We jump to the holder instead.
        if (current.ParentHolder != null)
            return current.ParentHolder.GetSpawnedHolderOrSelf();

        return null;
    }
    public static Thing GetSpawnedHolderOrSelf(this Thing thing) => GetSpawnedHolderOrSelf(thing as IThingHolder);

    public static PlanetTile WorldLocation(this Thing thing)
    {
        PlanetTile tile;
        var current = thing.GetSpawnedHolderOrSelf();

        if (current != null && current.Spawned)
            tile = thing.Tile;
        else
        {
            Log.Warning($"[PawnHistory] Cannot get location of {thing}");
            tile = PlanetTile.Invalid; // fallback to player home location
        }

        return tile;
    }
}
