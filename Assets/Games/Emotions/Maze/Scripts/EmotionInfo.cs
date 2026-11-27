using System.Collections.Generic;
using UFeel;
using UnityEngine;

namespace GameHub.Maze
{
    public readonly struct EmotionInfo
    {
        public static readonly EmotionInfo[] All =
        {
            new(EmotionType.Happy, new Color(1f, 0.84f, 0.25f, 1f)),
            new(EmotionType.Surprised, new Color(1f, 0.45f, 0.75f, 1f)),
            new(EmotionType.Sad, new Color(0.25f, 0.55f, 1f, 1f)),
            new(EmotionType.Angry, new Color(0.95f, 0.25f, 0.2f, 1f)),
            new(EmotionType.Fearful, new Color(0.6f, 0.4f, 1f, 1f)),
            new(EmotionType.Disgusted, new Color(0.55f, 0.78f, 0.2f, 1f)),
            new(EmotionType.Contemptuous, new Color(0.9f, 0.55f, 0.3f, 1f)),
        };

        public EmotionInfo(EmotionType emotion, Color color)
        {
            Emotion = emotion;
            Color = color;
        }

        public EmotionType Emotion { get; }

        public Color Color { get; }

        public static bool TryGet(EmotionType emotion, out EmotionInfo info)
        {
            foreach (EmotionInfo candidate in All)
            {
                if (candidate.Emotion == emotion)
                {
                    info = candidate;
                    return true;
                }
            }

            info = default;
            return false;
        }

        public static EmotionInfo Random(IReadOnlyList<EmotionInfo> emotions)
        {
            return emotions[UnityEngine.Random.Range(0, emotions.Count)];
        }

        public static EmotionInfo RandomExcept(IReadOnlyList<EmotionInfo> emotions, EmotionType excluded)
        {
            int excludedIndex = -1;
            for (int i = 0; i < emotions.Count; i++)
            {
                if (emotions[i].Emotion == excluded)
                    excludedIndex = i;
            }

            if (excludedIndex < 0 || emotions.Count < 2)
                return Random(emotions);

            int index = UnityEngine.Random.Range(0, emotions.Count - 1);
            return emotions[index >= excludedIndex ? index + 1 : index];
        }
    }
}
