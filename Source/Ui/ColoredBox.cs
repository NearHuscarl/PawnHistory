using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class ColoredBox(Color color, Widget child, string key = null)
    : DecoratedBox(new BoxDecoration(Color: color), child, key, WidgetIds.ColoredBox);
