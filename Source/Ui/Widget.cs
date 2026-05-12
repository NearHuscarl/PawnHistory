using System;
using System.Collections.Generic;
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

    
    private readonly Dictionary<MeasureCacheKey, Vector2> measureCache = [];
    private static int cacheHit = 0;
    private static int cacheMiss = 0;
    public Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        var cacheKey = new MeasureCacheKey(ctx.Theme, constraints);

        if (measureCache.TryGetValue(cacheKey, out var size))
        {
            cacheHit++;
            return size;
        }
        
        size = DoMeasure(ctx, constraints);
        measureCache[cacheKey] = size;
        cacheMiss++;
        return size;
    }

    protected abstract Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints);

    public abstract void Draw(UiContext ctx, Rect rect);
}

internal readonly record struct MeasureCacheKey(Theme Theme, float MinWidth, float MaxWidth, float MinHeight, float MaxHeight)
{
    public MeasureCacheKey(Theme theme, LayoutConstraints constraints) : this(
        theme,
        constraints.MinWidth,
        constraints.MaxWidth,
        constraints.MinHeight,
        constraints.MaxHeight)
    {
    }
}