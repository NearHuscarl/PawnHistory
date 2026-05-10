using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Ui;

internal readonly struct TextStyleScope : IDisposable
{
    private readonly GameFont previousFont;
    private readonly TextAnchor previousAnchor;

    public TextStyleScope(GameFont font, TextAnchor anchor)
    {
        previousFont = Text.Font;
        previousAnchor = Text.Anchor;
        Text.Font = font;
        Text.Anchor = anchor;
    }

    public void Dispose()
    {
        Text.Font = previousFont;
        Text.Anchor = previousAnchor;
    }
}
