using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class Label : Widget
{
    private readonly string text;
    private readonly GameFont font;
    private readonly TextAnchor anchor;
    private readonly float? width;
    private readonly float? height;

    public Label(string text, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.MiddleLeft, float? width = null, float? height = null, string key = null) : base(key)
    {
        this.text = text;
        this.font = font;
        this.anchor = anchor;
        this.width = width;
        this.height = height;
    }

    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        using (new TextStyleScope(font, anchor))
        {
            var desiredWidth = width ?? (constraints.HasBoundedWidth ? constraints.MaxWidth : Text.CalcSize(text).x);
            var measuredWidth = constraints.ConstrainWidth(desiredWidth);
            var desiredHeight = height ?? Text.CalcHeight(text, Mathf.Max(1f, measuredWidth));
            var measuredHeight = constraints.ConstrainHeight(desiredHeight);

            return constraints.Constrain(new Vector2(measuredWidth, measuredHeight));
        }
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        using (new TextStyleScope(font, anchor))
        {
            Widgets.Label(rect, text);
        }
    }
}
