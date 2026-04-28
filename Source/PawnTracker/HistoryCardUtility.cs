using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class HistoryCardUtility
{
    private const float PinnedBorderWidth = 2f;
    private static readonly Color PinnedBorderColor = NeedsCardUtility.MoodColorNegative;

    private static float containerPadding;
    /// <summary>
    /// default gap between common UI controls
    /// </summary>
    private static float gap;

    private static float filterHeight;

    private static float headerHeight;

    private static float minRowHeight;
    private static float colGap;
    private static float colWidthDate;
    private static float colWidthIcon;
    private static float colWidthDesc;
    private static float colWidthQuest;
    private static float cellPx;

    private static float scrollWidth;
    public static Vector2 ScrollPosition;

    static HistoryCardUtility() => ReloadLayoutConfig();

    [Reloadable]
    [NearDebugAction]
    private static void ReloadLayoutConfig()
    {
        containerPadding = 8f;
        gap = 10f;

        filterHeight = 30f;

        headerHeight = 25f;

        minRowHeight = 32f;
        colGap = 5f;
        colWidthDate = 90f;
        colWidthIcon = 20f;
        colWidthDesc = 470f;
        colWidthQuest = 20f;
        cellPx = 5f;

        scrollWidth = 16f;
        ScrollPosition = Vector2.zero;
    }

    private static readonly Dictionary<HistoryRecord, float> CachedHeights = [];
    private static float GetRowHeight(HistoryRecord record)
    {
        Text.Font = GameFont.Tiny;

        if (CachedHeights.TryGetValue(record, out var h))
            return h;
        
        var textHeight = Text.CalcHeight(LangUtility.StripColorTags(record.description), colWidthDesc);
        h = Mathf.Max(textHeight, minRowHeight);
        CachedHeights[record] = h;
        return h;
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

        sb.AppendLine(record.def.label.CapitalizeFirst());
        sb.AppendLine();
        sb.AppendLine($"Occurred {ticksAgo.ToStringTicksToPeriod()} ago");
        
        if (record.pinned)
            sb.AppendLine("This record is pinned. Pinned record will never be removed.");

        sb.AppendLine("Right click: Open the action menu.");
        
        return sb.ToString();
    }

    public static void DrawHistoryCard(Rect tabRect, Pawn pawn)
    {
        var color = GUI.color;
        var font = Text.Font;
        var anchor = Text.Anchor;

        var inRect = tabRect.ContractedBy(containerPadding);
        var records = pawn.HistoryRecords.Where(r => r.def.importance != RecordImportance.Debug).ToList();

        GUI.BeginGroup(inRect);

        // --- HEADER SETUP ---
        Text.Font = GameFont.Small; GUI.color = Color.gray; Text.Anchor = TextAnchor.MiddleLeft;

        var header = new Rect(0, filterHeight + gap, inRect.width, headerHeight);
        var dateHeaderCell = new Rect(cellPx, header.y, colWidthDate, headerHeight);
        Widgets.Label(dateHeaderCell, "NH_PH_HistoryCard_HeaderDate".Translate());
        var iconHeaderCell = new Rect(dateHeaderCell.xMax, header.y + (header.height - colWidthIcon) / 2, colWidthIcon, colWidthIcon); // used to calculate next cell rect
        var descHeaderCell = new Rect(iconHeaderCell.xMax + colGap, header.y, colWidthDesc, headerHeight);
        Widgets.Label(descHeaderCell, "NH_PH_HistoryCard_HeaderDescription".Translate());

        // --- SCROLL VIEW ---
        var tableY = filterHeight + gap + headerHeight;
        var outRect = new Rect(0, tableY, inRect.width, inRect.height - tableY);
        var totalHeight = records.Sum(GetRowHeight);
        var viewRect = new Rect(0, 0, inRect.width - scrollWidth, totalHeight);

        Widgets.BeginScrollView(outRect, ref ScrollPosition, viewRect);
        var curY = 0f;
        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var rowHeight = GetRowHeight(record);
            var row = new Rect(0, curY, viewRect.width, rowHeight);
            if (i % 2 == 0) Widgets.DrawHighlight(row);
            if (record.pinned) DrawPinnedBorder(row);

            var dateCell = new Rect(row.x + cellPx, row.y, colWidthDate, row.height);
            GUI.color = Color.gray; Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(dateCell, record.GetShortDate());
            TooltipHandler.TipRegion(dateCell, record.GetTipDate());

            GUI.color = Color.white;
            var iconCell = new Rect(dateCell.xMax, row.y + (row.height - colWidthIcon) / 2, colWidthIcon, colWidthIcon);
            GUI.DrawTexture(iconCell, record.Icon, ScaleMode.ScaleToFit);

            GUI.color = Color.white; Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft;
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
        GUI.EndGroup();

        GUI.color = color;
        Text.Font = font;
        Text.Anchor = anchor;
    }
}
