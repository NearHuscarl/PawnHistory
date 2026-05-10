using PawnHistory.Source.Helper;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class ScrollView(string key, Widget child, bool vertical = true) : Widget(key)
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
        var scrollPosition = ctx.GetScrollPosition(Key);
        var childConstraints = vertical
            ? LayoutConstraints.Loose(Mathf.Max(0f, rect.width - ctx.Theme.ScrollbarSize), float.PositiveInfinity)
            : LayoutConstraints.Loose(float.PositiveInfinity, rect.height);
        var childSize = child?.Measure(ctx, childConstraints) ?? Vector2.zero;
        var viewRect = vertical
            ? Rect.OfSize(Mathf.Max(rect.width - ctx.Theme.ScrollbarSize, childSize.x), childSize.y)
            : Rect.OfSize(childSize.x, Mathf.Max(rect.height, childSize.y));

        Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

        try
        {
            ctx.PushOffset(rect.position - scrollPosition);

            try
            {
                child?.Draw(ctx, new Rect(0f, 0f, viewRect.width, viewRect.height));
            }
            finally
            {
                ctx.PopOffset();
            }
        }
        finally
        {
            Widgets.EndScrollView();
        }

        ctx.SetScrollPosition(Key, scrollPosition);
    }
}
