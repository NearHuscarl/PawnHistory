using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.Helper;

internal static class LangUtility
{
    public static TaggedString FormatList(List<Pawn> pawns) => FormatList(pawns, p => p.NameShortColored.Resolve());

    public static TaggedString FormatList<T>(List<T> items, Func<T, string> toString = null, string otherText = null)
    {
        toString ??= o => o.ToString();
        otherText ??= "NH_PH_Other".Translate();

        int count = items.Count;
        if (count == 0) return string.Empty;

        var n1 = toString(items[0]);
        if (count == 1) return n1;

        var n2 = toString(items[1]);
        if (count == 2) return "NH_PH_List_Two".Translate(n1, n2);
        if (count == 3) return "NH_PH_List_Three".Translate(n1, n2, otherText);

        return "NH_PH_List_Many".Translate(n1, n2, count - 2, Find.ActiveLanguageWorker.Pluralize(otherText));
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0) return text;
        return text[..pos] + replace + text[(pos + search.Length)..];
    }

    // GrammarUtility is missing a method for plain text
    public static IEnumerable<Rule> RulesForString(string prefix, string text)
    {
        if (text == null)
        {
            Log.ErrorOnce($"Tried to insert rule {prefix} for null text", 464893221);
        }
        else
        {
            if (!prefix.NullOrEmpty())
                prefix += "_";
            
            yield return new Rule_String(prefix + "plural", Find.ActiveLanguageWorker.Pluralize(text));
            yield return new Rule_String(prefix + "pluralDef", Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(text)));
            yield return new Rule_String(prefix + "pluralIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(text)));
            yield return new Rule_String(prefix + "definite", Find.ActiveLanguageWorker.WithDefiniteArticle(text));
            yield return new Rule_String(prefix + "indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(text));
        }
    }

    public static string StripColorTags(string input)
    {
        return Regex.Replace(input, "<color=.*?>", string.Empty, RegexOptions.Compiled).Replace("</color>", string.Empty);
    }

    public static string StripTaggedContent(string s)
    {
        return Regex.Replace(s, "<[^>]+>[^<]*<\\/[^>]+>", string.Empty, RegexOptions.Compiled);
    }

    private static string Normalize(string s)
    {
        return new string([.. StripTaggedContent(s)
            .ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))]);
    }

    private static HashSet<string> Tokenize(string s)
    {
        return [.. Normalize(s).Split(' ', StringSplitOptions.RemoveEmptyEntries)];
    }

    public static float GetOverlapScore(string sentence1, string sentence2)
    {
        if (string.IsNullOrEmpty(sentence1) || string.IsNullOrEmpty(sentence2))
            return 0f;

        var setA = Tokenize(sentence1);
        var setB = Tokenize(sentence2);

        if (setA.Count == 0 || setB.Count == 0)
            return 0f;

        var common = setA.Intersect(setB).Count();
        var union = setA.Union(setB).Count();
        return (float)common / union;
    }
}
