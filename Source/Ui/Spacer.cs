namespace PawnHistory.Source.Ui;

public sealed class Spacer(int flex = 1, string key = null) : Expanded(SizedBox.Shrink(), flex, key, WidgetIds.Spacer);
