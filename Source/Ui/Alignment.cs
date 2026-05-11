using UnityEngine;

namespace PawnHistory.Source.Ui;

public readonly record struct Alignment(float X, float Y)
{
    public static readonly Alignment TopLeft = new(-1f, -1f);
    public static readonly Alignment TopCenter = new(0f, -1f);
    public static readonly Alignment TopRight = new(1f, -1f);
    public static readonly Alignment CenterLeft = new(-1f, 0f);
    public static readonly Alignment Center = new(0f, 0f);
    public static readonly Alignment CenterRight = new(1f, 0f);
    public static readonly Alignment BottomLeft = new(-1f, 1f);
    public static readonly Alignment BottomCenter = new(0f, 1f);
    public static readonly Alignment BottomRight = new(1f, 1f);

    public float AlongX(float available, float size)
    {
        return Along(available, size, X);
    }

    public float AlongY(float available, float size)
    {
        return Along(available, size, Y);
    }

    private static float Along(float available, float size, float direction)
    {
        if (available <= size)
            return 0f;

        return (available - size) * Mathf.Clamp01((direction + 1f) / 2f);
    }
}
