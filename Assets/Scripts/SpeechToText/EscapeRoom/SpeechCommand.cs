using System;
using UnityEngine;

[Serializable]
public class SpeechConcept
{
    [SerializeField] private string name;
    [SerializeField] private string[] words;
    [SerializeField, Range(0.5f, 1f)] private float fuzzyThreshold = 0.72f;
    [SerializeField, Range(0f, 10f)] private float weight = 1f;

    public string Name => name;
    public string[] Words => words;
    public float FuzzyThreshold => fuzzyThreshold;
    public float Weight => weight;

    public SpeechConcept(
        string name,
        string[] words,
        float fuzzyThreshold,
        float weight)
    {
        this.name = name;
        this.words = words;
        this.fuzzyThreshold = fuzzyThreshold;
        this.weight = weight;
    }
}

[Serializable]
public class SpeechCommand
{
    [SerializeField] private string id;
    [SerializeField] private SpeechConcept[] requiredConcepts;

    public string Id => id;
    public SpeechConcept[] RequiredConcepts => requiredConcepts;

    public SpeechCommand(
        string id,
        SpeechConcept[] requiredConcepts)
    {
        this.id = id;
        this.requiredConcepts = requiredConcepts;
    }
}
