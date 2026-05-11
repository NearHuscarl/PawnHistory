using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class GestureDetector(
    Widget child,
    Action onTap = null,
    Action onSecondaryTap = null,
    Action onHover = null,
    bool enabled = true,
    string key = null)
    : Widget(WidgetIds.GestureDetector, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        return child?.Measure(ctx, constraints) ?? Vector2.zero;
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        WidgetTree.DrawChild(ctx, child, 0, rect);

        if (!enabled || !Mouse.IsOver(rect))
            return;

        onHover?.Invoke();

        if (Event.current.type != EventType.MouseDown)
            return;

        switch (Event.current.button)
        {
            case 0:
                onTap?.Invoke();
                break;
            case 1:
                onSecondaryTap?.Invoke();
                break;
            default:
                return;
        }

        Event.current.Use();
    }
}
