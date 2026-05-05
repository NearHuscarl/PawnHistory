using PawnHistory.Source.DebugTools;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public static class HistoryTableDebugView
{
    private const float OverlayPadding = 6f;
    private const float OverlayMaxWidth = 420f;
    private static readonly Color OverlayFill = new(0f, 0f, 0f, 0.72f);

    public static void Draw(Rect inRect, HistoryTableState tableState, PaginationState paginationState)
    {
        if (!NearDebugSettings.DrawHistoryCardState)
            return;

        var oldFont = Text.Font;
        var oldAnchor = Text.Anchor;
        var oldColor = GUI.color;

        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;

        var debugText = $"{DebugUtility.Format(tableState)}\n{DebugUtility.Format(paginationState)}";
        var maxWidth = Mathf.Min(OverlayMaxWidth, inRect.width);
        var textHeight = Text.CalcHeight(debugText, maxWidth - OverlayPadding * 2f);
        var overlayHeight = textHeight + OverlayPadding * 2f;
        var overlayRect = new Rect(
            inRect.xMax - maxWidth,
            inRect.yMax - overlayHeight,
            maxWidth,
            overlayHeight);
        var textRect = overlayRect.ContractedBy(OverlayPadding);

        Widgets.DrawBoxSolid(overlayRect, OverlayFill);
        Widgets.DrawBox(overlayRect);
        Widgets.Label(textRect, debugText);

        GUI.color = oldColor;
        Text.Font = oldFont;
        Text.Anchor = oldAnchor;
    }
}
