using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class MenuSection(Widget child, float padding = 0f, string key = null) : Widget(key)
{
    private readonly Widget paddedChild = new Padding(child, new EdgeInsets(padding));
    
    public override Vector2 Measure(UiContext ctx, LayoutConstraints constraints)
    {
        return paddedChild.Measure(ctx, constraints);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        Widgets.DrawMenuSection(rect);
        child?.Draw(ctx, rect.ContractedBy(padding));
    }
}
