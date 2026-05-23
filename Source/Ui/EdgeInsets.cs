namespace PawnHistory.Source.Ui;

public readonly record struct EdgeInsets(float Left, float Top, float Right, float Bottom)
{
    public EdgeInsets(float all) : this(all, all, all, all)
    {
    }

    public static EdgeInsets Only(float? left = null, float? top = null, float? right = null, float? bottom = null) => new(
        left ?? 0,
        top ?? 0,
        right ?? 0,
        bottom ?? 0);

    public static EdgeInsets Symmetric(float? vertical = null, float? horizontal = null) => Only(horizontal, vertical, horizontal, vertical);
    
    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;
}
