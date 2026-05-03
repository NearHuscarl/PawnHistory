using System.Collections.Generic;
using PawnHistory.Source.DebugTools;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public sealed class HistoryCardPage
{
    private static float containerPadding;
    /// <summary>
    /// default gap between common UI controls
    /// </summary>
    private static float gap;
    private static float filterHeight;

    private readonly HistoryTableState tableState = new();
    private readonly PaginationState paginationState = new();
    private readonly HistoryTableController tableController = new();
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
        var layout = new HistoryTableLayout(gap);

        GUI.BeginGroup(inRect);
        try
        {
            var filterRect = new Rect(0f, 0f, inRect.width, filterHeight);
            var groupRect = new Rect(0f, filterHeight, inRect.width, inRect.height - filterHeight);
            var commands = new List<PaginationCommand>();
            
            tableController.SyncExternalState(pawn, tableState, paginationState);
            PaginationView.Draw(filterRect, paginationState, commands);
            HistoryTableView.Draw(groupRect, tableState, paginationState, ref scrollPosition, layout);
            tableController.HandleCommands(pawn, tableState, paginationState, commands);
        }
        finally
        {
            GUI.EndGroup();
            GUI.color = color;
            Text.Font = font;
            Text.Anchor = anchor;
        }
    }
}
