using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Ui;
using UnityEngine;
using Verse;
using static PawnHistory.Source.Ui.W;
using UiSizedBox = PawnHistory.Source.Ui.SizedBox;

namespace PawnHistory.Source.PawnTracker.Ui;

internal static class HistoryCardDebugOverlay
{
    public const float MaxWidth = 420f;
    private const float PaddingValue = 6f;
    private static readonly Color Fill = new(0f, 0f, 0f, 0.72f);

    public static Widget Build(HistoryCardState state)
    {
        if (!NearDebugSettings.DrawHistoryCardState)
            return UiSizedBox.Shrink();

        var text = $"{DebugUtility.Format(state.Table)}\n{DebugUtility.Format(state.Pagination)}";

        return SizedBox(
            width: MaxWidth,
            child: DecoratedBox(
                new BoxDecoration(Color: Fill, Border: true),
                Padding.All(
                    Label(text, GameFont.Tiny, TextAnchor.UpperLeft, color: Color.white),
                    PaddingValue)));
    }
}
