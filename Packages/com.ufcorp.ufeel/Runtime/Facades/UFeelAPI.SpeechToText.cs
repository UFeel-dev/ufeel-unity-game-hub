#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UFeel
{
    public sealed partial class UFeelAPI
    {
        public static class SpeechToText
        {
            public static KeyCode MicrophoneSelectorKey = KeyCode.F9;

            private const string MicrophonePreferenceKey = "UFeel.Microphone";

            public static bool IsRunning => session?.IsSpeechToTextEnabled ?? false;

            /// <summary>Empty when silent, null while detection is stopped.</summary>
            public static string? Current => IsRunning ? Snapshot.SpeechToText.Text : null;

            /// <summary>Null while the default microphone is used.</summary>
            public static string? SelectedMicrophone => session?.SelectedMicrophone;

            public static bool IsMicrophoneSelectorVisible => MicrophoneOverlay != null && MicrophoneOverlay.IsVisible;

            private static UFeelMicrophoneOverlay? MicrophoneOverlay =>
                instance != null ? instance.microphoneOverlay : null;

            public static void StartDetection() => SetEnabled(true);

            public static void StopDetection() => SetEnabled(false);

            /// <summary>The list refreshes only while detection is stopped.</summary>
            public static Task<IReadOnlyList<MicrophoneDevice>> GetMicrophonesAsync()
            {
                return session?.GetMicrophonesAsync() ??
                    Task.FromResult<IReadOnlyList<MicrophoneDevice>>(Array.Empty<MicrophoneDevice>());
            }

            /// <summary>
            /// Null selects the default microphone. On success, the choice is saved and applied at the next startups.
            /// Call it from the main thread.
            /// </summary>
            public static async Task<bool> SelectMicrophoneAsync(string? name)
            {
                if (session is null || !await session.SelectMicrophoneAsync(name))
                    return false;

                if (string.IsNullOrEmpty(name))
                    PlayerPrefs.DeleteKey(MicrophonePreferenceKey);
                else
                    PlayerPrefs.SetString(MicrophonePreferenceKey, name);

                PlayerPrefs.Save();
                Debug.Log($"Speech microphone: {name ?? "default"}");
                return true;
            }

            /// <summary>Unlocks the cursor while visible.</summary>
            public static void ShowMicrophoneSelector() => MicrophoneOverlay?.Show();

            public static void HideMicrophoneSelector() => MicrophoneOverlay?.Hide();

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

            /// <summary>A missing saved microphone is kept, the default one is used until it is back.</summary>
            internal static void RestoreMicrophone(UFeelSession running)
            {
                string name = PlayerPrefs.GetString(MicrophonePreferenceKey, string.Empty);
                if (name.Length == 0)
                    return;

                running.SelectMicrophoneAsync(name).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        Debug.LogException(task.Exception);
                    else if (task.IsCompletedSuccessfully && !task.Result)
                        Debug.LogWarning($"Saved microphone \"{name}\" is unavailable, using the default one.");
                });
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
