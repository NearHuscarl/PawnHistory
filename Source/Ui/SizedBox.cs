using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class SizedBox(float? width = null, float? height = null, Widget child = null, string key = null)
    : Widget(WidgetIds.SizedBox, key)
{
    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        if (child == null || width.HasValue && height.HasValue)
            return constraints.Constrain(new Vector2(width ?? 0f, height ?? 0f));

        var childConstraints = constraints.CopyWith(maxWidth: width, maxHeight: height);
        var childSize = child.Measure(ctx, childConstraints);
        return constraints.Constrain(new Vector2(width ?? childSize.x, height ?? childSize.y));
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        WidgetTree.DrawChild(ctx, child, 0, rect);
    }
}
