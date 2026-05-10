using RimWorld;
using UnityEngine;

namespace PawnHistory.Source.Ui;

public abstract class WidgetTab(Theme theme = null) : ITab
{
    private readonly WidgetHost widgets = new(theme);
    protected virtual RootSizing RootSize => RootSizing.FillParent;
    protected virtual Rect RootRect => new(0f, 0f, size.x, size.y);

    protected abstract Widget Build(UiContext ctx);

    protected sealed override void FillTab()
    {
        widgets.Draw(RootRect, Build, RootSize);
    }
}