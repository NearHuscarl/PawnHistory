using UnityEngine;

namespace PawnHistory.Source.Ui;

public enum StackAxis
{
    Horizontal,
    Vertical
}

public enum FlexFit
{
    Tight,
    Loose
}

public class Flexible(Widget child, int flex = 1, FlexFit fit = FlexFit.Loose, string key = null) : Widget(key)
{
    public Widget Child { get; } = child;
    public int Flex { get; } = Mathf.Max(1, flex);
    public FlexFit Fit { get; } = fit;

    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        return Child?.Measure(ctx, constraints) ?? Vector2.zero;
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        Child?.Draw(ctx, rect);
    }
}