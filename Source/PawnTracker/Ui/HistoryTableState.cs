using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public sealed class HistoryTableState
{
    public Pawn LastPawnShown;
    public bool PendingScrollToBottom;
    public readonly Dictionary<HistoryRecord, float> CachedHeights = [];
}
