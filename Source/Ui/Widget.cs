using System;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public abstract class Widget
{
    private readonly WidgetKey key;

    internal int WidgetId { get; }

    private Widget(int widgetId, WidgetKey key = default)
    {
        WidgetId = widgetId;
        this.key = key;
    }

    protected Widget(int widgetId, string key) : this(
        widgetId,
        key is null ? default : WidgetKey.Named(key))
    {
    }

    internal int SegmentKey(int index)
        => key.IsEmpty ? HashCode.Combine(WidgetId, index) : key.Value;

    protected int StateKey(UiContext ctx)
        => key.IsEmpty ? ctx.CurrentKey : key.Value;

    public abstract Vector2 Measure(UiContext ctx, LayoutConstraints constraints);
    public abstract void Draw(UiContext ctx, Rect rect);
}
