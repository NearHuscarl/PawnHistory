using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.Helper;

public static class WorldObjectUtility
{
    extension(WorldObject worldObject)
    {
        public string ColoredLabel => worldObject.Label.ApplyTag(TagType.Settlement, worldObject.Faction?.GetUniqueLoadID()).Resolve();
    }
}