using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class Autocomplete<T>(
    AutocompleteController<T> controller,
    Func<string, IEnumerable<T>> findOptions,
    Action<T> onSelected,
    Action<Rect, T> drawOption,
    float? height = null,
    float popupRowHeight = 26f,
    int maxPopupRows = 6,
    string key = null)
    : Widget(WidgetIds.Autocomplete, key)
{
    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        return Input(ctx).Measure(ctx, constraints);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        HandleKeyboard(ctx);

        var rootRect = ctx.ToRoot(rect);
        if (HandleMouse(ctx, rootRect))
            return;

        Input(ctx).Draw(ctx, rect);

        var visibleOptions = controller.Options
            .Take(Mathf.Min(maxPopupRows, controller.Options.Count))
            .ToList();

        if (visibleOptions.Count > 0)
            ctx.AddOverlay(() => DrawPopup(ctx, rootRect, visibleOptions));
    }

    private TextArea Input(UiContext ctx)
    {
        var desiredHeight = height ?? ctx.Theme.TextFieldHeight;
        return W.TextArea(
            value: controller.Query,
            onChange: query => controller.SetQuery(query, findOptions(query)),
            onSubmit: () => Confirm(ctx),
            minHeight: desiredHeight,
            maxHeight: desiredHeight,
            multiline: false);
    }

    private void HandleKeyboard(UiContext ctx)
    {
        var current = Event.current;
        var controlId = ctx.ControlId(StateKey(ctx));

        if (GUI.GetNameOfFocusedControl() != controlId || current.type != EventType.KeyDown)
            return;

        var rowCount = VisibleRowCount();

        switch (current.keyCode)
        {
            case KeyCode.DownArrow:
                controller.MoveHighlight(1, rowCount);
                current.Use();
                break;

            case KeyCode.UpArrow:
                controller.MoveHighlight(-1, rowCount);
                current.Use();
                break;
        }
    }

    private bool HandleMouse(UiContext ctx, Rect fieldRect)
    {
        var rowCount = VisibleRowCount();
        if (rowCount == 0)
            return false;

        var current = Event.current;
        var mousePosition = ctx.ToRoot(current.mousePosition);
        var popupRect = PopupRect(ctx, fieldRect, rowCount);

        if (!popupRect.Contains(mousePosition))
            return false;

        var index = Mathf.Clamp(
            (int)((mousePosition.y - popupRect.y) / popupRowHeight),
            0,
            rowCount - 1);

        controller.Highlight(index, rowCount);

        switch (current.type)
        {
            case EventType.MouseDown when current.button == 0:
                Select(ctx, controller.Options[index]);
                current.Use();
                return true;

            case EventType.MouseUp:
            case EventType.MouseDrag:
            case EventType.ScrollWheel:
                current.Use();
                break;
        }

        return false;
    }

    private void Confirm(UiContext ctx)
    {
        if (controller.TryGetHighlighted(VisibleRowCount(), out var option))
            Select(ctx, option);
    }

    private void Select(UiContext ctx, T option)
    {
        onSelected?.Invoke(option);
        controller.Clear();
        ctx.RequestFocus(StateKey(ctx));
    }

    private int VisibleRowCount()
    {
        return Mathf.Min(maxPopupRows, controller.Options.Count);
    }

    private void DrawPopup(UiContext ctx, Rect fieldRect, IReadOnlyList<T> options)
    {
        if (options.Count == 0)
            return;

        var popupRect = PopupRect(ctx, fieldRect, options.Count);
        if (HandleMouse(ctx, fieldRect))
            return;

        Widgets.DrawMenuSection(popupRect);

        for (var i = 0; i < options.Count; i++)
        {
            var rowRect = new Rect(
                popupRect.x,
                popupRect.y + i * popupRowHeight,
                popupRect.width,
                popupRowHeight);

            if (i % 2 == 1)
                Widgets.DrawLightHighlight(rowRect);

            if (i == controller.HighlightedIndex || Mouse.IsOver(rowRect))
                Widgets.DrawHighlight(rowRect);

            drawOption(rowRect, options[i]);
        }
    }

    private Rect PopupRect(UiContext ctx, Rect fieldRect, int rowCount)
    {
        return new Rect(
            fieldRect.x,
            fieldRect.yMax + ctx.Theme.GapXs,
            fieldRect.width,
            rowCount * popupRowHeight);
    }
}

public sealed class AutocompleteController<T>
{
    public string Query { get; private set; } = string.Empty;
    public List<T> Options { get; } = [];
    public int HighlightedIndex { get; private set; } = -1;

    public void SetQuery(string query, IEnumerable<T> options)
    {
        Query = query ?? string.Empty;
        Options.Clear();
        HighlightedIndex = -1;

        if (!string.IsNullOrWhiteSpace(Query) && options != null)
            Options.AddRange(options.Where(option => option != null));
    }

    public void Clear()
    {
        Query = string.Empty;
        Options.Clear();
        HighlightedIndex = -1;
    }

    public void MoveHighlight(int delta, int visibleCount)
    {
        var count = VisibleCount(visibleCount);
        var direction = Math.Sign(delta);

        if (count == 0)
        {
            HighlightedIndex = -1;
            return;
        }

        HighlightedIndex = Normalize(HighlightedIndex + direction, count);
    }

    public void Highlight(int index, int visibleCount)
    {
        var count = VisibleCount(visibleCount);
        HighlightedIndex = count == 0 ? -1 : Normalize(index, count);
    }

    public bool TryGetHighlighted(int visibleCount, out T option)
    {
        var count = VisibleCount(visibleCount);

        if (count == 0)
        {
            option = default;
            return false;
        }

        option = Options[HighlightedIndex < 0 ? 0 : Normalize(HighlightedIndex, count)];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int VisibleCount(int visibleCount) => Math.Clamp(visibleCount, 0, Options.Count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Normalize(int value, int count) => (value % count + count) % count;
}
