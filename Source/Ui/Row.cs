using System.Collections.Generic;

namespace PawnHistory.Source.Ui;

public sealed class Row(
    IEnumerable<Widget> children,
    StackMainAxis mainAxis = StackMainAxis.Start,
    StackCrossAxis crossAxis = StackCrossAxis.Center,
    float spacing = 0,
    string key = null)
    : Flex(WidgetIds.Row, StackAxis.Horizontal, children, mainAxis, crossAxis, spacing, key);
