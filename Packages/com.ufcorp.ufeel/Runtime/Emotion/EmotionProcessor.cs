using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UFeel
{
    public partial class UFeelAPI : MonoBehaviour
    {
        public static class Emotion
        {
            public static Dictionary<string, float> Current
            {
                get => UFeelProcessor.GetEmotions();
            }

            public static EmotionType? Dominant
            {
                get
                {
                    Dictionary<string, float> emotions = Current;

                    if (emotions == null || emotions.Count == 0)
                        return null;

                    KeyValuePair<string, float> dominant = emotions
                        .OrderByDescending(e => e.Value)
                        .First();

                    // Debug.Log($"Dominant emotion: {dominant.Key}");

                    return dominant.Key.ToUpperInvariant() switch
                    {
                        "ANGRY" => EmotionType.Angry,
                        "CONTEMPTUOUS" => EmotionType.Contemptuous,
                        "DISGUSTED" => EmotionType.Disgusted,
                        "FEARFUL" => EmotionType.Fearful,
                        "HAPPY" => EmotionType.Happy,
                        "NEUTRAL" => EmotionType.Neutral,
                        "SAD" => EmotionType.Sad,
                        "SURPRISED" => EmotionType.Surprised,
                        _ => null,
                    };
                }
            }

            public static RuleKey TriggerActionOnEmotion(EmotionType emotion, Action action, bool onlyOnce)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));

                return RulesProcessor.AddRule(
                    condition: () => Dominant == emotion,
                    action: action,
                    onlyOnce: onlyOnce
                );
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