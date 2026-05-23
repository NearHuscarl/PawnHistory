using UnityEngine;
using Verse;

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

public class Flexible(Widget child, int flex = 1, FlexFit fit = FlexFit.Loose, string key = null, int widgetId = WidgetIds.Flexible, bool debug = false)
    : Widget(widgetId, key)
{
    public Widget Child { get; } = child;
    public int Flex { get; } = Mathf.Max(1, flex);
    public FlexFit Fit { get; } = fit;

    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        return Child?.Measure(ctx, constraints) ?? Vector2.zero;
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        WidgetTree.DrawChild(ctx, Child, 0, rect);

        if (debug)
            Widgets.DrawBox(rect);
    }
}
