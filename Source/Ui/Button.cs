using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class Button(string label, Action onClick, float? width = null, float? height = null, string key = null) : Widget(key)
{
    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        var desiredWidth = width ?? Text.CalcSize(label).x + ctx.Theme.ButtonHorizontalPadding * 2f;
        var desiredHeight = height ?? ctx.Theme.ButtonHeight;
        return constraints.Constrain(new Vector2(desiredWidth, desiredHeight));
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        if (Widgets.ButtonText(rect, label))
            onClick?.Invoke();
    }
}
