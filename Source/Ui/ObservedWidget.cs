using System;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class ObservedWidget(Widget child, Action<Rect> onDraw, string key = null) : Widget(key)
{
    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        return child?.Measure(ctx, constraints) ?? constraints.Constrain(Vector2.zero);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        onDraw?.Invoke(rect);
        child?.Draw(ctx, rect);
    }
}
