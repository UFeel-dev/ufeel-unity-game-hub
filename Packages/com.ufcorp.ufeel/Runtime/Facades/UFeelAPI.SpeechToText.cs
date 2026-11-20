#nullable enable

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UFeel
{
    public sealed partial class UFeelAPI
    {
        public static class SpeechToText
        {
            public static bool IsRunning => session?.IsSpeechToTextEnabled ?? false;

            /// <summary>Empty when silent, null while detection is stopped.</summary>
            public static string? Current => IsRunning ? Snapshot.SpeechToText.Text : null;

            public static void StartDetection() => SetEnabled(true);

            public static void StopDetection() => SetEnabled(false);

            /// <summary>Case-insensitive "contains" match.</summary>
            public static RuleKey TriggerActionOnSpeech(string text, Action action, bool onlyOnce)
            {
                if (string.IsNullOrWhiteSpace(text))
                    throw new ArgumentException("A non-empty text is required.", nameof(text));

                return AddRule(
                    () => Current?.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0,
                    action,
                    onlyOnce);
            }

            public static RuleKey TriggerActionOnSpeechOnce(string text, Action action)
            {
                return TriggerActionOnSpeech(text, action, true);
            }

            public static RuleKey TriggerActionOnSpeechContinuous(string text, Action action)
            {
                return TriggerActionOnSpeech(text, action, false);
            }

            private static void SetEnabled(bool enabled)
            {
                if (session is null)
                    return;

                session.SetSpeechToTextEnabledAsync(enabled).ContinueWith(
                    task => Debug.LogException(task.Exception),
                    TaskContinuationOptions.OnlyOnFaulted);

                Debug.Log(enabled ? "Speech detection started." : "Speech detection stopped.");
            }
        }
    }
}
