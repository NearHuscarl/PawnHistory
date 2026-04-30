using System;

namespace PawnHistory.Source.PawnTracker.HistoryBackfill;

internal struct TimelineWindow(int earliestTick, int latestTick)
{
    public int EarliestTick = earliestTick;
    public int LatestTick = latestTick;
    public bool IsValid => EarliestTick <= LatestTick;

    public void ShrinkStartTo(int tick)
    {
        if (tick > EarliestTick)
            EarliestTick = tick;
    }

    public void ShrinkEndTo(int tick)
    {
        if (tick < LatestTick)
            LatestTick = tick;
    }
    
    public TimelineWindow ShrinkTo(int earliestTick, int latestTick) => new(Math.Max(EarliestTick, earliestTick), Math.Min(LatestTick, latestTick));
}
