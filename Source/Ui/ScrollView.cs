using PawnHistory.Source.Helper;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class ScrollView(Widget child, bool vertical = true, string key = null) : Widget(WidgetIds.ScrollView, key)
{
    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        var childSize = child?.Measure(ctx, constraints) ?? Vector2.zero;
        var width = constraints.HasBoundedWidth ? constraints.MaxWidth : childSize.x;
        var height = constraints.HasBoundedHeight ? constraints.MaxHeight : childSize.y;
        return constraints.Constrain(new Vector2(width, height));
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        var key = StateKey(ctx);
        var scrollPosition = ctx.GetScrollPosition(key);

        var childConstraints = vertical
            ? LayoutConstraints.Loose(Mathf.Max(0f, rect.width - ctx.Theme.ScrollbarSize), float.PositiveInfinity)
            : LayoutConstraints.Loose(float.PositiveInfinity, rect.height);
        var childSize = child?.Measure(ctx, childConstraints) ?? Vector2.zero;
        var viewRect = vertical
            ? Rect.OfSize(Mathf.Max(rect.width - ctx.Theme.ScrollbarSize, childSize.x), childSize.y)
            : Rect.OfSize(childSize.x, Mathf.Max(rect.height, childSize.y));

        Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

        ctx.PushOffset(rect.position - scrollPosition);
        WidgetTree.DrawChild(ctx, child, 0, Rect.OfSize(viewRect.width, viewRect.height));
        ctx.PopOffset();

        Widgets.EndScrollView();
        ctx.SetScrollPosition(key, scrollPosition);
    }
}
