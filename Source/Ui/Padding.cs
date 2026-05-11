using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class Padding(Widget child, EdgeInsets insets, string key = null) : Widget(WidgetIds.Padding, key)
{
    public static Padding All(Widget child, float value) => new(child, new EdgeInsets(value));
    public static Padding Left(Widget child, float value) => new(child, EdgeInsets.Only(left: value));
    public static Padding Right(Widget child, float value) => new(child, EdgeInsets.Only(right: value));
    public static Padding Top(Widget child, float value) => new(child, EdgeInsets.Only(top: value));
    public static Padding Bottom(Widget child, float value) => new(child, EdgeInsets.Only(bottom: value));

    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        if (child == null)
            return constraints.Constrain(insets.Horizontal, insets.Vertical);

        var inner = child.Measure(ctx, constraints.Deflate(insets.Horizontal, insets.Vertical));
        return constraints.Constrain(inner.x + insets.Horizontal, inner.y + insets.Vertical);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        if (child == null)
            return;

        var innerRect = new Rect(
            rect.x + insets.Left,
            rect.y + insets.Top,
            Mathf.Max(0f, rect.width - insets.Horizontal),
            Mathf.Max(0f, rect.height - insets.Vertical));
        WidgetTree.DrawChild(ctx, child, 0, innerRect);
    }
}
