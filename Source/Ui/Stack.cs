using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class Stack(IEnumerable<Widget> children, string key = null) : Widget(WidgetIds.Stack, key)
{
    private readonly Widget[] children = children?.ToArray() ?? [];

    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        if (constraints is { HasBoundedWidth: true, HasBoundedHeight: true })
            return constraints.Constrain(constraints.MaxWidth, constraints.MaxHeight);

        var width = 0f;
        var height = 0f;
        foreach (var child in children)
        {
            var size = child.Measure(ctx, constraints);
            width = Mathf.Max(width, size.x);
            height = Mathf.Max(height, size.y);
        }

        return constraints.Constrain(width, height);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        for (var i = 0; i < children.Length; i++)
            WidgetTree.DrawChild(ctx, children[i], i, rect);
    }
}
