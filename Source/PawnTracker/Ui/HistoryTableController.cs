using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public sealed class HistoryTableController
{
    public void SyncExternalState(Pawn pawn, HistoryTableState tableState, PaginationState paginationState, List<Command> commands)
    {
        if (tableState.LastPawnShown == pawn)
            return;

        tableState.LastPawnShown = pawn;
        tableState.ClearEditingSession();
        tableState.CachedHeights.Clear();
        tableState.PendingScrollToBottom = true;
        RefreshPageCount(paginationState, tableState);
        GoToPage(paginationState, paginationState.TotalPages);
        commands.Clear();
    }

    public void Handle(HistoryTableState tableState, PaginationState paginationState, List<Command> commands)
    {
        HandleTableCommands(tableState, paginationState, commands);
        HandlePaginationCommands(tableState, paginationState, commands);
    }

    private void HandlePaginationCommands(HistoryTableState tableState, PaginationState paginationState, List<Command> commands)
    {
        foreach (var command in commands)
        {
            switch (command)
            {
                case FirstPageClicked:
                    GoToPage(paginationState, 1);
                    break;
                case PreviousPageClicked:
                    GoToPage(paginationState, paginationState.CurrentPage - 1);
                    break;
                case NextPageClicked:
                    GoToPage(paginationState, paginationState.CurrentPage + 1);
                    break;
                case LastPageClicked:
                    GoToPage(paginationState, paginationState.TotalPages);
                    break;
                case PageInputSubmitted:
                    SubmitPageInput(tableState, paginationState);
                    break;
            }
        }
        commands.RemoveAll(c => c is PaginationCommand);
    }

    private void HandleTableCommands(HistoryTableState tableState, PaginationState paginationState, List<Command> commands)
    {
        foreach (var command in commands)
        {
            switch (command)
            {
                case BeginEditRequested beginEdit:
                    tableState.BeginEditing(beginEdit.Record);
                    break;
                case DeleteRecordRequested deleteRecord:
                    HandleDeleteRecordRequested(tableState, paginationState, deleteRecord.Record);
                    break;
                case SaveEditedRecord:
                    SaveEditedDescription(tableState);
                    break;
                case CancelEditedRecord:
                    tableState.ClearEditingSession();
                    break;
            }
        }
        commands.RemoveAll(c => c is HistoryTableCommand);
    }

    private static void HandleDeleteRecordRequested(HistoryTableState tableState, PaginationState paginationState, HistoryRecord record)
    {
        var comp = CompHistoryManager.GetComp(record.pawn);
        if (comp.RemoveRecord(record))
            return;

        tableState.ClearEditingSession();
        RefreshPageCount(paginationState, tableState);
        GoToPage(paginationState, paginationState.CurrentPage);
    }

    private static void SaveEditedDescription(HistoryTableState tableState)
    {
        var trimmed = tableState.EditingText.Trim();
        if (trimmed.Length == 0)
        {
            Messages.Message("NH_PH_HistoryCard_EditRejectedEmpty".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        tableState.EditingRecord.description = trimmed;
        tableState.CachedHeights.Remove(tableState.EditingRecord);
        tableState.ClearEditingSession();
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
    }

    private static void RefreshPageCount(PaginationState paginationState, HistoryTableState tableState)
    {
        paginationState.TotalPages = TotalPagesFor(GetVisibleRecords(tableState.LastPawnShown).Count());
    }
    
    private static void GoToPage(PaginationState paginationState, int page)
    {
        page = Mathf.Clamp(page, 1, Mathf.Max(1, paginationState.TotalPages));
        
        paginationState.CurrentPage = page;
        paginationState.PageText = page.ToString();
        paginationState.Error = null;
    }

    private static int TotalPagesFor(int recordCount) => Mathf.Max(1, Mathf.CeilToInt(recordCount / (float)PaginationView.PageSize));

    public static IEnumerable<HistoryRecord> GetVisibleRecords(Pawn pawn)
    {
        return pawn.HistoryRecords.Where(record => record.def.importance != RecordImportance.Debug).ToList();
    }
}
