using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker
{
    public class HistoryCardUtility
    {
        public static readonly float labelPadding = 2f;
        public static readonly float rowHeight = 28f;
        public static readonly float scrollWidth = 20f;
        public static readonly float headerHeight = 35f;
        public static Rect HistoryRect = new(0.0f, 0.0f, 800f, 480f);
        public static Vector2 scrollPosition = Vector2.zero;

        public static void DrawHistoryCard(Rect cardRect, Pawn pawn, CompHistory comp)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;

            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            var outRect = new Rect(cardRect.x, cardRect.y + headerHeight, cardRect.width, cardRect.height - headerHeight);
            var viewHeight = rowHeight * 15 + 3;
            var viewRect = new Rect(0.0f, 0.0f, cardRect.width - scrollWidth, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            for (var i = 0; i < comp.records.Count; i++)
            {
                var rect5 = new Rect(0.0f, i * rowHeight, viewRect.width, rowHeight);
                var num8 = rect5.y + rect5.height / 2f;
                var y2 = num8 - Text.LineHeight / 2f;
                var rect6 = new Rect(rect5.x + labelPadding, y2, 180, Text.LineHeight);
                var record = comp.records[i];
                Widgets.Label(rect6, $"{record.Date} {record.EventDef.defName} {record.EventDef.description}");
            }
            Widgets.EndScrollView();

            Text.Anchor = anchor;
            Text.Font = font;
        }
    }
}
