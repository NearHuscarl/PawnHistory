using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class Button(string label, Action onClick, float? width = null, float? height = null, bool enabled = true, string key = null)
    : Widget(WidgetIds.Button, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        var desiredWidth = width ?? Text.CalcSize(label).x + ctx.Theme.ButtonHorizontalPadding * 2f;
        var desiredHeight = height ?? ctx.Theme.ButtonHeight;
        return constraints.Constrain(desiredWidth, desiredHeight);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        var wasEnabled = GUI.enabled;
        GUI.enabled = wasEnabled && enabled;
        var clicked = Widgets.ButtonText(rect, label);
        GUI.enabled = wasEnabled;

        if (clicked)
            onClick?.Invoke();
    }
}
