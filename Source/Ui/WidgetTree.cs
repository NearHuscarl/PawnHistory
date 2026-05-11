using UnityEngine;

namespace PawnHistory.Source.Ui;

internal static class WidgetTree
{
    public static void DrawChild(UiContext ctx, Widget child, int index, Rect rect)
    {
        if (child == null)
            return;

        ctx.PushKey(child.SegmentKey(index));

        try
        {
            child.Draw(ctx, rect);
        }
        finally
        {
            ctx.PopKey();
        }
    }
}
