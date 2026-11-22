using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class SpeechNormalizer
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex NonLetterRegex = new(@"[^a-z0-9àâäæçéèêëîïôöœùûüÿñ'\s]", RegexOptions.Compiled);

    private static readonly string[] IgnoredWords =
    {
        "le",
        "la",
        "les",
        "l",
        "un",
        "une",
        "des",
        "du",
        "de",
        "d"
    };

    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string result = text.ToLowerInvariant().Trim();
        result = NonLetterRegex.Replace(result, " ");
        result = WhitespaceRegex.Replace(result, " ");

        return result.Trim();
    }

    public static string NormalizeForComparison(string text)
    {
        string normalized = Normalize(text);

        if (string.IsNullOrEmpty(normalized))
            return string.Empty;

        string decomposed = normalized.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new();

        foreach (char character in decomposed)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }

    public static string[] Tokenize(string text)
    {
        string normalized = NormalizeForComparison(text);

        if (string.IsNullOrEmpty(normalized))
            return Array.Empty<string>();

        string[] tokens = normalized.Split(' ');
        List<string> result = new();

        foreach (string token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token))
                continue;

            if (IsIgnoredWord(token))
                continue;

            result.Add(token);
        }

        return result.ToArray();
    }

    private static bool IsIgnoredWord(string word)
    {
        for (int i = 0; i < IgnoredWords.Length; i++)
        {
            if (IgnoredWords[i] == word)
                return true;
        }

        return false;
    }
}
