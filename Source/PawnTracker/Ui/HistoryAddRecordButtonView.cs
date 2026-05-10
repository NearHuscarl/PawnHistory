using System.Collections.Generic;
using PawnHistory.Source.DebugTools;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public static class HistoryAddRecordButtonView
{
    private static float containerPadding;
    private static float buttonWidth;
    private static float controlHeightMargin;

    static HistoryAddRecordButtonView() => ReloadHistoryAddRecordButtonView();

    [Reloadable]
    [NearDebugAction]
    private static void ReloadHistoryAddRecordButtonView()
    {
        containerPadding = 16f;
        buttonWidth = 24f;
        controlHeightMargin = 3f;
    }

    public static void Draw(Rect filterRect, Pawn pawn, HistoryTableState tableState, List<Command> commands)
    {
        if (pawn == null || Find.CurrentMap == null)
            return;

        var controlHeight = filterRect.height - controlHeightMargin * 2f;
        var buttonRect = new Rect(filterRect.x + containerPadding, filterRect.y + controlHeightMargin, buttonWidth, controlHeight);
        TooltipHandler.TipRegion(buttonRect, "NH_PH_AddRecord_Title".Translate());

        var wasEnabled = GUI.enabled;
        GUI.enabled = wasEnabled && !tableState.HasActiveEditSession;
        if (Widgets.ButtonImage(buttonRect, TexButton.Plus))
            Find.WindowStack.Add(new AddRecordDialog(pawn, () => commands.Add(new LatestPageRefreshed())));
        GUI.enabled = wasEnabled;
    }
}
