using System;
using System.Collections.Generic;
using PawnHistory.Source.Helper;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class UiContext(Theme theme = null)
{
    private readonly Dictionary<string, Vector2> scrollPositions = [];
    private readonly HashSet<string> pendingFocusKeys = [];
    private readonly List<Action> overlays = [];
    private readonly Stack<Vector2> offsets = new([Vector2.zero]);

    public Theme Theme { get; } = theme ?? new Theme();

    public Vector2 GetScrollPosition(string key)
    {
        return key != null && scrollPositions.TryGetValue(key, out var position)
            ? position
            : Vector2.zero;
    }

    public void SetScrollPosition(string key, Vector2 position)
    {
        if (key != null)
            scrollPositions[key] = position;
    }

    public void RequestFocus(string key)
    {
        if (!string.IsNullOrEmpty(key))
            pendingFocusKeys.Add(key);
    }

    public bool ConsumeFocus(string key)
    {
        return !string.IsNullOrEmpty(key) && pendingFocusKeys.Remove(key);
    }

    public Rect ToRoot(Rect rect) => rect.OffsetBy(offsets.Peek());

    public Vector2 ToRoot(Vector2 position) => offsets.Peek() + position;

    public void PushOffset(Vector2 offset)
    {
        offsets.Push(offsets.Peek() + offset);
    }

    public void PopOffset()
    {
        if (offsets.Count > 1)
            offsets.Pop();
    }

    public void AddOverlay(Action draw)
    {
        if (draw != null)
            overlays.Add(draw);
    }

    public void ClearOverlays()
    {
        overlays.Clear();
    }

    public void DrawOverlays()
    {
        foreach (var overlay in overlays)
            overlay();

        overlays.Clear();
    }
}