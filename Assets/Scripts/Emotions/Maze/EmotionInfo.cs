using UFeel;
using UnityEngine;

public readonly struct EmotionInfo
{
    public EmotionType Emotion { get; }
    public Color Color { get; }

    public EmotionInfo(EmotionType emotion, Color color)
    {
        Emotion = emotion;
        Color = color;
    }

    public static EmotionInfo GetRandomEmotion()
    {
        return Emotions[Random.Range(0, Emotions.Length)];
    }

    public static EmotionInfo GetRandomEmotionExcept(EmotionType excluded)
    {
        int excludedIndex = System.Array.FindIndex(Emotions, info => info.Emotion == excluded);
        if (excludedIndex < 0)
            return GetRandomEmotion();
        int index = Random.Range(0, Emotions.Length - 1);
        return Emotions[index >= excludedIndex ? index + 1 : index];
    }

    public static readonly EmotionInfo[] Emotions =
    {
        new(
            EmotionType.Happy,
            new Color(1f, 1f, 0f, 0.4f)
        ),
        new(
            EmotionType.Surprised,
            new Color(1f, 0.5f, 0f, 0.4f)
        ),
        new(
            EmotionType.Fearful,
            new Color(0f, 0f, 1f, 0.4f)
        )
    };
}
