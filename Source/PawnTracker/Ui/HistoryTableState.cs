using System.Collections.Generic;
using PawnHistory.Source.DebugTools;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public sealed class HistoryTableState
{
    public Pawn LastPawnShown;
    
    [DebugIgnore]
    public bool PendingScrollToBottom;
    
    [DebugIgnore]
    public readonly Dictionary<HistoryRecord, float> CachedHeights = [];
    public HistoryRecord EditingRecord;
    public string EditingText = string.Empty;
    
    public bool HasActiveEditSession => EditingRecord != null;

    public bool IsEditing(HistoryRecord record) => EditingRecord == record;

    public void BeginEditing(HistoryRecord record)
    {
        EditingRecord = record;
        EditingText = record.description;
    }

    public void ClearEditingSession()
    {
        EditingRecord = null;
        EditingText = string.Empty;
    }
}
