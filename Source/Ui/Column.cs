using System.Collections.Generic;

namespace PawnHistory.Source.Ui;

public sealed class Column(
    IEnumerable<Widget> children,
    float? gap = null,
    StackCrossAxis crossAxis = StackCrossAxis.Stretch,
    StackMainAxis mainAxis = StackMainAxis.Start,
    string key = null)
    : Flex(StackAxis.Vertical, children, gap, crossAxis, mainAxis, key);