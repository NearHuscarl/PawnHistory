namespace PawnHistory.Source.Ui;

public sealed class Expanded(Widget child, int flex = 1, string key = null) : Flexible(child, flex, FlexFit.Tight, key, WidgetIds.Expanded);
