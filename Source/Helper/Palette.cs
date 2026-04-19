using Verse;

namespace PawnHistory.Source.Helper;

public static class Palette
{
    public static string Red(object obj) => obj.ToString().ApplyTag(TagType.Red).Resolve();
    public static string Green(object obj) => obj.ToString().Colorize(ColoredText.FactionColor_Ally);
}