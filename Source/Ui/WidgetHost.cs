using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

public enum RootSizing
{
    FillParent,
    HugContent,
}

public sealed class WidgetHost(Theme theme = null)
{
    public UiContext Context { get; } = new(theme);
    public Theme Theme => Context.Theme;

    public void Draw(Rect rect, Func<UiContext, Widget> build, RootSizing sizing = RootSizing.FillParent)
    {
        var root = build?.Invoke(Context);
        if (root == null)
            return;

        DrawRoot(root, rect, sizing);
    }

    private void DrawRoot(Widget root, Rect rect, RootSizing sizing = RootSizing.FillParent)
    {
        var color = GUI.color;
        var font = Text.Font;
        var anchor = Text.Anchor;

        Context.ClearOverlays();

        try
        {
            if (sizing == RootSizing.HugContent)
                rect.size = root.Measure(Context, LayoutConstraints.Loose(rect.width, rect.height));

            root.Draw(Context, rect);
            Context.DrawOverlays();
        }
        finally
        {
            Context.ClearOverlays();

            GUI.color = color;
            Text.Font = font;
            Text.Anchor = anchor;
        }
    }
}