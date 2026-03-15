using PawnHistory.Source.DebugTools;
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
        //var pawns = DebugTools.AllPawns();
        //System.Diagnostics.Debugger.Break();

        containerPadding = 8f;
        gap = 10f;

        filterHeight = 30f;

        headerHeight = 25f;

        rowHeight = 32f;
        colGap = 5f;
        colWidthDate = 90f;
        colWidthIcon = 20f;
        colWidthDesc = 470f;
        cellPx = 5f;

        scrollWidth = 16f;
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
        //Widgets.Label(new Rect(colWidthDate + colGap, headerRect.y, colWidthIcon, headerHeight), "NH_PH_HistoryCard_HeaderEvent".Translate());
        Widgets.Label(new Rect(colWidthDate + colWidthIcon + colGap, headerRect.y, colWidthDesc, headerHeight), "NH_PH_HistoryCard_HeaderDescription".Translate());

        // --- SCROLL VIEW ---
        var tableY = filterHeight + gap + headerHeight;
        var outRect = new Rect(0, tableY, inRect.width, inRect.height - tableY);
        var viewRect = new Rect(0, 0, inRect.width - scrollWidth, rowHeight * comp.records.Count);

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        for (int i = comp.records.Count - 1; i >= 0; i--)
        {
            var rowIndex = comp.records.Count - 1 - i; // display in reversed order
            var record = comp.records[i];
            var row = new Rect(0, rowHeight * rowIndex, viewRect.width, rowHeight);
            if (i % 2 == 0) Widgets.DrawHighlight(row);

            var dateCell = new Rect(row.x + cellPx, row.y, colWidthDate, row.height);
            GUI.color = Color.gray; Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(dateCell, comp.GetShortDate(record));
            TooltipHandler.TipRegion(dateCell, comp.GetTipDate(record));

            GUI.color = Color.white;
            var iconCell = new Rect(colWidthDate, row.y + ((row.height - colWidthIcon) / 2), colWidthIcon, colWidthIcon);
            GUI.DrawTexture(iconCell, record.GetIcon(), ScaleMode.ScaleToFit);

            GUI.color = Color.white; Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft;
            var descCell = new Rect(colWidthDate + colWidthIcon + colGap, row.y, colWidthDesc, row.height);
            Widgets.Label(descCell, record.GetDescription());

            var ticksAgo = GenTicks.TicksAbs - record.date;
            var dateAgoText = $"Occurred {ticksAgo.ToStringTicksToPeriod()} ago";
            TooltipHandler.TipRegion(descCell, dateAgoText);
            if (Widgets.ButtonInvisible(row, record.concerns.Count > 0))
            {
                var thing = record.GetThingToJumpTo();
                CameraJumper.TryJumpAndSelect(thing);
            }
        }
        Widgets.EndScrollView();
        GUI.EndGroup();

        GUI.color = color;
        Text.Font = font;
        Text.Anchor = anchor;
    }
}
