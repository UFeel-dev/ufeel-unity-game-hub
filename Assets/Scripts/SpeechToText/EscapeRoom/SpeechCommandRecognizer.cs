using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SpeechCommandRecognizer
{
    private readonly float recognitionWindowSeconds;
    private readonly float minimumOverallScore;

    private readonly Dictionary<string, float> matchedConceptScores = new();
    private readonly Dictionary<string, SpeechConceptMatch> matchedConcepts = new();

    private SpeechCommand currentCommand;
    private float lastSpeechChangeTime;
    private string lastSpeech = string.Empty;

    public SpeechCommandRecognizer(
        float recognitionWindowSeconds = 4f,
        float minimumOverallScore = 0.75f)
    {
        this.recognitionWindowSeconds = recognitionWindowSeconds;
        this.minimumOverallScore = minimumOverallScore;
    }

    public void SetCommand(SpeechCommand command)
    {
        currentCommand = command;
        Reset();
    }

    public void Reset()
    {
        matchedConceptScores.Clear();
        matchedConcepts.Clear();

        lastSpeech = string.Empty;
        lastSpeechChangeTime = 0f;
    }

    public SpeechRecognitionResult ProcessSpeech(string speech)
    {
        if (currentCommand == null)
            return CreateEmptyResult();

        if (string.IsNullOrWhiteSpace(speech))
            return CreateEmptyResult();

        if (!string.Equals(speech, lastSpeech, StringComparison.Ordinal))
        {
            lastSpeech = speech;
            lastSpeechChangeTime = Time.time;

            ProcessTokens(SpeechNormalizer.Tokenize(speech));
        }

        if (Time.time - lastSpeechChangeTime > recognitionWindowSeconds)
        {
            Reset();
            lastSpeech = speech;
            lastSpeechChangeTime = Time.time;

            ProcessTokens(SpeechNormalizer.Tokenize(speech));
        }

        return BuildResult();
    }

    private void ProcessTokens(string[] tokens)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            TryMatchToken(tokens[i]);
        }
    }

    private void TryMatchToken(string token)
    {
        SpeechConcept[] concepts = currentCommand.RequiredConcepts;

        for (int i = 0; i < concepts.Length; i++)
        {
            SpeechConcept concept = concepts[i];

            if (matchedConcepts.ContainsKey(concept.Name))
                continue;

            string[] expectedWords = concept.Words;

            SpeechWordMatch bestMatch = default;

            for (int j = 0; j < expectedWords.Length; j++)
            {
                SpeechWordMatch match = SpeechMatcher.Match(
                    token,
                    expectedWords[j],
                    concept.FuzzyThreshold);

                if (match.Score > bestMatch.Score)
                    bestMatch = match;
            }

            if (!bestMatch.Matched)
                continue;

            SpeechConceptMatch conceptMatch = new(
                concept.Name,
                bestMatch.SpokenWord,
                bestMatch.ExpectedWord,
                bestMatch.Score);

            matchedConcepts.Add(concept.Name, conceptMatch);
            matchedConceptScores.Add(concept.Name, bestMatch.Score);

            return;
        }
    }

    private SpeechRecognitionResult BuildResult()
    {
        if (currentCommand.RequiredConcepts == null ||
            currentCommand.RequiredConcepts.Length == 0)
        {
            return CreateEmptyResult();
        }

        float totalWeight = 0f;
        float matchedWeight = 0f;

        for (int i = 0; i < currentCommand.RequiredConcepts.Length; i++)
        {
            SpeechConcept concept = currentCommand.RequiredConcepts[i];

            totalWeight += concept.Weight;

            if (matchedConceptScores.TryGetValue(
                    concept.Name,
                    out float score))
            {
                matchedWeight += score * concept.Weight;
            }
        }

        float overallScore = totalWeight <= 0f
            ? 0f
            : matchedWeight / totalWeight;

        bool allConceptsMatched =
            matchedConcepts.Count == currentCommand.RequiredConcepts.Length;

        bool valid =
            allConceptsMatched &&
            overallScore >= minimumOverallScore;

        List<SpeechConceptMatch> matches =
            new(matchedConcepts.Values);

        return new SpeechRecognitionResult(
            valid,
            overallScore,
            matches);
    }

    private static SpeechRecognitionResult CreateEmptyResult()
    {
        return new SpeechRecognitionResult(
            false,
            0f,
            new List<SpeechConceptMatch>());
    }
}
