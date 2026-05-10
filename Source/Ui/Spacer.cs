using UnityEngine;

namespace PawnHistory.Source.Ui;

public sealed class Spacer(float width = 0f, float height = 0f, string key = null) : Widget(key)
{
    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        return constraints.Constrain(new Vector2(width, height));
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
    }
}
