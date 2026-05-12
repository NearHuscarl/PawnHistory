using UnityEngine;

namespace PawnHistory.Source.Ui;

public class Align(Widget child, Alignment alignment = default, string key = null, int widgetId = WidgetIds.Align)
    : Widget(widgetId, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        return constraints.Constrain(child?.Measure(ctx, LayoutConstraints.Loose(constraints.MaxWidth, constraints.MaxHeight)) ?? Vector2.zero);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        if (child == null)
            return;

        var childSize = child.Measure(ctx, LayoutConstraints.Loose(rect.size));
        var childRect = new Rect(
            rect.x + alignment.AlongX(rect.width, childSize.x),
            rect.y + alignment.AlongY(rect.height, childSize.y),
            Mathf.Min(rect.width, childSize.x),
            Mathf.Min(rect.height, childSize.y));
        WidgetTree.DrawChild(ctx, child, 0, childRect);
    }
}
