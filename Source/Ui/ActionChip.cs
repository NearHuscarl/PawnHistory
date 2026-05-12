using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class ActionChip(Thing thing, Action<Thing> onRemove, string key = null) : Widget(WidgetIds.ActionChip, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        var theme = ctx.Theme;
        using (new TextStyleScope(GameFont.Tiny, TextAnchor.MiddleLeft))
        {
            var width = theme.ChipHorizontalPadding * 4f + theme.ChipIconSize * 2 + Text.CalcSize(thing.LabelShortCap).x;
            return constraints.Constrain(width, theme.ChipsHeight);
        }
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        Widgets.DrawMenuSection(rect);

        var theme = ctx.Theme;
        var layout = ChipLayout.For(rect, theme);

        Widgets.ThingIcon(layout.Icon, thing);
        using (new TextStyleScope(GameFont.Tiny, TextAnchor.MiddleLeft))
        {
            Widgets.Label(layout.Label, thing.LabelShortCap.Truncate(layout.Label.width));
        }

        if (Widgets.ButtonImage(layout.RemoveButton, Icons.Delete))
            onRemove?.Invoke(thing);
    }
    
    private readonly record struct ChipLayout(Rect Icon, Rect Label, Rect RemoveButton)
    {
        public static ChipLayout For(Rect rect, Theme theme)
        {
            var padding = theme.ChipHorizontalPadding;
            var iconSize = theme.ChipIconSize;

            var icon = new Rect(
                rect.x + padding,
                rect.y + (rect.height - iconSize) / 2f,
                iconSize,
                iconSize
            );

            var removeButton = new Rect(
                rect.xMax - padding - iconSize,
                rect.y + (rect.height - iconSize) / 2f,
                iconSize,
                iconSize
            );

            var label = new Rect(
                icon.xMax + padding,
                rect.y,
                Mathf.Max(0f, removeButton.xMin - icon.xMax - padding * 2f),
                rect.height
            );

            return new ChipLayout(icon, label, removeButton);
        }
    }
}
