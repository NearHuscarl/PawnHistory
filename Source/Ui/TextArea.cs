using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class TextArea : Widget
{
    private readonly string value;
    private readonly Action<string> onChange;
    private readonly Action onSubmit;
    private readonly Action onCancel;
    private readonly float? width;
    private readonly float minHeight;
    private readonly float? maxHeight;
    private readonly bool multiline;

    public TextArea(string key, string value, Action<string> onChange, Action onSubmit = null, Action onCancel = null, float? width = null, float minHeight = 32f, float? maxHeight = null, bool multiline = true) : base(key)
    {
        this.value = value ?? string.Empty;
        this.onChange = onChange;
        this.onSubmit = onSubmit;
        this.onCancel = onCancel;
        this.width = width;
        this.minHeight = minHeight;
        this.maxHeight = maxHeight;
        this.multiline = multiline;
    }

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
        HandleKeyboard();

        GUI.SetNextControlName(Key);
        var edited = multiline
            ? Widgets.TextArea(rect, value)
            : Widgets.TextField(rect, value);

        if (!string.Equals(edited, value, StringComparison.Ordinal))
            onChange?.Invoke(edited);

        if (ctx.ConsumeFocus(Key))
            UI.FocusControl(Key, Find.WindowStack.currentlyDrawnWindow);
    }

    private void HandleKeyboard()
    {
        var current = Event.current;
        if (GUI.GetNameOfFocusedControl() != Key || current.type != EventType.KeyDown)
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
