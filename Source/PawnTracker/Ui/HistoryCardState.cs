using PawnHistory.Source.Ui;

namespace PawnHistory.Source.PawnTracker.Ui;

internal sealed class HistoryCardState
{
    public HistoryTableState Table { get; } = new();
    public PaginationState Pagination { get; } = new();
    public ScrollController TableScroll { get; } = new();
}
