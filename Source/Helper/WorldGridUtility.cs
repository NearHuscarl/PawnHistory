using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.Helper;

public static class WorldGridUtility
{
    public static PlanetTile GetNearbyTile(this WorldGrid grid, PlanetTile? tile = null)
    {
        var neighbors = new List<PlanetTile>();
        grid.GetTileNeighbors(tile ?? Find.AnyPlayerHomeMap.Tile, neighbors);
        return neighbors.First();
    }
}