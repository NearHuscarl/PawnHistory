using RimWorld;
using RimWorld.Planet;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class ThingUtility
{
    public static Thing GetJumpTarget(this Thing thing)
    {
        if (thing == null) return null;

        var current = thing;

        // 1. If it's a dead pawn, try to target the corpse
        if (thing is Pawn pawn && pawn.Dead && pawn.Corpse != null)
            current = pawn.Corpse;

        // 2. If it's directly on the ground, return it
        if (current.Spawned) return current;

        // 3. Optional: Handle things inside containers (e.g. inside a shelf or a colonist's pocket)
        // If it's not spawned, it might be in an inventory. We jump to the holder instead.
        if (current.ParentHolder is Thing holderThing)
            return holderThing.GetJumpTarget();

        return null;
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
