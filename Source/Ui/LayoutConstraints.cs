using UnityEngine;

namespace PawnHistory.Source.Ui;

public readonly record struct LayoutConstraints(float MinWidth, float MaxWidth, float MinHeight, float MaxHeight)
{
    public bool HasBoundedWidth => !float.IsPositiveInfinity(MaxWidth);
    public bool HasBoundedHeight => !float.IsPositiveInfinity(MaxHeight);
    
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

    public Vector2 Constrain(Vector2 size)
    {
        var width = Mathf.Clamp(size.x, MinWidth, MaxWidth);
        var height = Mathf.Clamp(size.y, MinHeight, MaxHeight);
        return new Vector2(width, height);
    }
    
    public float ConstrainWidth(float width) => Mathf.Clamp(width, MinWidth, MaxWidth);
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
}
