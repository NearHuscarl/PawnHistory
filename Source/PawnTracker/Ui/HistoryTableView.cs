using PawnHistory.Source.DebugTools;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using PawnHistory.Source.Helper;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public readonly record struct HistoryTableLayout(float Gap);

public static class HistoryTableView
{
    private const float PinnedBorderWidth = 2f;
    private const string EditControlNamePrefix = "HistoryDescriptionEdit";
    private static readonly Color PinnedBorderColor = NeedsCardUtility.MoodColorNegative;

    private static float headerHeight;

    private static float minRowHeight;
    private static float colGap;
    private static float colWidthDate;
    private static float colWidthIcon;
    private static float colWidthQuest;
    private static float cellPx;

    private static float scrollWidth;

    private static bool drawDebuggingBox;
    
    static HistoryTableView() => ReloadHistoryTableViewLayout();

    [Reloadable]
    [NearDebugAction]
    private static void ReloadHistoryTableViewLayout()
    {
        headerHeight = 25f;

        minRowHeight = 32f;
        colGap = 5f;
        colWidthDate = 90f;
        colWidthIcon = 20f;
        colWidthQuest = 20f;
        cellPx = 5f;

        scrollWidth = 16f;
        drawDebuggingBox = false;
    }

    private static List<HistoryRecord> GetCurrentPageRecords(HistoryTableState tableState, PaginationState paginationState)
    {
        var visibleRecords = tableState.LastPawnShown.VisibleHistoryRecords;
        var startIndex = (paginationState.CurrentPage - 1) * PaginationView.PageSize;
        return visibleRecords.Skip(startIndex).Take(PaginationView.PageSize).ToList();
    }

