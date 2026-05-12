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
        var buttonSize = HistoryCardLayout.ControlWidth;

        return Row([
            TextButton(FirstPageIcon, goToFirstPage, buttonSize, buttonSize, enabled: enabled && canGoToPreviousPage),
            TextButton(PreviousPageIcon, goToPreviousPage, buttonSize, buttonSize, enabled: enabled && canGoToPreviousPage),
            TextField(state.PageText, updatePageText, submitPageInput, width: HistoryCardLayout.PageInputWidth, height: buttonSize, enabled: enabled, key: "history-page-field"),
            TextButton(NextPageIcon, goToNextPage, buttonSize, buttonSize, enabled: enabled && canGoToNextPage),
            TextButton(LastPageIcon, goToLastPage, buttonSize, buttonSize, enabled: enabled && canGoToNextPage),
        ], crossAxis: StackCrossAxis.Center, spacing: ctx.Theme.GapSm);
    }
}
