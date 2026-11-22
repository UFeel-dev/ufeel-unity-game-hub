using UFeel;
using UnityEngine;

namespace GameHub.Maze
{
    public readonly struct EmotionInfo
    {
        public static readonly EmotionInfo[] All =
        {
            new(EmotionType.Happy, new Color(1f, 1f, 0f, 0.4f)),
            new(EmotionType.Surprised, new Color(1f, 0.5f, 0f, 0.4f)),
            new(EmotionType.Fearful, new Color(0f, 0f, 1f, 0.4f)),
        };

        public EmotionInfo(EmotionType emotion, Color color)
        {
            Emotion = emotion;
            Color = color;
        }

        public EmotionType Emotion { get; }

        public Color Color { get; }

        public static EmotionInfo Random()
        {
            return All[UnityEngine.Random.Range(0, All.Length)];
        }

        public static EmotionInfo RandomExcept(EmotionType excluded)
        {
            int excludedIndex = System.Array.FindIndex(All, info => info.Emotion == excluded);
            if (excludedIndex < 0)
                return Random();

            int index = UnityEngine.Random.Range(0, All.Length - 1);
            return All[index >= excludedIndex ? index + 1 : index];
        }
    }
}
