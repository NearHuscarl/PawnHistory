using System;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class CustomPaint(
    Action<Rect> painter,
    Widget child = null,
    Action<Rect> foregroundPainter = null,
    string key = null)
    : Widget(WidgetIds.CustomPaint, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        return child?.Measure(ctx, constraints) ?? Vector2.zero;
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        painter?.Invoke(rect);
        WidgetTree.DrawChild(ctx, child, 0, rect);
        foregroundPainter?.Invoke(rect);
    }
}
