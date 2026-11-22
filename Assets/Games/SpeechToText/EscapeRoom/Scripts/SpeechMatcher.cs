using UnityEngine;

public readonly struct SpeechWordMatch
{
    public readonly bool Matched;
    public readonly float Score;
    public readonly string SpokenWord;
    public readonly string ExpectedWord;

    public SpeechWordMatch(
        bool matched,
        float score,
        string spokenWord,
        string expectedWord)
    {
        Matched = matched;
        Score = score;
        SpokenWord = spokenWord;
        ExpectedWord = expectedWord;
    }
}

public static class SpeechMatcher
{
    public static SpeechWordMatch Match(
        string spokenWord,
        string expectedWord,
        float threshold)
    {
        string spoken = SpeechNormalizer.NormalizeForComparison(spokenWord);
        string expected = SpeechNormalizer.NormalizeForComparison(expectedWord);

        if (string.IsNullOrEmpty(spoken) || string.IsNullOrEmpty(expected))
            return new SpeechWordMatch(false, 0f, spoken, expected);

        if (spoken == expected)
            return new SpeechWordMatch(true, 1f, spoken, expected);

        float score = CalculateSimilarity(spoken, expected);

        float effectiveThreshold = GetEffectiveThreshold(
            spoken,
            expected,
            threshold);

        return new SpeechWordMatch(
            score >= effectiveThreshold,
            score,
            spoken,
            expected);
    }

    private static float GetEffectiveThreshold(
        string spoken,
        string expected,
        float threshold)
    {
        int length = Mathf.Max(spoken.Length, expected.Length);

        if (length <= 3)
            return Mathf.Max(threshold, 0.88f);

        if (length <= 5)
            return Mathf.Max(threshold, 0.80f);

        return threshold;
    }

    private static float CalculateSimilarity(string first, string second)
    {
        int distance = DamerauLevenshteinDistance(first, second);
        int maxLength = Mathf.Max(first.Length, second.Length);

        if (maxLength == 0)
            return 1f;

        return 1f - ((float)distance / maxLength);
    }

    private static int DamerauLevenshteinDistance(string first, string second)
    {
        int[,] matrix = new int[first.Length + 1, second.Length + 1];

        for (int i = 0; i <= first.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= second.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= first.Length; i++)
        {
            for (int j = 1; j <= second.Length; j++)
            {
                int substitutionCost = first[i - 1] == second[j - 1] ? 0 : 1;

                matrix[i, j] = Mathf.Min(
                    matrix[i - 1, j] + 1,
                    matrix[i, j - 1] + 1,
                    matrix[i - 1, j - 1] + substitutionCost);

                if (i > 1 &&
                    j > 1 &&
                    first[i - 1] == second[j - 2] &&
                    first[i - 2] == second[j - 1])
                {
                    matrix[i, j] = Mathf.Min(
                        matrix[i, j],
                        matrix[i - 2, j - 2] + substitutionCost);
                }
            }
        }

        return matrix[first.Length, second.Length];
    }
}
