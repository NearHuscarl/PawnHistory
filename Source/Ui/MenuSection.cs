using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public sealed class MenuSection(Widget child, float padding = 0f, string key = null) : Widget(WidgetIds.MenuSection, key)
{
    private readonly Widget paddedChild = new Padding(child, new EdgeInsets(padding));

    protected override Vector2 DoMeasure(UiContext ctx, LayoutConstraints constraints)
    {
        return paddedChild.Measure(ctx, constraints);
    }

    public override void Draw(UiContext ctx, Rect rect)
    {
        Widgets.DrawMenuSection(rect);
        WidgetTree.DrawChild(ctx, child, 0, rect.ContractedBy(padding));
    }
}
