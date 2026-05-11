using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class ThingTile(Thing thing, float? padding = null, string key = null) : Widget(WidgetIds.ThingTile, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        if (thing == null)
            return Vector2.zero;

        var labelSize = Text.CalcSize(thing.LabelCap);
        var p = padding ?? ctx.Theme.PaddingXs;

        var width = constraints.HasBoundedWidth
            ? constraints.MaxWidth
            : labelSize.x + ctx.Theme.GapXs + labelSize.y + p * 2f;

        var height = constraints.HasBoundedHeight
            ? constraints.MaxHeight
            : labelSize.y + p * 2f;

        return constraints.Constrain(width, height);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        if (thing == null)
            return;

        var theme = ctx.Theme;
        var p = padding ?? theme.PaddingXs;
        var iconSize = rect.height - p * 2f;

        var iconRect = new Rect(
            rect.x + p,
            rect.y + p,
            iconSize,
            iconSize);

        var labelRect = new Rect(
            iconRect.xMax + theme.GapXs,
            rect.y,
            rect.xMax - iconRect.xMax - theme.GapXs - p,
            rect.height);

        Widgets.ThingIcon(iconRect, thing);
        Widgets.Label(labelRect, thing.LabelCap);
    }
}