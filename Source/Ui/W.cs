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
        StackCrossAxis crossAxis = StackCrossAxis.Center,
        float spacing = 0f,
        string key = null)
        => new(children, mainAxis, crossAxis, spacing, key);

    public static Row Row(
        IEnumerable<Widget> children,
        StackMainAxis mainAxis = StackMainAxis.Start,
        StackCrossAxis crossAxis = StackCrossAxis.Center,
        float spacing = 0f,
        string key = null)
        => new(children, mainAxis, crossAxis, spacing, key);

    public static Wrap Wrap(IEnumerable<Widget> children, float? gap = null, float? lineGap = null, string key = null) => new(children, gap, lineGap, key);
    public static Expanded Expanded(Widget child, int flex = 1, string key = null, bool debug = false) => new(child, flex, key, debug: debug);
    public static Flexible Flexible(Widget child, int flex = 1, FlexFit fit = FlexFit.Loose, string key = null, bool debug = false) => new(child, flex, fit, key, debug: debug);
    public static ScrollView ScrollView(Widget child, bool vertical = true, string key = null, ScrollController controller = null) => new(child, vertical, key, controller);
    public static SizedBox SizedBox(float? width = null, float? height = null, Widget child = null, string key = null, bool debug = false) => new(width, height, child, key, debug);
    public static SizedBox SizedBox(float dimension, Widget child = null, string key = null, bool debug = false) => new(dimension, dimension, child, key, debug);
    public static SizedBox SizedBoxShrink(string key = null) => Ui.SizedBox.Shrink(key);
    public static TextButton TextButton(string label, Action onClick, float? width = null, float? height = null, bool enabled = true, string key = null) => new(label, onClick, width, height, enabled, key);
    public static IconButton IconButton(Texture2D texture, Action onClick, float? iconSize = null, string tooltip = null, bool enabled = true, string key = null) => new(texture, onClick, iconSize, tooltip, enabled, key);
    public static Image Image(Texture2D texture, ScaleMode scaleMode = ScaleMode.ScaleToFit, Color? color = null, string key = null) => new(texture, scaleMode, color, key);
    public static Label Label(string text, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.MiddleLeft, float? width = null, float? height = null, Color? color = null, string key = null)
        => new(text, font, anchor, width, height, color, key);
    public static TextField TextField(
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
        => new(value, onChange, onSubmit, onCancel, onClickOutside, width, height, minHeight, maxHeight, multiline, enabled, font, focusCursorToEnd, key);

    public static LabeledField LabeledField(string label, Widget child, float labelWidth, float? gap = null, float? minHeight = null, string key = null)
        => new(label, child, labelWidth, gap, minHeight, key);
    public static MenuSection MenuSection(Widget child, float padding = 0f, string key = null) => new(child, padding, key);
    public static DecoratedBox DecoratedBox(BoxDecoration decoration, Widget child, string key = null) => new(decoration, child, key);
    public static ColoredBox ColoredBox(Color color, Widget child, string key = null) => new(color, child, key);
    public static CustomPaint CustomPaint(Action<Rect> painter, Widget child = null, Action<Rect> foregroundPainter = null, string key = null) => new(painter, child, foregroundPainter, key);
    public static Stack Stack(IEnumerable<Widget> children, string key = null) => new(children, key);
    public static ConstrainedBox ConstrainedBox(float? minWidth = null, float? maxWidth = null, float? minHeight = null, float? maxHeight = null, Widget child = null, string key = null)
        => new(minWidth, maxWidth, minHeight, maxHeight, child, key);
    public static Align Align(Widget child, Alignment alignment = default, string key = null) => new(child, alignment, key);
    public static Center Center(Widget child, string key = null) => new(child, key);
    public static GestureDetector GestureDetector(Widget child, Action onTap = null, Action onSecondaryTap = null, Action onHover = null, bool enabled = true, string key = null)
        => new(child, onTap, onSecondaryTap, onHover, enabled, key);
    public static Tooltip Tooltip(Widget child, string tip, string key = null) => new(child, tip, key);
    public static Positioned Positioned(Widget child, float? left = null, float? top = null, float? right = null, float? bottom = null, float? width = null, float? height = null, string key = null)
        => new(child, left, top, right, bottom, width, height, key);
    public static Spacer Spacer(int flex = 1, string key = null) => new(flex, key);

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

    public static ThingTile ThingTile(Thing thing, float? padding = null, string key = null) => new(thing, padding, key);
}
