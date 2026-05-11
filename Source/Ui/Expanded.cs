namespace PawnHistory.Source.Ui;

public class Expanded(Widget child, int flex = 1, string key = null, int widgetId = WidgetIds.Expanded) : Flexible(child, flex, FlexFit.Tight, key, widgetId);