    public static void Draw(Rect inRect, HistoryTableState tableState, PaginationState paginationState, ref Vector2 scrollPosition, HistoryTableLayout layout, List<Command> commands)
    {
        GUI.BeginGroup(inRect);
        
        Text.Font = GameFont.Small;
        GUI.color = Color.gray;
        Text.Anchor = TextAnchor.MiddleLeft;

        var header = new Rect(0f, layout.Gap, inRect.width, headerHeight);
        var descWidth = inRect.width - cellPx * 2 - colWidthDate - colWidthIcon - colWidthQuest - colGap * 3f - scrollWidth;
        var dateHeaderCell = new Rect(cellPx, header.y, colWidthDate, headerHeight);
        Widgets.Label(dateHeaderCell, "NH_PH_HistoryCard_HeaderDate".Translate());
        var iconHeaderCell = new Rect(dateHeaderCell.xMax + colGap, header.y + (header.height - colWidthIcon) / 2, colWidthIcon, colWidthIcon);
        var descHeaderCell = new Rect(iconHeaderCell.xMax + colGap, header.y, descWidth, headerHeight);
        Widgets.Label(descHeaderCell, "NH_PH_HistoryCard_HeaderDescription".Translate());

        var tableY = layout.Gap + headerHeight;
        var outRect = new Rect(0f, tableY, inRect.width, inRect.height - tableY);
        var records = GetCurrentPageRecords(tableState, paginationState);
        var totalHeight = records.Sum(r => GetRowHeight(r, descWidth, tableState));
        var viewRect = new Rect(0f, 0f, inRect.width - scrollWidth, totalHeight);

        ApplyScrollState(tableState, ref scrollPosition, totalHeight, outRect.height);

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        var curY = 0f;
        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var rowHeight = GetRowHeight(record, descWidth, tableState);
            var row = new Rect(0f, curY, viewRect.width, rowHeight);
            if (i % 2 == 0)
                Widgets.DrawHighlight(row);
            if (record.pinned)
                DrawPinnedBorder(row);

            var dateCell = new Rect(row.x + cellPx, row.y, colWidthDate, row.height);
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(dateCell, record.GetShortDate());
            TooltipHandler.TipRegion(dateCell, record.GetTipDate());
            if (drawDebuggingBox) Widgets.DrawBox(dateCell);

            GUI.color = Color.white;
            var iconCell = new Rect(dateCell.xMax + colGap, row.y + (row.height - colWidthIcon) / 2, colWidthIcon, colWidthIcon);
            GUI.DrawTexture(iconCell, record.Icon, ScaleMode.ScaleToFit);
            if (drawDebuggingBox) Widgets.DrawBox(iconCell);

            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            var descCell = new Rect(iconCell.xMax + colGap, row.y, descWidth, row.height);
            if (tableState.IsEditing(record))
                DrawEditingDescriptionCell(descCell, record, tableState, commands);
            else
                Widgets.Label(descCell, record.description);
            if (drawDebuggingBox) Widgets.DrawBox(descCell);

            var questCell = new Rect(descCell.xMax + colGap, row.y + (row.height - colWidthQuest) / 2, colWidthQuest, colWidthQuest);
            if (drawDebuggingBox) Widgets.DrawBox(questCell);
            if (record.quest != null)
            {
                if (Widgets.ButtonImage(questCell, TexCommand.OpenLinkedQuestTex))
                {
                    Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
                    ((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(record.quest);
                }

                TooltipHandler.TipRegion(questCell, record.quest.name);
            }

            if (!tableState.IsEditing(record))
                TooltipHandler.TipRegion(descCell, GetTooltipOf(record));

            if (Mouse.IsOver(row))
            {
                foreach (var target in record.GlobalTargets)
                    TargetHighlighter.Highlight(target);
            }

            if (!tableState.HasActiveEditSession && Mouse.IsOver(row) && Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                    CameraJumper.TryJumpAndSelect(record.GetThingToJumpTo());
                else if (Event.current.button == 1)
                    Find.WindowStack.Add(new FloatMenu(HistoryCardMenuOptions.GetActionMenuOptions(record, commands.Add)));

                Event.current.Use();
            }

            curY += rowHeight;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();
    }

    private static void ApplyScrollState(HistoryTableState tableState, ref Vector2 scrollPosition, float totalHeight, float viewportHeight)
    {
        scrollPosition.x = 0f;

        if (tableState.PendingScrollToBottom)
        {
            scrollPosition.y = Mathf.Max(0f, totalHeight - viewportHeight);
            tableState.PendingScrollToBottom = false;
            return;
        }

        scrollPosition.y = Mathf.Clamp(scrollPosition.y, 0f, Mathf.Max(0f, totalHeight - viewportHeight));
    }

    private static float GetRowHeight(HistoryRecord record, float descWidth, HistoryTableState state)
    {
        Text.Font = GameFont.Tiny;

        if (state.IsEditing(record))
            return Mathf.Max(Text.CurTextAreaStyle.CalcHeight(new GUIContent(state.EditingText), descWidth), minRowHeight);

        if (state.CachedHeights.TryGetValue(record, out var height))
            return height;

        var textHeight = Text.CalcHeight(record.description, descWidth);
        height = Mathf.Max(textHeight, minRowHeight);
        state.CachedHeights[record] = height;
        return height;
    }

    private static void DrawEditingDescriptionCell(Rect descCell, HistoryRecord record, HistoryTableState tableState, List<Command> commands)
    {
        var controlName = GetEditControlName(record);
        var current = Event.current;
        var hasFocus = GUI.GetNameOfFocusedControl() == controlName;

        if (hasFocus && current.type == EventType.KeyDown)
        {
            switch (current.keyCode)
            {
                case KeyCode.Escape:
                    commands.Add(new CancelEditedRecord());
                    UI.UnfocusCurrentControl();
                    current.Use();
                    return;
                case KeyCode.Return or KeyCode.KeypadEnter when !current.shift:
                    commands.Add(new SaveEditedRecord());
                    UI.UnfocusCurrentControl();
                    current.Use();
                    return;
            }
        }

        if (hasFocus && Event.current.type == EventType.MouseDown && !Mouse.IsOver(descCell))
        {
            commands.Add(new CancelEditedRecord());
            UI.UnfocusCurrentControl();
            current.Use();
            return;
        }

        GUI.SetNextControlName(controlName);
        // must place after custom event handling
        tableState.EditingText = Widgets.TextArea(descCell, tableState.EditingText);

        if (!hasFocus && tableState.HasActiveEditSession)
        {
            UI.FocusControl(controlName, Find.WindowStack.currentlyDrawnWindow);
            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            editor.OnFocus();
            editor.MoveTextEnd();
        }
    }

    private static void DrawPinnedBorder(Rect row)
    {
        var pinnedBorderRect = new Rect(row.xMax - PinnedBorderWidth, row.y, PinnedBorderWidth, row.height);
        GUI.color = PinnedBorderColor;
        GUI.DrawTexture(pinnedBorderRect, BaseContent.WhiteTex);
        GUI.color = Color.white;
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
