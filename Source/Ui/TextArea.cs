using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class TextArea(
    string value,
    Action<string> onChange,
    Action onSubmit = null,
    Action onCancel = null,
    float? width = null,
    float minHeight = 32f,
    float? maxHeight = null,
    bool multiline = true,
    string key = null)
    : Widget(WidgetIds.TextArea, key)
{
    private readonly string value = value ?? string.Empty;

    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        var desiredWidth = width ?? (constraints.HasBoundedWidth ? constraints.MaxWidth : 200f);
        var desiredHeight = multiline
            ? Mathf.Max(Text.CalcHeight(value, Mathf.Max(1f, desiredWidth)), minHeight)
            : minHeight;

        if (maxHeight.HasValue)
            desiredHeight = Mathf.Min(desiredHeight, maxHeight.Value);

        return constraints.Constrain(new Vector2(desiredWidth, desiredHeight));
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        var key = StateKey(ctx);
        var controlId = ctx.ControlId(key);

        HandleKeyboard(controlId);

        GUI.SetNextControlName(controlId);
        var edited = multiline
            ? Widgets.TextArea(rect, value)
            : Widgets.TextField(rect, value);

        if (!string.Equals(edited, value, StringComparison.Ordinal))
            onChange?.Invoke(edited);

        if (ctx.ConsumeFocus(key))
            UI.FocusControl(controlId, Find.WindowStack.currentlyDrawnWindow);
    }

    private void HandleKeyboard(string controlId)
    {
        var current = Event.current;
        if (GUI.GetNameOfFocusedControl() != controlId || current.type != EventType.KeyDown)
            return;

        if (current.keyCode == KeyCode.Escape && onCancel != null)
        {
            onCancel();
            UI.UnfocusCurrentControl();
            current.Use();
            return;
        }

        var isSubmitKey = current.keyCode is KeyCode.Return or KeyCode.KeypadEnter;
        if (!isSubmitKey || current.shift || onSubmit == null)
            return;

        onSubmit();
        current.Use();
    }
}
