using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public readonly record struct BoxDecoration(Color? Color = null, bool Border = false)
{
    public void PaintBackground(Rect rect)
    {
        if (Color.HasValue)
            Widgets.DrawBoxSolid(rect, Color.Value);
    }

    public void PaintForeground(Rect rect)
    {
        if (Border)
            Widgets.DrawBox(rect);
    }
}
