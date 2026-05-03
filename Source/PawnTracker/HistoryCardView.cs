using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PawnHistory.Source.DebugTools;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public readonly record struct HistoryCardLayout(float Gap);

public static class HistoryCardView
{
    private const float PinnedBorderWidth = 2f;
    private static readonly Color PinnedBorderColor = NeedsCardUtility.MoodColorNegative;
    
    private static float headerHeight;

    private static float minRowHeight;
    private static float colGap;
    private static float colWidthDate;
    private static float colWidthIcon;
    private static float colWidthDesc;
    private static float colWidthQuest;
    private static float cellPx;

    private static float scrollWidth;
    
    [Reloadable]
    [NearDebugAction]
    private static void ReloadHistoryCardViewLayout()
    {
        headerHeight = 25f;

        minRowHeight = 32f;
        colGap = 5f;
        colWidthDate = 90f;
        colWidthIcon = 20f;
        colWidthDesc = 470f;
        colWidthQuest = 20f;
        cellPx = 5f;

        scrollWidth = 16f;
    }

    public static void Draw(
        Rect inRect,
        List<HistoryRecord> pageRecords,
        ref Vector2 scrollPosition,
        ref bool pendingScrollToBottom,
        Dictionary<HistoryRecord, float> cachedHeights,
        HistoryCardLayout layout)
    {
        Text.Font = GameFont.Small;
        GUI.color = Color.gray;
        Text.Anchor = TextAnchor.MiddleLeft;

        var header = new Rect(0, layout.Gap, inRect.width, headerHeight);
        var dateHeaderCell = new Rect(cellPx, header.y, colWidthDate, headerHeight);
        Widgets.Label(dateHeaderCell, "NH_PH_HistoryCard_HeaderDate".Translate());
        var iconHeaderCell = new Rect(dateHeaderCell.xMax, header.y + (header.height - colWidthIcon) / 2, colWidthIcon, colWidthIcon);
        var descHeaderCell = new Rect(iconHeaderCell.xMax + colGap, header.y, colWidthDesc, headerHeight);
        Widgets.Label(descHeaderCell, "NH_PH_HistoryCard_HeaderDescription".Translate());

        var tableY = headerHeight + layout.Gap;
        var outRect = new Rect(0, tableY, inRect.width, inRect.height - tableY);
        var totalHeight = pageRecords.Sum(record => GetRowHeight(record, cachedHeights, layout));
        var viewRect = new Rect(0, 0, inRect.width - scrollWidth, totalHeight);

        ApplyScrollState(ref scrollPosition, ref pendingScrollToBottom, totalHeight, outRect.height);

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        var curY = 0f;
        for (var i = 0; i < pageRecords.Count; i++)
        {
            var record = pageRecords[i];
            var rowHeight = GetRowHeight(record, cachedHeights, layout);
            var row = new Rect(0, curY, viewRect.width, rowHeight);
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

            GUI.color = Color.white;
            var iconCell = new Rect(dateCell.xMax, row.y + (row.height - colWidthIcon) / 2, colWidthIcon, colWidthIcon);
            GUI.DrawTexture(iconCell, record.Icon, ScaleMode.ScaleToFit);

            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            var descCell = new Rect(iconCell.xMax + colGap, row.y, colWidthDesc, row.height);
            Widgets.Label(descCell, record.description);

            var questCell = new Rect(descCell.xMax + colGap, row.y + (row.height - colWidthQuest) / 2, colWidthQuest, colWidthQuest);
            if (record.quest != null)
            {
                if (Widgets.ButtonImage(questCell, TexCommand.OpenLinkedQuestTex))
                {
                    Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
                    ((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(record.quest);
                }

                if (Mouse.IsOver(questCell))
                    TooltipHandler.TipRegion(questCell, record.quest.name);
            }

            TooltipHandler.TipRegion(descCell, GetTooltipOf(record));

            if (Mouse.IsOver(row))
            {
                foreach (var target in record.GlobalTargets)
                    TargetHighlighter.Highlight(target);
            }

            if (Mouse.IsOver(row) && Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
            {
                if (Event.current.button == 0)
                    CameraJumper.TryJumpAndSelect(record.GetThingToJumpTo());
                else if (Event.current.button == 1)
                    Find.WindowStack.Add(new FloatMenu(HistoryCardMenuOptions.GetActionMenuOptions(record)));

                Event.current.Use();
            }

            curY += rowHeight;
        }

        Widgets.EndScrollView();
    }

    private static void ApplyScrollState(ref Vector2 scrollPosition, ref bool pendingScrollToBottom, float totalHeight, float viewportHeight)
    {
        scrollPosition.x = 0f;

        if (pendingScrollToBottom)
        {
            scrollPosition.y = Mathf.Max(0f, totalHeight - viewportHeight);
            pendingScrollToBottom = false;
            return;
        }

        scrollPosition.y = Mathf.Clamp(scrollPosition.y, 0f, Mathf.Max(0f, totalHeight - viewportHeight));
    }

    private static float GetRowHeight(HistoryRecord record, Dictionary<HistoryRecord, float> cachedHeights, HistoryCardLayout layout)
    {
        Text.Font = GameFont.Tiny;

        if (cachedHeights.TryGetValue(record, out var height))
            return height;

        var textHeight = Text.CalcHeight(LangUtility.StripColorTags(record.description), colWidthDesc);
        height = Mathf.Max(textHeight, minRowHeight);
        cachedHeights[record] = height;
        return height;
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
        sb.AppendLine($"Occurred {ticksAgo.ToStringTicksToPeriod()} ago");

        if (record.pinned)
            sb.AppendLine("This record is pinned. Pinned record will never be removed.");

        sb.AppendLine("Right click: Open the action menu.");

        return sb.ToString();
    }
}
