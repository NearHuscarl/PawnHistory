using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class Image(Texture2D texture, ScaleMode scaleMode = ScaleMode.ScaleToFit, Color? color = null, string key = null)
    : Widget(WidgetIds.Image, key)
{
    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        var width = texture == null ? 0f : texture.width;
        var height = texture == null ? 0f : texture.height;
        return constraints.Constrain(width, height);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        if (texture == null)
            return;

        var oldColor = GUI.color;
        if (color.HasValue)
            GUI.color = color.Value;
        GUI.DrawTexture(rect, texture, scaleMode);
        GUI.color = oldColor;
    }
}
