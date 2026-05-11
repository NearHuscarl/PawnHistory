using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public enum StackCrossAxis
{
    Start,
    Center,
    End,
    Stretch
}

public enum StackMainAxis
{
    Start,
    Center,
    End
}

public abstract class Flex(
    int widgetId,
    StackAxis axis,
    IEnumerable<Widget> children,
    StackMainAxis mainAxis,
    StackCrossAxis crossAxis,
    float? gap,
    string key = null)
    : Widget(widgetId, key)
{
    private readonly Widget[] children = children?.ToArray() ?? [];

    private float Gap(UiContext ctx) => gap ?? ctx.Theme.Gap;

    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        var sizes = MeasureChildren(ctx, constraints, out var main, out var cross);
        return constraints.Constrain(Size(main, cross));
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        var sizes = MeasureChildren(ctx, LayoutConstraints.Tight(rect.size), out var contentMain, out _);
        var cursor = MainStart(rect) + Align(Main(rect.size), contentMain, mainAxis);

        for (var i = 0; i < children.Length; i++)
        {
            var child = children[i];
            var size = sizes[i];
            var main = Main(size);
            var cross = crossAxis == StackCrossAxis.Stretch
                ? Cross(rect.size)
                : Mathf.Min(Cross(size), Cross(rect.size));

            var crossStart = CrossStart(rect) + Align(Cross(rect.size), cross, crossAxis);
            var childRect = axis == StackAxis.Horizontal
                ? new Rect(cursor, crossStart, main, cross)
                : new Rect(crossStart, cursor, cross, main);

            WidgetTree.DrawChild(ctx, child, i, childRect);
            cursor += main + Gap(ctx);
        }
    }

    private Vector2[] MeasureChildren(UiContext ctx, LayoutConstraints constraints, out float main, out float cross)
    {
        var sizes = new Vector2[children.Length];
        var bounded = IsBounded(constraints);
        var gapTotal = children.Length > 1 ? Gap(ctx) * (children.Length - 1) : 0f;
        var fixedMain = 0f;
        var totalFlex = 0;
        cross = 0f;

        for (var i = 0; i < children.Length; i++)
        {
            if (children[i] is Expanded expanded && bounded)
            {
                totalFlex += expanded.Flex;
                continue;
            }

            var child = children[i] is Expanded e ? e.Child : children[i];
            var maxMain = axis == StackAxis.Horizontal && bounded
                ? Mathf.Max(0f, MaxMain(constraints) - fixedMain - Gap(ctx) * i)
                : float.PositiveInfinity;

            sizes[i] = child.Measure(ctx, Loose(constraints, maxMain));
            fixedMain += Main(sizes[i]);
            cross = Mathf.Max(cross, Cross(sizes[i]));
        }

        if (totalFlex > 0)
        {
            var remaining = Mathf.Max(0f, MaxMain(constraints) - fixedMain - gapTotal);

            for (var i = 0; i < children.Length; i++)
            {
                if (children[i] is not Expanded expanded)
                    continue;

                var childMain = remaining * expanded.Flex / totalFlex;
                sizes[i] = WithMain(expanded.Child.Measure(ctx, TightMain(constraints, childMain)), childMain);
                cross = Mathf.Max(cross, Cross(sizes[i]));
            }
        }

        main = gapTotal + sizes.Sum(Main);
        return sizes;
    }

    private LayoutConstraints Loose(LayoutConstraints c, float maxMain) =>
        axis == StackAxis.Horizontal
            ? LayoutConstraints.Loose(maxMain, c.MaxHeight)
            : LayoutConstraints.Loose(c.MaxWidth, maxMain);

    private LayoutConstraints TightMain(LayoutConstraints c, float main) =>
        axis == StackAxis.Horizontal
            ? new LayoutConstraints(main, main, c.MinHeight, c.MaxHeight)
            : new LayoutConstraints(c.MinWidth, c.MaxWidth, main, main);

    private bool IsBounded(LayoutConstraints c) =>
        axis == StackAxis.Horizontal ? c.HasBoundedWidth : c.HasBoundedHeight;

    private float MaxMain(LayoutConstraints c) =>
        axis == StackAxis.Horizontal ? c.MaxWidth : c.MaxHeight;

    private Vector2 Size(float main, float cross) =>
        axis == StackAxis.Horizontal ? new Vector2(main, cross) : new Vector2(cross, main);

    private Vector2 WithMain(Vector2 size, float main) =>
        axis == StackAxis.Horizontal ? new Vector2(main, size.y) : new Vector2(size.x, main);

    private float Main(Vector2 size) => axis == StackAxis.Horizontal ? size.x : size.y;
    private float Cross(Vector2 size) => axis == StackAxis.Horizontal ? size.y : size.x;
    private float MainStart(Rect rect) => axis == StackAxis.Horizontal ? rect.x : rect.y;
    private float CrossStart(Rect rect) => axis == StackAxis.Horizontal ? rect.y : rect.x;

    private static float Align(float available, float size, StackMainAxis align)
    {
        if (available <= size)
            return 0f;

        return align switch
        {
            StackMainAxis.Center => (available - size) / 2f,
            StackMainAxis.End => available - size,
            _ => 0f
        };
    }

    private static float Align(float available, float size, StackCrossAxis align)
    {
        if (available <= size || align == StackCrossAxis.Stretch)
            return 0f;

        return align switch
        {
            StackCrossAxis.Center => (available - size) / 2f,
            StackCrossAxis.End => available - size,
            _ => 0f
        };
    }
}
