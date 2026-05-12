using PawnHistory.Source.Helper;
using PawnHistory.Source.Ui;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static PawnHistory.Source.Ui.W;

namespace PawnHistory.Source.PawnTracker.Ui;

internal static class HistoryTable
{
    public static Widget Build(UiContext ctx, HistoryCardState state, HistoryRecordActions actions)
    {
        return Column(
        [
            SizedBox(height: ctx.Theme.Gap),
            SizedBox(height: HistoryCardLayout.HeaderHeight, child: BuildHeader(ctx)),
            Expanded(ScrollView(
                Column(CurrentPageRecords(state).Select((record, index) => HistoryRecordRow.Build(ctx, state.Table, record, index, actions))),
                key: "history-table-scroll",
                controller: state.TableScroll)),
        ]);
    }

    private static IEnumerable<HistoryRecord> CurrentPageRecords(HistoryCardState state)
    {
        var visibleRecords = state.Table.LastPawnShown?.VisibleHistoryRecords ?? [];
        var startIndex = (state.Pagination.CurrentPage - 1) * HistoryCardLayout.PageSize;
        return visibleRecords.Skip(startIndex).Take(HistoryCardLayout.PageSize);
    }

    private static Widget BuildHeader(UiContext ctx)
    {
        var theme = ctx.Theme;
        
        return Row(
        [
            SizedBox(width: theme.PaddingXs),
            SizedBox(width: HistoryCardLayout.ColWidthDate, child: Label("NH_PH_HistoryCard_HeaderDate".Translate(), color: Color.gray)),
            SizedBox(width: theme.ButtonIconSize),
            Expanded(Label("NH_PH_HistoryCard_HeaderDescription".Translate(), color: Color.gray)),
        ], crossAxis: StackCrossAxis.Stretch, spacing: theme.PaddingSm);
    }
}
