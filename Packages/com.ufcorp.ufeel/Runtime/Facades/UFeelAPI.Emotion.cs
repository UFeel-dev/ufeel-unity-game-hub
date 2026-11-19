#nullable enable

using System;

namespace UFeel
{
    public sealed partial class UFeelAPI
    {
        public static class Emotion
        {
            public static EmotionResult Current => Read(DetectionType.Emotions).Emotions;

            /// <summary>Highest-scoring emotion, or null without a valid face.</summary>
            public static EmotionType? Dominant => Current.Dominant;

            public static RuleKey TriggerActionOnEmotion(EmotionType emotion, Action action, bool onlyOnce)
            {
                return AddRule(() => Dominant == emotion, action, onlyOnce);
            }

            public static RuleKey TriggerActionOnEmotionOnce(EmotionType emotion, Action action)
            {
                return TriggerActionOnEmotion(emotion, action, true);
            }

            public static RuleKey TriggerActionOnEmotionContinuous(EmotionType emotion, Action action)
            {
                return TriggerActionOnEmotion(emotion, action, false);
            }
        }
    }
}
