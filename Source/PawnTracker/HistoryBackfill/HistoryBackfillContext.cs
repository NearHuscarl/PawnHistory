using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.HistoryBackfill;

internal sealed class HistoryBackfillContext(Pawn pawn, HistoryRecord anchorRecord, IReadOnlyList<HistoryRecord> allRecords)
{
    public Pawn Pawn { get; } = pawn ?? throw new ArgumentNullException(nameof(pawn));
    public HistoryRecord AnchorRecord { get; } = anchorRecord ?? throw new ArgumentNullException(nameof(anchorRecord));
    public IReadOnlyList<HistoryRecord> AllRecords { get; } = allRecords ?? throw new ArgumentNullException(nameof(allRecords));
    public int AnchorTick => AnchorRecord.date;
    public long BirthAbsTicks => Pawn.ageTracker?.BirthAbsTicks ?? AnchorTick;
    public int BirthTick => ClampToInt(BirthAbsTicks);

    public float BiologicalAgeAt(int tick) => (float)(tick - BirthAbsTicks) / GenDate.TicksPerYear;

    public static int ClampToInt(long ticks)
    {
        if (ticks > int.MaxValue)
            return int.MaxValue;
        if (ticks < int.MinValue)
            return int.MinValue;
        return (int)ticks;
    }
}
