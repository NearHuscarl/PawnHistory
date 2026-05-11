namespace PawnHistory.Source.Ui;

public readonly record struct WidgetKey(int Value)
{
    public bool IsEmpty => Value == 0;

    public static WidgetKey Named(string value) => new(value.GetHashCode());
}
