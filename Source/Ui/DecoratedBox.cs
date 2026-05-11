using UnityEngine;

namespace PawnHistory.Source.Ui;

public class DecoratedBox(BoxDecoration decoration, Widget child, string key = null, int widgetId = WidgetIds.DecoratedBox)
    : Widget(widgetId, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        return child?.Measure(ctx, constraints) ?? Vector2.zero;
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        decoration.PaintBackground(rect);
        WidgetTree.DrawChild(ctx, child, 0, rect);
        decoration.PaintForeground(rect);
    }
}
