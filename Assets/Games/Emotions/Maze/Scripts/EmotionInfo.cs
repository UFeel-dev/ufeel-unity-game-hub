using System;
using System.Collections.Generic;
using UFeel;
using UnityEngine;

namespace GameHub.Maze
{
    [Serializable]
    public struct EmotionInfo
    {
        [SerializeField] private EmotionType emotion;
        [SerializeField] private Color color;

        public EmotionInfo(EmotionType emotion, Color color)
        {
            this.emotion = emotion;
            this.color = color;
        }

        public readonly EmotionType Emotion => emotion;

        public readonly Color Color => color;

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
