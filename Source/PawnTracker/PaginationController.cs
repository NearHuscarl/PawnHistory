using UnityEngine;

namespace PawnHistory.Source.PawnTracker;

public sealed class PaginationController
{
    public const int PageSize = 12;

    public void Handle(PaginationCommand command, PaginationState state)
    {
        switch (command)
        {
            case FirstPageClicked:
                GoToPage(state, 1);
                break;
            case PreviousPageClicked:
                GoToPage(state, state.CurrentPage - 1);
                break;
            case NextPageClicked:
                GoToPage(state, state.CurrentPage + 1);
                break;
            case LastPageClicked:
                GoToPage(state, state.TotalPages);
                break;
            case PageInputSubmitted:
                SubmitPageInput(state);
                break;
        }
    }

    private static void SubmitPageInput(PaginationState state)
    {
        if (!InputValidators.TryPositiveInt(state.PageText, out var page, out var error))
        {
            ResetSubmittedInput(state, error);
            return;
        }

        if (page > state.TotalPages)
        {
            ResetSubmittedInput(state, $"Enter a page from 1 to {state.TotalPages}.");
            return;
        }

        GoToPage(state, page);
    }

    private static void ResetSubmittedInput(PaginationState state, string error)
    {
        state.Error = error;
        state.PageText = state.CurrentPage.ToString();
        state.ParsedPage = state.CurrentPage;
    }

    private static void GoToPage(PaginationState state, int page)
    {
        var clampedPage = Mathf.Clamp(page, 1, Mathf.Max(1, state.TotalPages));
        state.CurrentPage = clampedPage;
        state.ParsedPage = clampedPage;
        state.PageText = clampedPage.ToString();
        state.Error = null;
        state.PendingScrollToBottom = false;
    }
}
