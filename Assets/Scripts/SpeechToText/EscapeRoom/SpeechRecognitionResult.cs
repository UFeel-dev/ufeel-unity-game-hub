using System.Collections.Generic;

public readonly struct SpeechConceptMatch
{
    public readonly string ConceptName;
    public readonly string SpokenWord;
    public readonly string ExpectedWord;
    public readonly float Score;

    public SpeechConceptMatch(
        string conceptName,
        string spokenWord,
        string expectedWord,
        float score)
    {
        ConceptName = conceptName;
        SpokenWord = spokenWord;
        ExpectedWord = expectedWord;
        Score = score;
    }
}

public sealed class SpeechRecognitionResult
{
    public bool IsValid { get; }
    public float Score { get; }
    public IReadOnlyList<SpeechConceptMatch> Matches { get; }

    public SpeechRecognitionResult(
        bool isValid,
        float score,
        IReadOnlyList<SpeechConceptMatch> matches)
    {
        IsValid = isValid;
        Score = score;
        Matches = matches;
    }
}
