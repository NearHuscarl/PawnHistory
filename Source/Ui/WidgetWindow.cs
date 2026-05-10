using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public abstract class WidgetWindow(Theme theme = null) : Window
{
    private readonly WidgetHost widgets = new(theme);
    protected virtual RootSizing RootSize => RootSizing.FillParent;

    protected abstract Widget Build(UiContext ctx);

    public sealed override void DoWindowContents(Rect inRect)
    {
        widgets.Draw(inRect, Build, RootSize);
    }
}