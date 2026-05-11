using PawnHistory.Source.DebugTools;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

internal sealed class HistoryTableState
{
    public Pawn LastPawnShown;
    public int KnownRecordCount;
    public HistoryRecord EditingRecord;
    public string EditingText = string.Empty;

    [DebugIgnore]
    public bool PendingEditFocus;

    public bool HasActiveEditSession => EditingRecord != null;

    public bool IsEditing(HistoryRecord record) => EditingRecord == record;

    public void BeginEditing(HistoryRecord record)
    {
        EditingRecord = record;
        EditingText = record.description;
        PendingEditFocus = true;
    }

    public void ClearEditingSession()
    {
        EditingRecord = null;
        EditingText = string.Empty;
        PendingEditFocus = false;
    }
}
