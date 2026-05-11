using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class ConstrainedBox(
    Widget child,
    float? minWidth = null,
    float? maxWidth = null,
    float? minHeight = null,
    float? maxHeight = null,
    string key = null)
    : Widget(WidgetIds.ConstrainedBox, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        var childConstraints = Enforce(constraints);

        return childConstraints.Constrain(child?.Measure(ctx, childConstraints) ?? Vector2.zero);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        WidgetTree.DrawChild(ctx, child, 0, rect);
    }

    private LayoutConstraints Enforce(LayoutConstraints parent)
    {
        var enforcedMaxWidth = Mathf.Clamp(maxWidth ?? parent.MaxWidth, parent.MinWidth, parent.MaxWidth);
        var enforcedMaxHeight = Mathf.Clamp(maxHeight ?? parent.MaxHeight, parent.MinHeight, parent.MaxHeight);

        return new LayoutConstraints(
            Mathf.Clamp(minWidth ?? parent.MinWidth, parent.MinWidth, enforcedMaxWidth),
            enforcedMaxWidth,
            Mathf.Clamp(minHeight ?? parent.MinHeight, parent.MinHeight, enforcedMaxHeight),
            enforcedMaxHeight);
    }
}
