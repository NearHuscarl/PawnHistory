using System.Runtime.CompilerServices;
using System.Text;
using PawnHistory.Source.Ui;
using RimWorld;
using UnityEngine;
using Verse;
using static PawnHistory.Source.Ui.W;

namespace PawnHistory.Source.PawnTracker.Ui;

internal readonly record struct HistoryRecordActions(
    System.Action<HistoryRecord> JumpToRecord,
    System.Action<HistoryRecord> OpenRecordMenu,
    System.Action<HistoryRecord> HighlightTargets,
    System.Action<Quest> OpenQuest,
    System.Action<string> UpdateEditingText,
    System.Action SaveEditedDescription,
    System.Action ClearEditingSession);

internal static class HistoryRecordRow
{
    private const float PinnedBorderWidth = 2f;
    private const string EditControlNamePrefix = "HistoryDescriptionEdit";
    private static readonly Color PinnedBorderColor = NeedsCardUtility.MoodColorNegative;

    public static Widget Build(UiContext ctx, HistoryTableState state, HistoryRecord record, int index, HistoryRecordActions actions)
    {
        var content = GestureDetector(
            ConstrainedBox(
                minHeight: HistoryCardLayout.MinRowHeight,
                child: Stack([
                    BuildRowBackground(index),
                    BuildCells(ctx, state, record, actions),
                    BuildPinnedBorder(record),
                ])),
            onTap: () => actions.JumpToRecord(record),
            onSecondaryTap: () => actions.OpenRecordMenu(record),
            onHover: () => actions.HighlightTargets(record),
            enabled: !state.HasActiveEditSession);

        return content;
    }

    private static Widget BuildCells(UiContext ctx, HistoryTableState state, HistoryRecord record, HistoryRecordActions actions)
    {
        var theme = ctx.Theme;
        return Row(
        [
            SizedBox(width: theme.PaddingXs),
            SizedBox(width: HistoryCardLayout.ColWidthDate, child: Tooltip(Label(record.GetShortDate(), GameFont.Tiny, color: Color.gray), record.GetTipDate())),
            SizedBox(dimension: theme.ButtonIconSize, Center(child: Image(record.Icon))),
            Expanded(BuildDescription(ctx, state, record, actions)),
            SizedBox(dimension: theme.ButtonIconSize, Center(child: BuildQuestButton(record, actions))),
            SizedBox(width: theme.PaddingXs),
        ], crossAxis: StackCrossAxis.Stretch, spacing: theme.PaddingSm);
    }

    private static Widget BuildDescription(UiContext ctx, HistoryTableState state, HistoryRecord record, HistoryRecordActions actions)
    {
        if (!state.IsEditing(record))
            return Tooltip(Label(record.description, GameFont.Tiny), GetTooltipOf(record));

        var key = GetEditControlName(record);
        if (state.PendingEditFocus)
        {
            ctx.RequestFocus(key);
            state.PendingEditFocus = false;
        }

        return TextField(
            state.EditingText,
            actions.UpdateEditingText,
            onSubmit: actions.SaveEditedDescription,
            onCancel: actions.ClearEditingSession,
            onClickOutside: actions.ClearEditingSession,
            minHeight: HistoryCardLayout.MinRowHeight,
            multiline: true,
            font: GameFont.Tiny,
            focusCursorToEnd: true,
            key: key);
    }

    private static Widget BuildQuestButton(HistoryRecord record, HistoryRecordActions actions)
    {
        if (record.quest == null)
            return SizedBoxShrink();

        return IconButton(
            TexCommand.OpenLinkedQuestTex,
            () => actions.OpenQuest(record.quest),
            tooltip: record.quest.name);
    }

    private static Widget BuildRowBackground(int index)
    {
        if (index % 2 != 0)
            return SizedBoxShrink();

        return Positioned(
            CustomPaint(Widgets.DrawHighlight),
            left: 0f,
            top: 0f,
            right: 0f,
            bottom: 0f);
    }

    private static Widget BuildPinnedBorder(HistoryRecord record)
    {
        if (!record.pinned)
            return SizedBoxShrink();

        return Positioned(
            ColoredBox(PinnedBorderColor, SizedBoxShrink()),
            top: 0f,
            right: 0f,
            bottom: 0f,
            width: PinnedBorderWidth);
    }

    private static string GetTooltipOf(HistoryRecord record)
    {
        var sb = new StringBuilder();
        var ticksAgo = GenTicks.TicksAbs - record.date;

        sb.AppendLine(record.def.LabelCap.Colorize(ColoredText.TipSectionTitleColor));
        sb.AppendLine();
        sb.AppendLine("NH_PH_HistoryCard_OccurredAgo".Translate(ticksAgo.ToStringTicksToPeriod()));

        if (record.pinned)
            sb.AppendLine("NH_PH_HistoryCard_RecordPinned".TranslateSimple());

        sb.AppendLine("NH_PH_HistoryCard_RightClickToOpenMenu".TranslateSimple());

        return sb.ToString();
    }

    private static string GetEditControlName(HistoryRecord record) => $"{EditControlNamePrefix}_{RuntimeHelpers.GetHashCode(record)}";
}
