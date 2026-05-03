using PawnHistory.Source.Helper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public sealed class HistoryTableController
{
    public void SyncExternalState(Pawn pawn, HistoryTableState tableState, PaginationState paginationState)
    {
        var visibleRecords = GetVisibleRecords(pawn);
        paginationState.TotalPages = TotalPagesFor(visibleRecords.Count);

        if (tableState.LastPawnShown != pawn)
        {
            tableState.LastPawnShown = pawn;
            GoToPage(paginationState, paginationState.TotalPages);
            tableState.PendingScrollToBottom = true;
            tableState.CachedHeights.Clear();
        }
        else if (paginationState.CurrentPage < 1 || paginationState.CurrentPage > paginationState.TotalPages)
            GoToPage(paginationState, paginationState.CurrentPage); // clamp on record deletion
    }

    public void HandleCommands(Pawn pawn, HistoryTableState tableState, PaginationState paginationState, List<PaginationCommand> commands)
    {
        if (commands.Count == 0)
            return;

        foreach (var command in commands)
        {
            switch (command)
            {
                case FirstPageClicked:
                    GoToPage(paginationState, 1);
                    tableState.PendingScrollToBottom = false;
                    break;
                case PreviousPageClicked:
                    GoToPage(paginationState, paginationState.CurrentPage - 1);
                    tableState.PendingScrollToBottom = false;
                    break;
                case NextPageClicked:
                    GoToPage(paginationState, paginationState.CurrentPage + 1);
                    tableState.PendingScrollToBottom = false;
                    break;
                case LastPageClicked:
                    GoToPage(paginationState, paginationState.TotalPages);
                    tableState.PendingScrollToBottom = false;
                    break;
                case PageInputSubmitted:
                    SubmitPageInput(tableState, paginationState);
                    break;
            }
        }
    }

    public static List<HistoryRecord> GetVisibleRecords(Pawn pawn)
    {
        return pawn.HistoryRecords.Where(record => record.def.importance != RecordImportance.Debug).ToList();
    }

    private static void SubmitPageInput(HistoryTableState tableState, PaginationState paginationState)
    {
        if (!InputValidators.TryPositiveInt(paginationState.PageText, out var page, out var error))
        {
            paginationState.Error = error;
            paginationState.PageText = paginationState.CurrentPage.ToString();
            return;
        }

        if (page > paginationState.TotalPages)
        {
            paginationState.Error = $"Enter a page from 1 to {paginationState.TotalPages}.";
            paginationState.PageText = paginationState.CurrentPage.ToString();
            return;
        }

        GoToPage(paginationState, page);
        tableState.PendingScrollToBottom = false;
    }
    
    private static void GoToPage(PaginationState paginationState, int page)
    {
        page = Mathf.Clamp(page, 1, Mathf.Max(1, paginationState.TotalPages));
        
        paginationState.CurrentPage = page;
        paginationState.PageText = page.ToString();
        paginationState.Error = null;
    }

    private static int TotalPagesFor(int recordCount) => Mathf.Max(1, Mathf.CeilToInt(recordCount / (float)PaginationView.PageSize));
}
