using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class TextField(
    string value,
    Action<string> onChange,
    Action onSubmit = null,
    Action onCancel = null,
    Action onClickOutside = null,
    float? width = null,
    float? height = null,
    float minHeight = 32f,
    float? maxHeight = null,
    bool multiline = false,
    bool enabled = true,
    GameFont font = GameFont.Small,
    bool focusCursorToEnd = false,
    string key = null)
    : Widget(WidgetIds.TextField, key)
{
    private readonly string value = value ?? string.Empty;

    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        using (new TextStyleScope(font, TextAnchor.MiddleLeft))
        {
            var desiredWidth = width ?? (constraints.HasBoundedWidth ? constraints.MaxWidth : 200f);
            var desiredHeight = ResolveHeight(ctx, desiredWidth);

            return constraints.Constrain(desiredWidth, desiredHeight);
        }
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        var key = StateKey(ctx);
        var controlId = ctx.ControlId(key);

        HandleKeyboard(controlId);
        if (HandleClickOutside(controlId, rect))
            return;

        var wasEnabled = GUI.enabled;
        GUI.enabled = wasEnabled && enabled;

        string edited;
        using (new TextStyleScope(font, TextAnchor.MiddleLeft))
        {
            GUI.SetNextControlName(controlId);
            edited = multiline
                ? Widgets.TextArea(rect, value)
                : Widgets.TextField(rect, value);
        }
        GUI.enabled = wasEnabled;

        if (!string.Equals(edited, value, StringComparison.Ordinal))
            onChange?.Invoke(edited);

        if (ctx.ConsumeFocus(key))
        {
            UI.FocusControl(controlId, Find.WindowStack.currentlyDrawnWindow);
            if (focusCursorToEnd)
                MoveTextEnd();
        }
    }

    private float ResolveHeight(UiContext ctx, float desiredWidth)
    {
        var desiredHeight = height ?? (multiline
            ? Mathf.Max(Text.CurTextAreaStyle.CalcHeight(new GUIContent(value), Mathf.Max(1f, desiredWidth)), minHeight)
            : minHeight);

        if (maxHeight.HasValue)
            desiredHeight = Mathf.Min(desiredHeight, maxHeight.Value);

        return desiredHeight;
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

    private bool HandleClickOutside(string controlId, Rect rect)
    {
        var current = Event.current;
        if (onClickOutside == null
            || GUI.GetNameOfFocusedControl() != controlId
            || current.type != EventType.MouseDown
            || Mouse.IsOver(rect))
            return false;

        onClickOutside();
        UI.UnfocusCurrentControl();
        current.Use();
        return true;
    }

    private static void MoveTextEnd()
    {
        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
        editor.OnFocus();
        editor.MoveTextEnd();
    }
}
