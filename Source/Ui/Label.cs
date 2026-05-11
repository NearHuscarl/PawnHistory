using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class Label(string text, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.MiddleLeft, float? width = null, float? height = null, Color? color = null, string key = null)
    : Widget(WidgetIds.Label, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        using (new TextStyleScope(font, anchor))
        {
            var desiredWidth = width ?? (constraints.HasBoundedWidth ? constraints.MaxWidth : Text.CalcSize(text).x);
            var measuredWidth = constraints.ConstrainWidth(desiredWidth);
            var desiredHeight = height ?? Text.CalcHeight(text, Mathf.Max(1f, measuredWidth));
            var measuredHeight = constraints.ConstrainHeight(desiredHeight);

            return constraints.Constrain(measuredWidth, measuredHeight);
        }
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        using (new TextStyleScope(font, anchor))
        {
            var oldColor = GUI.color;
            if (color.HasValue)
                GUI.color = color.Value;
            Widgets.Label(rect, text);
            GUI.color = oldColor;
        }
    }
}
