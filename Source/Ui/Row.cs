using System.Collections.Generic;

namespace PawnHistory.Source.Ui;

public sealed class Row : Flex
{
    public Row(
        IEnumerable<Widget> children,
        float? gap = null,
        StackCrossAxis crossAxis = StackCrossAxis.Stretch,
        StackMainAxis mainAxis = StackMainAxis.Start,
        string key = null)
        : base(StackAxis.Horizontal, children, gap, crossAxis, mainAxis, key)
    {
    }
}