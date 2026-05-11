using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

internal static class W
{
    public static Column Column(
        IEnumerable<Widget> children,
        StackMainAxis mainAxis = StackMainAxis.Start,
        StackCrossAxis crossAxis = StackCrossAxis.Start,
        float? gap = null,
        string key = null)
        => new(children, mainAxis, crossAxis, gap, key);

    public static Row Row(
        IEnumerable<Widget> children,
        StackMainAxis mainAxis = StackMainAxis.Start,
        StackCrossAxis crossAxis = StackCrossAxis.Start,
        float? gap = null,
        string key = null)
        => new(children, mainAxis, crossAxis, gap, key);

    public static Wrap Wrap(IEnumerable<Widget> children, float? gap = null, float? lineGap = null, string key = null) => new(children, gap, lineGap, key);
    public static Expanded Expanded(Widget child, int flex = 1, string key = null) => new(child, flex, key);
    public static ScrollView ScrollView(Widget child, bool vertical = true, string key = null) => new(child, vertical, key);
    public static SizedBox SizedBox(float? width = null, float? height = null, Widget child = null, string key = null) => new(width, height, child, key);
    public static Button Button(string label, Action onClick, float? width = null, float? height = null, string key = null) => new(label, onClick, width, height, key);
    public static Label Label(string text, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.MiddleLeft, float? width = null, float? height = null, string key = null)
        => new(text, font, anchor, width, height, key);

    public static TextArea TextArea(
        string value,
        Action<string> onChange,
        Action onSubmit = null,
        Action onCancel = null,
        float? width = null,
        float minHeight = 32f,
        float? maxHeight = null,
        bool multiline = true,
        string key = null)
        => new(value, onChange, onSubmit, onCancel, width, minHeight, maxHeight, multiline, key);

    public static LabeledField LabeledField(string label, Widget child, float labelWidth, float? gap = null, float? minHeight = null, string key = null)
        => new(label, child, labelWidth, gap, minHeight, key);
    public static MenuSection MenuSection(Widget child, float padding = 0f, string key = null) => new(child, padding, key);

    public static ActionChip ActionChip(Thing thing, Action<Thing> onRemove) => new ActionChip(thing, onRemove);
    
    public static Autocomplete<T> Autocomplete<T>(
        AutocompleteController<T> controller,
        Func<string, IEnumerable<T>> findOptions,
        Action<T> onSelected,
        Action<Rect, T> drawOption,
        float? height = null,
        float popupRowHeight = 26f,
        int maxPopupRows = 6,
        string key = null)
        => new(controller, findOptions, onSelected, drawOption, height, popupRowHeight, maxPopupRows, key);
}
