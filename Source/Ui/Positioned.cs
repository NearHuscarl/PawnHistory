using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class Positioned(
    Widget child,
    float? left = null,
    float? top = null,
    float? right = null,
    float? bottom = null,
    float? width = null,
    float? height = null,
    string key = null)
    : Widget(WidgetIds.Positioned, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        return Vector2.zero;
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        var childRect = Resolve(rect);
        WidgetTree.DrawChild(ctx, child, 0, childRect);
    }

    private Rect Resolve(Rect rect)
    {
        var x = rect.x + (left ?? 0f);
        var y = rect.y + (top ?? 0f);
        var resolvedWidth = width ?? Mathf.Max(0f, rect.width - (left ?? 0f) - (right ?? 0f));
        var resolvedHeight = height ?? Mathf.Max(0f, rect.height - (top ?? 0f) - (bottom ?? 0f));

        if (right.HasValue && !left.HasValue)
            x = rect.xMax - right.Value - resolvedWidth;
        if (bottom.HasValue && !top.HasValue)
            y = rect.yMax - bottom.Value - resolvedHeight;

        return new Rect(x, y, resolvedWidth, resolvedHeight);
    }
}
