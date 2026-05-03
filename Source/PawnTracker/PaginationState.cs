using Verse;

namespace PawnHistory.Source.PawnTracker;

public sealed class PaginationState
{
    public string PageText = "1";
    public int CurrentPage = 1;
    public int TotalPages = 1;
    public int? ParsedPage = 1;
    public string Error;
    public Pawn LastPawnShown;
    public bool PendingScrollToBottom;
}

