using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class Padding : Widget
{
    private readonly Widget child;
    private readonly EdgeInsets insets;

    public Padding(Widget child, EdgeInsets insets, string key = null) : base(key)
    {
        this.child = child;
        this.insets = insets;
    }

    public static Padding Left(Widget child, float value) => new Padding(child, EdgeInsets.Only(left: value));
    public static Padding Right(Widget child, float value) => new Padding(child, EdgeInsets.Only(right: value));
    public static Padding Top(Widget child, float value) => new Padding(child, EdgeInsets.Only(top: value));
    public static Padding Bottom(Widget child, float value) => new Padding(child, EdgeInsets.Only(bottom: value));

    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        if (child == null)
            return constraints.Constrain(new Vector2(insets.Horizontal, insets.Vertical));

        var inner = child.Measure(ctx, constraints.Deflate(insets.Horizontal, insets.Vertical));
        return constraints.Constrain(new Vector2(inner.x + insets.Horizontal, inner.y + insets.Vertical));
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
        child.Draw(ctx, innerRect);
    }
}
