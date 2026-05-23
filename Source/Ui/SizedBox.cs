using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class SizedBox(float? width = null, float? height = null, Widget child = null, string key = null, bool debug = false)
    : Widget(WidgetIds.SizedBox, key)
{
    public static SizedBox Shrink(string key = null) => new(0f, 0f, key: key);

    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        if (child == null || width.HasValue && height.HasValue)
            return constraints.Constrain(width ?? 0f, height ?? 0f);

        var childConstraints = constraints.CopyWith(maxWidth: width, maxHeight: height);
        var childSize = child.Measure(ctx, childConstraints);
        return constraints.Constrain(width ?? childSize.x, height ?? childSize.y);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        WidgetTree.DrawChild(ctx, child, 0, rect);

        if (debug)
            Widgets.DrawBox(rect);
    }
}
