using System;
using System.Collections.Generic;
using PawnHistory.Source.Ui;
using PawnHistory.Source.Helper;
using RimWorld;
using UnityEngine;
using Verse;
using static PawnHistory.Source.Ui.W;

namespace PawnHistory.Source.PawnTracker.Ui;

internal sealed class HistoryCardPage
{
    private readonly HistoryCardState state = new();
    private readonly HistoryRecordActions recordActions;
    private readonly Action goToFirstPageAction;
    private readonly Action goToPreviousPageAction;
    private readonly Action<string> updatePageTextAction;
    private readonly Action submitPageInputAction;
    private readonly Action goToNextPageAction;
    private readonly Action goToLastPageAction;
    private Pawn pawn;

    public HistoryCardPage()
    {
        recordActions = new(
            JumpToRecord,
            OpenRecordMenu,
            HighlightTargets,
            OpenQuest,
            UpdateEditingText,
            SaveEditedDescription,
            ClearEditingSession);
        goToFirstPageAction = GoToFirstPage;
        goToPreviousPageAction = GoToPreviousPage;
        updatePageTextAction = UpdatePageText;
        submitPageInputAction = SubmitPageInput;
        goToNextPageAction = GoToNextPage;
        goToLastPageAction = GoToLastPage;
    }

    public Widget Build(UiContext ctx, Pawn shownPawn)
    {
        this.pawn = shownPawn;
        SyncPawn(pawn);

        return Padding.All(
            Stack([
                Column(
                [
                    SizedBox(height: HistoryCardLayout.TopBarHeight, child: BuildTopBar(ctx)),
                    Expanded(HistoryTable.Build(ctx, state, recordActions)),
                ]),
                Align(HistoryCardDebugOverlay.Build(ctx, state), Alignment.BottomRight),
            ]),
            ctx.Theme.Padding);
    }

    private Widget BuildTopBar(UiContext ctx)
    {
        var theme = ctx.Theme;
        return Row(
        [
            SizedBox(width: theme.PaddingXs),
            BuildAddRecordButton(),
            Spacer(),
            HistoryPagination.Build(
                ctx,
                state.Pagination,
                !state.Table.HasActiveEditSession,
                CanGoToPreviousPage,
                CanGoToNextPage,
                goToFirstPageAction,
                goToPreviousPageAction,
                updatePageTextAction,
                submitPageInputAction,
                goToNextPageAction,
                goToLastPageAction),
            SizedBox(width: theme.ScrollWidth),
        ], crossAxis: StackCrossAxis.Center);
    }

    private Widget BuildAddRecordButton()
    {
        if (pawn == null || Find.CurrentMap == null)
            return SizedBoxShrink();

        return IconButton(
            TexButton.Plus,
            OpenAddRecordDialog,
            tooltip: "NH_PH_AddRecord_Title".Translate(),
            enabled: !state.Table.HasActiveEditSession);
    }

    private void SyncPawn(Pawn nextPawn)
    {
        var recordCount = nextPawn?.HistoryRecords.Count ?? 0;
        var pawnChanged = state.Table.LastPawnShown != nextPawn;
        var recordCountChanged = state.Table.KnownRecordCount != recordCount;

        if (!pawnChanged && !recordCountChanged)
            return;

        var previousCount = state.Table.KnownRecordCount;
        state.Table.LastPawnShown = nextPawn;
        state.Table.KnownRecordCount = recordCount;
        state.Table.ClearEditingSession();

        if (pawnChanged || recordCount > previousCount)
            RefreshLatestPage();
        else
            RefreshCurrentPage();
    }

    private void OpenAddRecordDialog()
    {
        if (pawn != null)
            Find.WindowStack.Add(new AddRecordDialog(pawn, RefreshLatestPage));
    }

    private void RefreshLatestPage()
    {
        RefreshPageCount();
        GoToPage(state.Pagination.TotalPages);
        state.Table.KnownRecordCount = state.Table.LastPawnShown?.HistoryRecords.Count ?? 0;
        state.TableScroll.ScrollToBottom();
    }

    private void RefreshCurrentPage()
    {
        RefreshPageCount();
        GoToPage(state.Pagination.CurrentPage);
        state.Table.KnownRecordCount = state.Table.LastPawnShown?.HistoryRecords.Count ?? 0;
    }

