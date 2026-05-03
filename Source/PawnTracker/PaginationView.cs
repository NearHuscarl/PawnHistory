using PawnHistory.Source.DebugTools;
using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class PaginationView
{
    private const string ControlName = "HistoryPaginationPageField";
    private const string FirstPageIcon = "◀";
    private const string PreviousPageIcon = "<";
    private const string NextPageIcon = ">";
    private const string LastPageIcon = "▶";

    private static float containerPadding;
    private static float controlGap;
    private static float buttonWidth;
    private static float inputWidth;
    private static float controlHeightMargin;

    static PaginationView() => ReloadPaginationView();

    [Reloadable]
    [NearDebugAction]
    private static void ReloadPaginationView()
    {
        containerPadding = 16f;
        controlGap = 4f;
        buttonWidth = 24f;
        inputWidth = 42f;
        controlHeightMargin = 3f;
    }

    public static void Draw(Rect filterRect, PaginationState state, Action<PaginationCommand> emit)
    {
        state.PageText ??= state.CurrentPage.ToString();

        var current = Event.current;
        var shouldSubmit = GUI.GetNameOfFocusedControl() == ControlName
            && current.type == EventType.KeyDown
            && current.keyCode is KeyCode.Return or KeyCode.KeypadEnter;
        if (shouldSubmit)
            current.Use();

        var controlHeight = filterRect.height - controlHeightMargin * 2f;
        var controlY = filterRect.y + controlHeightMargin;
        var controlsWidth = buttonWidth * 4f + inputWidth + controlGap * 4f;
        var startX = filterRect.xMax - containerPadding - controlsWidth;

        var firstButtonRect = new Rect(startX, controlY, buttonWidth, controlHeight);
        var previousButtonRect = new Rect(firstButtonRect.xMax + controlGap, controlY, buttonWidth, controlHeight);
        var inputRect = new Rect(previousButtonRect.xMax + controlGap, controlY, inputWidth, controlHeight);
        var nextButtonRect = new Rect(inputRect.xMax + controlGap, controlY, buttonWidth, controlHeight);
        var lastButtonRect = new Rect(nextButtonRect.xMax + controlGap, controlY, buttonWidth, controlHeight);

        var wasEnabled = GUI.enabled;

        GUI.enabled = wasEnabled && CanGoToFirstPage(state);
        if (Widgets.ButtonText(firstButtonRect, FirstPageIcon))
            emit(new FirstPageClicked());

        GUI.enabled = wasEnabled && CanGoToPreviousPage(state);
        if (Widgets.ButtonText(previousButtonRect, PreviousPageIcon))
            emit(new PreviousPageClicked());

        GUI.enabled = wasEnabled;
        GUI.SetNextControlName(ControlName);
        var edited = Widgets.TextField(inputRect, state.PageText);
        if (edited != state.PageText && InputValidators.DigitsOnly(edited))
        {
            state.PageText = edited;
            state.Error = null;
            state.ParsedPage = null;
        }

        if (shouldSubmit)
            emit(new PageInputSubmitted());

        GUI.enabled = wasEnabled && CanGoToNextPage(state);
        if (Widgets.ButtonText(nextButtonRect, NextPageIcon))
            emit(new NextPageClicked());

        GUI.enabled = wasEnabled && CanGoToLastPage(state);
        if (Widgets.ButtonText(lastButtonRect, LastPageIcon))
            emit(new LastPageClicked());

        GUI.enabled = wasEnabled;
    }

    private static bool CanGoToFirstPage(PaginationState state) => state.TotalPages > 0 && state.CurrentPage > 1;
    private static bool CanGoToPreviousPage(PaginationState state) => state.TotalPages > 0 && state.CurrentPage > 1;
    private static bool CanGoToNextPage(PaginationState state) => state.TotalPages > 0 && state.CurrentPage < state.TotalPages;
    private static bool CanGoToLastPage(PaginationState state) => state.TotalPages > 0 && state.CurrentPage < state.TotalPages;
}
