using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public readonly record struct LayoutConstraints(float MinWidth, float MaxWidth, float MinHeight, float MaxHeight)
{
    public bool HasBoundedWidth => !float.IsPositiveInfinity(MaxWidth);
    public bool HasBoundedHeight => !float.IsPositiveInfinity(MaxHeight);
    public bool HasInfiniteWidth => float.IsPositiveInfinity(MaxWidth);
    public bool HasInfiniteHeight => float.IsPositiveInfinity(MaxHeight);

    public static LayoutConstraints Tight(Vector2 size) => new(size.x, size.x, size.y, size.y);

    public static LayoutConstraints Loose(float maxWidth, float maxHeight) => new(0f, maxWidth, 0f, maxHeight);

    public LayoutConstraints CopyWith(float? minWidth = null, float? maxWidth = null, float? minHeight = null, float? maxHeight = null)
    {
        return new LayoutConstraints(
            minWidth ?? MinWidth,
            maxWidth ?? MaxWidth,
            minHeight ?? MinHeight,
            maxHeight ?? MaxHeight
        );
    }

    public Vector2 Constrain(Vector2 size) => new(ConstrainWidth(size.x), ConstrainHeight(size.y));
    public Vector2 Constrain(float width, float height) => new(ConstrainWidth(width), ConstrainHeight(height));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ConstrainWidth(float width) => Mathf.Clamp(width, MinWidth, MaxWidth);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ConstrainHeight(float height) => Mathf.Clamp(height, MinHeight, MaxHeight);
    
    public LayoutConstraints Deflate(float horizontal, float vertical)
    {
        return new LayoutConstraints(
            Mathf.Max(0f, MinWidth - horizontal),
            Mathf.Max(0f, MaxWidth - horizontal),
            Mathf.Max(0f, MinHeight - vertical),
            Mathf.Max(0f, MaxHeight - vertical)
        );
    }

    public bool Equals(LayoutConstraints other)
    {
        return Mathf.Approximately(MinWidth, other.MinWidth)
               && Mathf.Approximately(MaxWidth, other.MaxWidth)
               && Mathf.Approximately(MinHeight, other.MinHeight)
               && Mathf.Approximately(MaxHeight, other.MaxHeight);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MinWidth, MaxWidth, MinHeight, MaxHeight);
    }
}
