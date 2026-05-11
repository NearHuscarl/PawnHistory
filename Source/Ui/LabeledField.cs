using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class LabeledField(string label, Widget field, float labelWidth, float? gap = null, float? minHeight = null, string key = null)
    : Widget(WidgetIds.LabeledField, key)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float Gap(UiContext ctx) => gap ?? ctx.Theme.Gap;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float MinHeight(UiContext ctx) => minHeight ?? ctx.Theme.TextFieldHeight;
    
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        var g = Gap(ctx);
        var fieldWidth = Mathf.Max(0f, constraints.MaxWidth - labelWidth - g);
        var fieldSize = field.Measure(ctx, LayoutConstraints.Loose(fieldWidth, constraints.MaxHeight));
        var width = constraints.HasBoundedWidth
            ? constraints.MaxWidth
            : labelWidth + g + fieldSize.x;
        return constraints.Constrain(width, Mathf.Max(MinHeight(ctx), fieldSize.y));
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        var g = Gap(ctx);
        var labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
        var fieldRect = new Rect(rect.x + labelWidth + g, rect.y, Mathf.Max(0f, rect.width - labelWidth - g), rect.height);
        var previousFont = Text.Font;
        var previousAnchor = Text.Anchor;

        Text.Font = GameFont.Small;
        Text.Anchor = rect.height > MinHeight(ctx) ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, label);

        Text.Font = previousFont;
        Text.Anchor = previousAnchor;
        WidgetTree.DrawChild(ctx, field, 0, fieldRect);
    }
}
