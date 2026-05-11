using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class Tooltip(Widget child, string tip, string key = null) : Widget(WidgetIds.Tooltip, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        // Layout-transparent: Tooltip contributes exactly the child size and no extra space.
        return child?.Measure(ctx, constraints) ?? Vector2.zero;
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        WidgetTree.DrawChild(ctx, child, 0, rect);

        if (!tip.NullOrEmpty())
            TooltipHandler.TipRegion(rect, tip);
    }
}
