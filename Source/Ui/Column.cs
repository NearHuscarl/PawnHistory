using System.Collections.Generic;

namespace PawnHistory.Source.Ui;

public sealed class Column(
    IEnumerable<Widget> children,
    StackMainAxis mainAxis = StackMainAxis.Start,
    StackCrossAxis crossAxis = StackCrossAxis.Stretch,
    float? gap = null,
    string key = null)
    : Flex(WidgetIds.Column, StackAxis.Vertical, children, mainAxis, crossAxis, gap, key);
