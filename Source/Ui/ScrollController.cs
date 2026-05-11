using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class ScrollController
{
    private bool pendingScrollToBottom;

    public void ScrollToBottom()
    {
        pendingScrollToBottom = true;
    }

    internal Vector2 Apply(Vector2 current, Vector2 viewport, Vector2 content, bool vertical)
    {
        current.x = vertical ? 0f : Mathf.Clamp(current.x, 0f, Mathf.Max(0f, content.x - viewport.x));

        if (pendingScrollToBottom)
        {
            current.y = vertical
                ? Mathf.Max(0f, content.y - viewport.y)
                : 0f;
            pendingScrollToBottom = false;
            return current;
        }

        current.y = vertical ? Mathf.Clamp(current.y, 0f, Mathf.Max(0f, content.y - viewport.y)) : 0f;
        return current;
    }
}
