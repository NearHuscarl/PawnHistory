namespace PawnHistory.Source.Ui;

public abstract class Widget(string key = null)
{
    public string Key { get; } = key;

    public abstract UnityEngine.Vector2 Measure(UiContext ctx, LayoutConstraints constraints);
    public abstract void Draw(UiContext ctx, UnityEngine.Rect rect);
}
