using PawnHistory.Source.Helper;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class ScrollView(Widget child, bool vertical = true, string key = null, ScrollController controller = null) : Widget(WidgetIds.ScrollView, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        if (constraints is { HasBoundedWidth: true, HasBoundedHeight: true })
            return constraints.Constrain(constraints.MaxWidth, constraints.MaxHeight);

        var childSize = child?.Measure(ctx, ChildConstraints(ctx, constraints.MaxWidth, constraints.MaxHeight)) ?? Vector2.zero;
        var width = constraints.HasBoundedWidth
            ? constraints.MaxWidth
            : childSize.x + (vertical ? ctx.Theme.ScrollbarSize : 0f);
        var height = constraints.HasBoundedHeight
            ? constraints.MaxHeight
            : childSize.y + (vertical ? 0f : ctx.Theme.ScrollbarSize);

        return constraints.Constrain(width, height);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        var key = StateKey(ctx);
        var scrollPosition = ctx.GetScrollPosition(key);

        var childConstraints = ChildConstraints(ctx, rect.width, rect.height);
        var childSize = child?.Measure(ctx, childConstraints) ?? Vector2.zero;
        var viewRect = vertical
            ? Rect.OfSize(Mathf.Max(ContentWidth(ctx, rect.width), childSize.x), childSize.y)
            : Rect.OfSize(childSize.x, Mathf.Max(ContentHeight(ctx, rect.height), childSize.y));

        scrollPosition = controller?.Apply(scrollPosition, rect.size, viewRect.size, vertical) ?? scrollPosition;
        Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

        ctx.PushOffset(rect.position - scrollPosition);
        WidgetTree.DrawChild(ctx, child, 0, Rect.OfSize(viewRect.width, viewRect.height));
        ctx.PopOffset();

        Widgets.EndScrollView();
        ctx.SetScrollPosition(key, scrollPosition);
    }

    private LayoutConstraints ChildConstraints(UiContext ctx, float viewportWidth, float viewportHeight) =>
        vertical
            ? LayoutConstraints.Loose(ContentWidth(ctx, viewportWidth), float.PositiveInfinity)
            : LayoutConstraints.Loose(float.PositiveInfinity, ContentHeight(ctx, viewportHeight));

    private static float ContentWidth(UiContext ctx, float viewportWidth) =>
        Mathf.Clamp(viewportWidth - ctx.Theme.ScrollbarSize, 0f, viewportWidth);

    private static float ContentHeight(UiContext ctx, float viewportHeight) =>
        Mathf.Clamp(viewportHeight - ctx.Theme.ScrollbarSize, 0f, viewportHeight);
}