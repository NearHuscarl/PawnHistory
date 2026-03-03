using UnityEngine;
using Verse;

namespace PawnHistory.Source.WorldPawn
{
    class Widget
    {
        private static Color defaultColor = GUI.color;

        public static bool IconButtonWorker(Rect cellRect, Texture2D buttonTex, int buttonWidth, int buttonHeight, Color color)
        {
            var rect = new Rect(cellRect.center.x - buttonWidth / 2, cellRect.center.y - buttonHeight / 2, buttonWidth, buttonHeight);
            TooltipHandler.TipRegionByKey(rect, "DefInfoTip");
            bool result = Widgets.ButtonImage(rect, buttonTex, color);
            UIHighlighter.HighlightOpportunity(rect, "InfoCard");
            return result;
        }

        public static bool IconButtonWorker(Rect cellRect, Texture2D buttonTex, int buttonWidth, int buttonHeight)
        {
            return IconButtonWorker(cellRect, buttonTex, buttonWidth, buttonHeight, GUI.color);
        }
    }
}
