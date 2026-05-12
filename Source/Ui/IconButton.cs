using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class IconButton(Texture2D texture, Action onClick, float? iconSize, string tooltip = null, bool enabled = true, string key = null)
    : Widget(WidgetIds.IconButton, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        var width = iconSize ?? ctx.Theme.ButtonIconSize;
        var height = iconSize ?? ctx.Theme.ButtonIconSize;
        return constraints.Constrain(width, height);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        if (!tooltip.NullOrEmpty())
            TooltipHandler.TipRegion(rect, tooltip);

        var wasEnabled = GUI.enabled;
        GUI.enabled = wasEnabled && enabled;
        var clicked = Widgets.ButtonImage(rect, texture);
        GUI.enabled = wasEnabled;

        if (clicked)
            onClick?.Invoke();
    }
}
