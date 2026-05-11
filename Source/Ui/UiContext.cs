using System;
using System.Collections.Generic;
using PawnHistory.Source.Helper;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class UiContext(Theme theme = null)
{
    private readonly Dictionary<int, Vector2> scrollPositions = [];
    private readonly HashSet<int> pendingFocusKeys = [];
    private readonly List<Action> overlays = [];
    private readonly Stack<Vector2> offsets = new([Vector2.zero]);
    private readonly Stack<int> keyStack = [];
    private int currentKey = 1;

    public Theme Theme { get; } = theme ?? new Theme();
    public int CurrentKey => currentKey;

    public Vector2 GetScrollPosition(int key)
    {
        return scrollPositions.TryGetValue(key, out var position)
            ? position
            : Vector2.zero;
    }

    public void SetScrollPosition(int key, Vector2 position)
    {
        scrollPositions[key] = position;
    }

    public void RequestFocus(int key)
    {
        pendingFocusKeys.Add(key);
    }

    public bool ConsumeFocus(int key)
    {
        return pendingFocusKeys.Remove(key);
    }

    public void ResetKeyPath()
    {
        keyStack.Clear();
        currentKey = 1;
    }

    public void PushKey(int segment)
    {
        keyStack.Push(currentKey);
        currentKey = HashCode.Combine(currentKey, segment);
    }

    public void PopKey()
    {
        currentKey = keyStack.Pop();
    }

    public Rect ToRoot(Rect rect) => rect.OffsetBy(offsets.Peek());

    public Vector2 ToRoot(Vector2 position) => offsets.Peek() + position;

    public string ControlId(int key) => key.ToString();

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
