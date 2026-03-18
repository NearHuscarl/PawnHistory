using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class CellRectExtensions
{
    public static IntVec3 OutsideOf(this CellRect rect)
    {
        var side = Rand.RangeInclusive(0, 3);

        return side switch
        {
            // Left
            0 => new IntVec3(rect.minX - 1, 0, Rand.RangeInclusive(rect.minZ, rect.maxZ)),
            // Right
            1 => new IntVec3(rect.maxX + 1, 0, Rand.RangeInclusive(rect.minZ, rect.maxZ)),
            // Bottom
            2 => new IntVec3(Rand.RangeInclusive(rect.minX, rect.maxX), 0, rect.minZ - 1),
            // Top
            _ => new IntVec3(Rand.RangeInclusive(rect.minX, rect.maxX), 0, rect.maxZ + 1),
        };
    }
}
