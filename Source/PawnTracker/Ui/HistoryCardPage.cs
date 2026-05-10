using System.Collections.Generic;
using PawnHistory.Source.DebugTools;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public record HistoryCardPageContext(Rect PageRect);

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
    private readonly List<Command> commands = [];
    private readonly HistoryTableController tableController = new();
    private Vector2 scrollPosition;
    public static HistoryCardPageContext Context;

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
        Context = new HistoryCardPageContext(tabRect);
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
            
            tableController.SyncExternalState(pawn, tableState, paginationState, commands);
            HistoryAddRecordButtonView.Draw(filterRect, pawn, tableState, commands);
            PaginationView.Draw(filterRect, paginationState, tableState, commands);
            HistoryTableView.Draw(groupRect, tableState, paginationState, ref scrollPosition, layout, commands);
            HistoryTableDebugView.Draw(inRect, tableState, paginationState);
            tableController.Handle(tableState, paginationState, commands);
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
