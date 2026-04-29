namespace PawnHistory.Source.PawnTracker;

internal struct TimelineWindow(int earliestTick, int latestTick)
{
    public int EarliestTick = earliestTick;
    public int LatestTick = latestTick;
    public bool IsValid => EarliestTick <= LatestTick;

    public void ClampEarliest(int tick)
    {
        if (tick > EarliestTick)
            EarliestTick = tick;
    }

    public void ClampLatest(int tick)
    {
        if (tick < LatestTick)
            LatestTick = tick;
    }
}
