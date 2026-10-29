using System;
using UnityEngine;

namespace UFeel
{
    public partial class UFeelAPI : MonoBehaviour
    {
        public static class SpeechToText
        {
            private static bool _speechToTextIsRunning;

            private static void ToggleSpeechDetection(bool status)
            {
                UFeelProcessor.SetSpeechToTextEnabled(status);
            }

            public static void StartDetection()
            {
                ToggleSpeechDetection(true);
                _speechToTextIsRunning = true;
                Debug.Log("Speech detection started.");
            }

            public static void StopDetection()
            {
                ToggleSpeechDetection(false);
                _speechToTextIsRunning = false;
                Debug.Log("Speech detection stopped.");
            }

            public static string? Current
            {
                get
                {
                    if (!_speechToTextIsRunning) return null;

                    return UFeelProcessor.GetSpeechToText();
                }
            }

            public static RuleKey TriggerActionOnSpeech(string text, Action action, bool onlyOnce)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));

                return RulesProcessor.AddRule(
                    condition: () =>
                    {
                        string? currentSpeechData = Current;
                        if (currentSpeechData == null) return false;

                        string targetToLower = text.ToUpperInvariant();
                        string? currentToLower = currentSpeechData?.ToUpperInvariant();
                        return targetToLower.Contains(currentToLower);
                    },
                    action: action,
                    onlyOnce: onlyOnce
                );
            }

            public static RuleKey TriggerActionOnSpeechOnce(string text, Action action)
            {
                return TriggerActionOnSpeech(text, action, true);
            }

            public static RuleKey TriggerActionOnSpeechContinuous(string text, Action action)
            {
                return TriggerActionOnSpeech(text, action, false);
            }
        }
    }
}