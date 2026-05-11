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
            SizedBox(height: HistoryCardLayout.HeaderHeight, child: BuildHeader()),
            Expanded(ScrollView(
                Column(CurrentPageRecords(state).Select((record, index) => HistoryRecordRow.Build(ctx, state.Table, record, index, actions)), gap: 0f),
                key: "history-table-scroll",
                controller: state.TableScroll)),
        ], gap: 0f);
    }

    private static IEnumerable<HistoryRecord> CurrentPageRecords(HistoryCardState state)
    {
        var visibleRecords = state.Table.LastPawnShown?.VisibleHistoryRecords ?? [];
        var startIndex = (state.Pagination.CurrentPage - 1) * HistoryCardLayout.PageSize;
        return visibleRecords.Skip(startIndex).Take(HistoryCardLayout.PageSize);
    }

    private static Widget BuildHeader()
    {
        return Row(
        [
            SizedBox(width: HistoryCardLayout.CellPx),
            SizedBox(width: HistoryCardLayout.ColWidthDate, child: Label("NH_PH_HistoryCard_HeaderDate".Translate(), color: Color.gray)),
            SizedBox(width: HistoryCardLayout.ColGap + HistoryCardLayout.ColWidthIcon + HistoryCardLayout.ColGap),
            Expanded(Label("NH_PH_HistoryCard_HeaderDescription".Translate(), color: Color.gray)),
            SizedBox(width: HistoryCardLayout.ColGap + HistoryCardLayout.ColWidthIcon + HistoryCardLayout.CellPx + HistoryCardLayout.ScrollWidth),
        ], crossAxis: StackCrossAxis.Stretch, gap: 0f);
    }
}