    private void RefreshPageCount()
    {
        var recordCount = state.Table.LastPawnShown?.VisibleHistoryRecords.Count ?? 0;
        state.Pagination.TotalPages = Mathf.Max(1, Mathf.CeilToInt(recordCount / (float)HistoryCardLayout.PageSize));
    }

    private void GoToPage(int page)
    {
        var pagination = state.Pagination;
        page = Mathf.Clamp(page, 1, Mathf.Max(1, pagination.TotalPages));

        pagination.CurrentPage = page;
        pagination.PageText = page.ToString();
        pagination.Error = null;
    }

    private void GoToFirstPage() => GoToPage(1);
    private void GoToPreviousPage() => GoToPage(state.Pagination.CurrentPage - 1);
    private void GoToNextPage() => GoToPage(state.Pagination.CurrentPage + 1);
    private void GoToLastPage() => GoToPage(state.Pagination.TotalPages);

    private void UpdatePageText(string value)
    {
        if (!InputValidators.DigitsOnly(value))
            return;

        state.Pagination.PageText = value;
        state.Pagination.Error = null;
    }

    private void SubmitPageInput()
    {
        var pagination = state.Pagination;
        if (!InputValidators.TryPositiveInt(pagination.PageText, out var page, out var error))
        {
            pagination.Error = error;
            pagination.PageText = pagination.CurrentPage.ToString();
            return;
        }

        if (page > pagination.TotalPages)
        {
            pagination.Error = $"Enter a page from 1 to {pagination.TotalPages}.";
            pagination.PageText = pagination.CurrentPage.ToString();
            return;
        }

        GoToPage(page);
    }

    public void BeginEditing(HistoryRecord record)
    {
        state.Table.BeginEditing(record);
    }

    public void DeleteRecord(HistoryRecord record)
    {
        var comp = CompHistoryManager.GetComp(record.pawn);
        if (comp == null || !comp.RemoveRecord(record))
            return;

        state.Table.ClearEditingSession();
        RefreshCurrentPage();
    }

    public void SaveEditedDescription()
    {
        var trimmed = state.Table.EditingText.Trim();
        if (trimmed.Length == 0)
        {
            Messages.Message("NH_PH_HistoryCard_EditRejectedEmpty".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        state.Table.EditingRecord.description = trimmed;
        state.Table.ClearEditingSession();
    }

    public void ClearEditingSession()
    {
        state.Table.ClearEditingSession();
    }

    public void UpdateEditingText(string text)
    {
        state.Table.EditingText = text;
    }

    public void CopyDescriptionToClipboard(HistoryRecord record)
    {
        GUIUtility.systemCopyBuffer = LangUtility.StripColorTags(record.description);
        Messages.Message("NH_PH_HistoryCard_RecordCopied".Translate(), MessageTypeDefOf.NeutralEvent);
    }

    public void TogglePinned(HistoryRecord record)
    {
        record.pinned = !record.pinned;
    }

    public void JumpToRecord(HistoryRecord record)
    {
        CameraJumper.TryJumpAndSelect(record.GetThingToJumpTo());
    }

    public void OpenRecordMenu(HistoryRecord record)
    {
        Find.WindowStack.Add(new FloatMenu(GetActionMenuOptions(record)));
    }

    private List<FloatMenuOption> GetActionMenuOptions(HistoryRecord record)
    {
        return [
            new FloatMenuOption((record.pinned ? "NH_PH_HistoryCard_MenuUnpin" : "NH_PH_HistoryCard_MenuPin").Translate(), () => TogglePinned(record)),
            new FloatMenuOption("NH_PH_HistoryCard_MenuEdit".Translate(), () => BeginEditing(record)),
            new FloatMenuOption("NH_PH_HistoryCard_MenuDelete".Translate(), () => DeleteRecord(record)),
            new FloatMenuOption("NH_PH_HistoryCard_MenuCopyDescription".Translate(), () => CopyDescriptionToClipboard(record)),
        ];
    }

    public void HighlightTargets(HistoryRecord record)
    {
        foreach (var target in record.GlobalTargets)
            TargetHighlighter.Highlight(target);
    }

    public void OpenQuest(Quest quest)
    {
        if (quest == null)
            return;

        Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
        ((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
    }

    private bool CanGoToPreviousPage => state.Pagination.TotalPages > 0 && state.Pagination.CurrentPage > 1;
    private bool CanGoToNextPage => state.Pagination.TotalPages > 0 && state.Pagination.CurrentPage < state.Pagination.TotalPages;

}
