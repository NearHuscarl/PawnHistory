using System;
using PawnHistory.Source.Ui;
using static PawnHistory.Source.Ui.W;

namespace PawnHistory.Source.PawnTracker.Ui;

internal static class HistoryPagination
{
    private const string FirstPageIcon = "◀";
    private const string PreviousPageIcon = "<";
    private const string NextPageIcon = ">";
    private const string LastPageIcon = "▶";

    public static Widget Build(
        UiContext ctx,
        PaginationState state,
        bool enabled,
        bool canGoToPreviousPage,
        bool canGoToNextPage,
        Action goToFirstPage,
        Action goToPreviousPage,
        Action<string> updatePageText,
        Action submitPageInput,
        Action goToNextPage,
        Action goToLastPage)
    {
        var height = HistoryCardLayout.ControlWidth;

        return Padding.Right(
            Row(
            [
                Button(FirstPageIcon, goToFirstPage, width: HistoryCardLayout.ControlWidth, height: height, enabled: enabled && canGoToPreviousPage),
                Button(PreviousPageIcon, goToPreviousPage, width: HistoryCardLayout.ControlWidth, height: height, enabled: enabled && canGoToPreviousPage),
                TextField(state.PageText, updatePageText, submitPageInput, width: HistoryCardLayout.PageInputWidth, height: height, enabled: enabled, key: "history-page-field"),
                Button(NextPageIcon, goToNextPage, width: HistoryCardLayout.ControlWidth, height: height, enabled: enabled && canGoToNextPage),
                Button(LastPageIcon, goToLastPage, width: HistoryCardLayout.ControlWidth, height: height, enabled: enabled && canGoToNextPage),
            ], crossAxis: StackCrossAxis.Center, gap: ctx.Theme.GapXs),
            ctx.Theme.ButtonHorizontalPadding);
    }
}
