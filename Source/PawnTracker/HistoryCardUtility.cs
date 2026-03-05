using RimWorld;
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

    private static float rowHeight;
    private static float colGap;
    private static float colWidthDate;
    private static float colWidthEvent;
    private static float colWidthDesc;
    private static float cellPx;
    private static int visibleRecords;

    private static float scrollWidth;
    public static Vector2 scrollPosition;

    static HistoryCardUtility() => ReloadLayoutConfig();

    [Reloadable]
    [NearDebugAction]
    private static void ReloadLayoutConfig()
    {
        var pawns = DebugTools.AllCorpses();
        System.Diagnostics.Debugger.Break();
        containerPadding = 8f;
        gap = 10f;

        filterHeight = 30f;

        headerHeight = 25f;

        rowHeight = 28f;
        colGap = 5f;
        colWidthDate = 90f;
        colWidthEvent = 100f;
        colWidthDesc = 400f;
        cellPx = 5f;
        visibleRecords = 14;

        scrollWidth = 10f;
        scrollPosition = Vector2.zero;
    }

    public static void DrawHistoryCard(Rect tabRect, Pawn pawn, CompHistory comp)
    {
        var color = GUI.color;
        var font = Text.Font;
        var anchor = Text.Anchor;

        var inRect = tabRect.ContractedBy(containerPadding);

        GUI.BeginGroup(inRect);

        // --- HEADER SETUP ---
        Text.Font = GameFont.Small; GUI.color = Color.gray; Text.Anchor = TextAnchor.MiddleLeft;

        var headerRect = new Rect(0, filterHeight + gap, inRect.width, headerHeight);
        Widgets.Label(new Rect(cellPx, headerRect.y, colWidthDate, headerHeight), "NH_PH_HistoryCard_HeaderDate".Translate());
        Widgets.Label(new Rect(colWidthDate + colGap, headerRect.y, colWidthEvent, headerHeight), "NH_PH_HistoryCard_HeaderEvent".Translate());
        Widgets.Label(new Rect(colWidthDate + colWidthEvent + colGap * 2, headerRect.y, colWidthDesc, headerHeight), "NH_PH_HistoryCard_HeaderDescription".Translate());

        // --- SCROLL VIEW ---
        var tableY = filterHeight + gap + headerHeight;
        var outRect = new Rect(0, tableY, inRect.width, inRect.height - tableY);
        var viewRect = new Rect(0, 0, inRect.width - scrollWidth, rowHeight * visibleRecords);

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        for (var i = comp.records.Count - 1; i >= 0; i--)
        {
            var record = comp.records[i];
            var row = new Rect(0, rowHeight * i, viewRect.width, rowHeight);
            if (i % 2 == 0) Widgets.DrawHighlight(row);

            var dateCell = new Rect(row.x + cellPx, row.y, colWidthDate, row.height);
            GUI.color = Color.gray; Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(dateCell, comp.GetShortDate(record));
            TooltipHandler.TipRegion(dateCell, comp.GetTipDate(record));

            var eventCell = new Rect(colWidthDate + colGap, row.y, colWidthEvent, row.height);
            GUI.color = Color.white; Text.Font = GameFont.Small; Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(eventCell, record.eventDef.defName);

            GUI.color = Color.white; Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft;
            var descCell = new Rect(colWidthDate + colWidthEvent + colGap * 2, row.y, colWidthDesc, row.height);
            Widgets.Label(descCell, record.GetDescription());

            var ticksAgo = GenTicks.TicksAbs - record.date;
            var dateAgoText = $"Occurred {ticksAgo.ToStringTicksToPeriod()} ago";
            TooltipHandler.TipRegion(descCell, dateAgoText);
        }
        Widgets.EndScrollView();
        GUI.EndGroup();

        GUI.color = color;
        Text.Font = font;
        Text.Anchor = anchor;
    }
}
