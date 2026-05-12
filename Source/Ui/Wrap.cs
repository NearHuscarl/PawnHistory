using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PawnHistory.Source.Helper;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class Wrap(IEnumerable<Widget> children, float? gap = null, float? lineGap = null, string key = null)
    : Widget(WidgetIds.Wrap, key)
{
    private readonly Widget[] children = children?.ToArray() ?? [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float Gap(UiContext ctx) => gap ?? ctx.Theme.GapXs;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float LineGap(UiContext ctx) => lineGap ?? ctx.Theme.GapXs;

    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        return MeasureLayout(ctx, constraints.MaxWidth, constraints).Size;
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        var layout = MeasureLayout(ctx, rect.width, LayoutConstraints.Loose(rect.size));

        foreach (var (index, child, childRect) in layout.Children)
        {
            WidgetTree.DrawChild(ctx, child, index, childRect.OffsetBy(rect.position));
        }
    }

    private WrapLayout MeasureLayout(UiContext ctx, float availableWidth, LayoutConstraints constraints)
    {
        var placed = new List<(int index, Widget widget, Rect rect)>(children.Length);
        var canWrap = !float.IsPositiveInfinity(availableWidth);

        var x = 0f;
        var y = 0f;
        var lineHeight = 0f;
        var maxWidth = 0f;
        var g = Gap(ctx);

        for (var i = 0; i < children.Length; i++)
        {
            var child = children[i];
            var childConstraints = LayoutConstraints.Loose(availableWidth, constraints.MaxHeight);
            var childSize = child.Measure(ctx, childConstraints);
            var needsWrap = canWrap && x > 0f && x + childSize.x > availableWidth;

            if (needsWrap)
            {
                maxWidth = Mathf.Max(maxWidth, x - g);
                x = 0f;
                y += lineHeight + LineGap(ctx);
                lineHeight = 0f;
            }

            placed.Add((i, child, new Rect(x, y, childSize.x, childSize.y)));
            x += childSize.x + g;
            lineHeight = Mathf.Max(lineHeight, childSize.y);
        }
        
        if (placed.Count > 0)
            maxWidth = Mathf.Max(maxWidth, x - g);
        
        var width = canWrap ? availableWidth : maxWidth;
        var height = placed.Count == 0 ? 0f : y + lineHeight;
        var size = constraints.Constrain(width, height);

        return new WrapLayout(size, placed);
    }

    private readonly record struct WrapLayout(Vector2 Size, List<(int index, Widget widget, Rect rect)> Children);
}
