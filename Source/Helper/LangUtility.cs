using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.Helper;

internal static class LangUtility
{
    public static TaggedString FormatList(List<Pawn> pawns) => FormatList(pawns, p => p.NameDef);

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

        var n3 = toString(items[2]);
        if (count == 3) return "NH_PH_List_Three".Translate(n1, n2, n3);

        return "NH_PH_List_Many".Translate(n1, n2, count - 2, Find.ActiveLanguageWorker.Pluralize(otherText));
    }

    public static string ReplaceFirstMatch(this string text, string search, string replace, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        var pos = text.IndexOf(search, comparisonType);
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

            var rawText = StripColorTags(text);

            yield return new Rule_String(prefix + "plural", Find.ActiveLanguageWorker.Pluralize(text));
            yield return new Rule_String(prefix + "pluralDef", Find.ActiveLanguageWorker.WithDefiniteArticlePostProcessed(Find.ActiveLanguageWorker.Pluralize(rawText)).Replace(rawText, text));
            yield return new Rule_String(prefix + "pluralIndef", Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(Find.ActiveLanguageWorker.Pluralize(rawText)).Replace(rawText, text));
            yield return new Rule_String(prefix + "definite", Find.ActiveLanguageWorker.WithDefiniteArticlePostProcessed(rawText).Replace(rawText, text));
            yield return new Rule_String(prefix + "indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(rawText).Replace(rawText, text));
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

    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..maxLength] + "...";
    }

    public static bool IsStructurallyTheSame(string template, string actual, bool exactMatch = false)
    {
        if (string.IsNullOrEmpty(template) || string.IsNullOrEmpty(actual))
            return false;
        
        var segments = Regex.Matches(template, @"(?<rule>\[[^\]]+\])|(?<literal>[^\[]+)", RegexOptions.Compiled);
        var searchFrom = 0;
        var expectingContent = false;

        foreach (Match m in segments)
        {
            if (m.Groups["rule"].Success)
            {
                expectingContent = true;
                continue;
            }

            var literal = m.Groups["literal"].Value;
            var index = actual.IndexOf(literal, searchFrom, StringComparison.OrdinalIgnoreCase);

            // The text following a rule wasn't found
            if (index == -1)
                return false;

            // The placeholder was empty (literal found immediately at current position)
            if (expectingContent && index == searchFrom)
                return false;

            searchFrom = index + literal.Length;
            expectingContent = false;
        }

        if (exactMatch && searchFrom != actual.Length)
            return false;

        // Handle case where template ends with a placeholder [Rule]
        if (expectingContent && searchFrom >= actual.Length)
            return false;

        return true;
    }

    public static bool MatchesTranslationTemplate(this string text, string translationKey, bool exactMatch = false)
    {
        if (translationKey == null || !translationKey.CanTranslate() || string.IsNullOrEmpty(text))
        {
            return false;
        }

        var translation = translationKey.TranslateSimple();
        var segments = Regex.Matches(translation, @"(?<namedArg>{[^}]+})|(?<literal>[^{]+)", RegexOptions.Compiled);
        var searchFrom = 0;
        var expectingContent = false;

        foreach (Match m in segments)
        {
            if (m.Groups["namedArg"].Success)
            {
                expectingContent = true;
                continue;
            }

            var literal = m.Groups["literal"].Value;
            var index = text.IndexOf(literal, searchFrom, StringComparison.OrdinalIgnoreCase);

            // The text following a namedArg wasn't found
            if (index == -1)
                return false;

            // The placeholder was empty (literal found immediately at current position)
            if (expectingContent && index == searchFrom)
                return false;

            searchFrom = index + literal.Length;
            expectingContent = false;
        }

        if (exactMatch && searchFrom != text.Length)
            return false;

        // Handle case where template ends with a placeholder {namedArg}
        if (expectingContent && searchFrom >= text.Length)
            return false;

        return true;
    }
}
