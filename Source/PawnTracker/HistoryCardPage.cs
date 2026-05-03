using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public sealed class HistoryCardPage
{
    private static float containerPadding;
    /// <summary>
    /// default gap between common UI controls
    /// </summary>
    private static float gap;
    private static float filterHeight;

    private readonly Dictionary<HistoryRecord, float> cachedHeights = [];
    private readonly PaginationState paginationState = new();
    private readonly PaginationController paginationController = new();
    private Vector2 scrollPosition;

    static HistoryCardPage() => ReloadHistoryCardPageLayout();

    [Reloadable]
    [NearDebugAction]
    private static void ReloadHistoryCardPageLayout()
    {
        containerPadding = 8f;
        gap = 10f;
        filterHeight = 30f;
    }

    public void Draw(Rect tabRect, Pawn pawn)
    {
        var color = GUI.color;
        var font = Text.Font;
        var anchor = Text.Anchor;

        var inRect = tabRect.ContractedBy(containerPadding);
        var records = pawn.HistoryRecords.Where(r => r.def.importance != RecordImportance.Debug).ToList();
        ReconcilePaginationState(pawn, records.Count);
        var pageRecords = SliceRecords(records);
        var layout = new HistoryCardLayout(gap);

        GUI.BeginGroup(inRect);
        try
        {
            var filterRect = new Rect(0f, filterHeight, inRect.width, filterHeight);
            var groupRect = new Rect(0f, 0f, inRect.width, inRect.height);
            PaginationView.Draw(filterRect, paginationState, command => paginationController.Handle(command, paginationState));
            HistoryCardView.Draw(groupRect, pageRecords, ref scrollPosition, ref paginationState.PendingScrollToBottom, cachedHeights, layout);
        }
        finally
        {
            GUI.EndGroup();
            GUI.color = color;
            Text.Font = font;
            Text.Anchor = anchor;
        }
    }

    private void ReconcilePaginationState(Pawn pawn, int recordCount)
    {
        var totalPages = TotalPagesFor(recordCount);
        paginationState.TotalPages = totalPages;

        if (paginationState.LastPawnShown != pawn)
        {
            paginationState.LastPawnShown = pawn;
            CommitPage(totalPages, scrollToBottom: true);
            return;
        }

        if (paginationState.CurrentPage < 1 || paginationState.CurrentPage > totalPages)
        {
            CommitPage(Mathf.Clamp(paginationState.CurrentPage, 1, totalPages));
            return;
        }

        paginationState.PageText ??= paginationState.CurrentPage.ToString();
    }

    private void CommitPage(int page, bool scrollToBottom = false)
    {
        paginationState.CurrentPage = page;
        paginationState.ParsedPage = page;
        paginationState.PageText = page.ToString();
        paginationState.Error = null;
        paginationState.PendingScrollToBottom = scrollToBottom;
    }

    private static int TotalPagesFor(int recordCount) => Mathf.Max(1, Mathf.CeilToInt(recordCount / (float)PaginationController.PageSize));

    private List<HistoryRecord> SliceRecords(List<HistoryRecord> records)
    {
        var startIndex = (paginationState.CurrentPage - 1) * PaginationController.PageSize;
        return records.Skip(startIndex).Take(PaginationController.PageSize).ToList();
    }
}
