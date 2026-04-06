using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class HistoryCardUtility
{
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
    private static float cellPx;

    private static float scrollWidth;
    public static Vector2 scrollPosition;

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
        cellPx = 5f;

        scrollWidth = 16f;
        scrollPosition = Vector2.zero;
    }

    private static readonly Dictionary<HistoryRecord, float> cachedHeights = [];
    private static float GetRowHeight(HistoryRecord record)
    {
        Text.Font = GameFont.Tiny;

        if (!cachedHeights.TryGetValue(record, out var h))
        {
            var textHeight = Text.CalcHeight(LangUtility.StripColorTags(record.description), colWidthDesc);
            h = Mathf.Max(textHeight, minRowHeight);
            cachedHeights[record] = h;
        }
        return h;
    }

    public static void DrawHistoryCard(Rect tabRect, Pawn pawn)
    {
        var color = GUI.color;
        var font = Text.Font;
        var anchor = Text.Anchor;

        var inRect = tabRect.ContractedBy(containerPadding);
        var records = pawn.GetHistoryRecords();

        GUI.BeginGroup(inRect);

        // --- HEADER SETUP ---
        Text.Font = GameFont.Small; GUI.color = Color.gray; Text.Anchor = TextAnchor.MiddleLeft;

        var headerRect = new Rect(0, filterHeight + gap, inRect.width, headerHeight);
        Widgets.Label(new Rect(cellPx, headerRect.y, colWidthDate, headerHeight), "NH_PH_HistoryCard_HeaderDate".Translate());
        //Widgets.Label(new Rect(colWidthDate + colGap, headerRect.y, colWidthIcon, headerHeight), "NH_PH_HistoryCard_HeaderEvent".Translate());
        Widgets.Label(new Rect(colWidthDate + colWidthIcon + colGap, headerRect.y, colWidthDesc, headerHeight), "NH_PH_HistoryCard_HeaderDescription".Translate());

        // --- SCROLL VIEW ---
        var tableY = filterHeight + gap + headerHeight;
        var outRect = new Rect(0, tableY, inRect.width, inRect.height - tableY);
        var totalHeight = records.Sum(GetRowHeight);
        var viewRect = new Rect(0, 0, inRect.width - scrollWidth, totalHeight);

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var rowHeight = GetRowHeight(record);
            var row = new Rect(0, i * rowHeight, viewRect.width, rowHeight);
            if (i % 2 == 0) Widgets.DrawHighlight(row);

            var dateCell = new Rect(row.x + cellPx, row.y, colWidthDate, row.height);
            GUI.color = Color.gray; Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(dateCell, record.GetShortDate());
            TooltipHandler.TipRegion(dateCell, record.GetTipDate());

            GUI.color = Color.white;
            var iconCell = new Rect(colWidthDate, row.y + ((row.height - colWidthIcon) / 2), colWidthIcon, colWidthIcon);
            GUI.DrawTexture(iconCell, record.GetIcon(), ScaleMode.ScaleToFit);

            GUI.color = Color.white; Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft;
            var descCell = new Rect(colWidthDate + colWidthIcon + colGap, row.y, colWidthDesc, row.height);
            Widgets.Label(descCell, record.description);

            var ticksAgo = GenTicks.TicksAbs - record.date;
            var dateAgoText = $"Occurred {ticksAgo.ToStringTicksToPeriod()} ago";
            TooltipHandler.TipRegion(descCell, dateAgoText);

            if (Mouse.IsOver(row))
            {
                foreach (var target in record.AllTargets)
                    TargetHighlighter.Highlight(target);
            }
            if (Mouse.IsOver(row) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                CameraJumper.TryJumpAndSelect(record.GetThingToJumpTo());
            }
            else if (Mouse.IsOver(descCell) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                GUIUtility.systemCopyBuffer = LangUtility.StripColorTags(record.description);
                Messages.Message("Record is copied to clipboard.", MessageTypeDefOf.NeutralEvent);
            }
        }
        Widgets.EndScrollView();
        GUI.EndGroup();

        GUI.color = color;
        Text.Font = font;
        Text.Anchor = anchor;
    }
}
