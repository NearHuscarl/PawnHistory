using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.Helper;

public static class WorldGridUtility
{
    public static PlanetTile GetNearbyTile(this WorldGrid grid, PlanetTile tile)
    {
        var neighbors = new List<PlanetTile>();
        grid.GetTileNeighbors(tile, neighbors);
        return neighbors.First();
    }
}