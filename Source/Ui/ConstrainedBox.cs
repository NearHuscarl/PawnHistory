using PawnHistory.Source.Helper;
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
        var childConstraints = Enforce(LayoutConstraints.Loose(rect.size));
        var childSize = childConstraints.Constrain(child?.Measure(ctx, childConstraints) ?? Vector2.zero);
        var childRect = Rect.OfSize(childSize).OffsetBy(rect.position);

        WidgetTree.DrawChild(ctx, child, 0, childRect);
    }

    private LayoutConstraints Enforce(LayoutConstraints parent)
    {
        var enforcedMaxWidth = parent.ConstrainWidth(maxWidth ?? parent.MaxWidth);
        var enforcedMaxHeight = parent.ConstrainHeight(maxHeight ?? parent.MaxHeight);
        var enforcedMinWidth = Mathf.Clamp(minWidth ?? parent.MinWidth, parent.MinWidth, enforcedMaxWidth);
        var enforcedMinHeight = Mathf.Clamp(minHeight ?? parent.MinHeight, parent.MinHeight, enforcedMaxHeight);

        return new LayoutConstraints(enforcedMinWidth, enforcedMaxWidth, enforcedMinHeight, enforcedMaxHeight);
    }
}
