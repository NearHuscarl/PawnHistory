using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Ui;
using UnityEngine;
using Verse;
using static PawnHistory.Source.Ui.W;

namespace PawnHistory.Source.PawnTracker.Ui;

internal static class HistoryCardDebugOverlay
{
    private const float MaxWidth = 420f;
    private static readonly Color Fill = new(0f, 0f, 0f, 0.72f);

    public static Widget Build(UiContext ctx, HistoryCardState state)
    {
        if (!NearDebugSettings.DrawHistoryCardState)
            return SizedBoxShrink();

        var theme = ctx.Theme;
        var text = $"{DebugUtility.Format(state.Table)}\n{DebugUtility.Format(state.Pagination)}";

        return SizedBox(
            width: MaxWidth,
            child: DecoratedBox(
                new BoxDecoration(Color: Fill, Border: true),
                Padding.All(
                    Label(text, GameFont.Tiny, TextAnchor.UpperLeft, color: Color.white),
                    theme.PaddingSm)));
    }
}
