#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
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

            private static int microphonesChanged;

            /// <summary>Raised on the main thread when a microphone was added or removed, or the system default changed.</summary>
            public static event Action? MicrophonesChanged;

            public static bool IsRunning => session?.IsSpeechToTextEnabled ?? false;

            /// <summary>Gets the current speech-to-text result.</summary>
            /// <remarks>Empty when silent, null while detection is stopped.</remarks>
            public static string? Current => IsRunning ? Snapshot.SpeechToText.Text : null;

            /// <summary>Gets the ID of the currently selected microphone.</summary>
            /// <summary>Null while the default microphone is used.</summary>
            public static string? SelectedMicrophoneId => session?.SelectedMicrophoneId;

            public static bool IsMicrophoneSelectorVisible => MicrophoneOverlay != null && MicrophoneOverlay.IsVisible;

            private static UFeelMicrophoneOverlay? MicrophoneOverlay =>
                instance != null ? instance.microphoneOverlay : null;

            public static void StartDetection() => SetEnabled(true);

            public static void StopDetection() => SetEnabled(false);

            /// <summary>Restarts a running detection to rescan the devices.</summary>
            /// <returns>True if the detection was restarted, false if it was not running.</returns>
            public static Task<IReadOnlyList<MicrophoneDevice>> GetMicrophonesAsync()
            {
                return session?.GetMicrophonesAsync() ??
                    Task.FromResult<IReadOnlyList<MicrophoneDevice>>(Array.Empty<MicrophoneDevice>());
            }

            /// <summary>
            /// Null selects the default microphone. On success, the choice is saved and applied at the next startups.
            /// Call it from the main thread.
            /// </summary>
            /// <param name="id">The microphone ID, or null for the default one.</param>
            /// <returns>True if the microphone was selected, false if it was unavailable.</returns
            public static async Task<bool> SelectMicrophoneAsync(string? id)
            {
                if (session is null || !await session.SelectMicrophoneAsync(id))
                    return false;

                if (string.IsNullOrEmpty(id))
                    PlayerPrefs.DeleteKey(MicrophonePreferenceKey);
                else
                    PlayerPrefs.SetString(MicrophonePreferenceKey, id);

                PlayerPrefs.Save();
                Debug.Log($"Speech microphone: {id ?? "default"}");
                return true;
            }

            /// <summary>Unlocks the cursor while visible.</summary>
            public static void ShowMicrophoneSelector() => MicrophoneOverlay?.Show();

            public static void HideMicrophoneSelector() => MicrophoneOverlay?.Hide();

            /// <summary>Case-insensitive "contains" match.</summary>
            /// <param name="text">The text to match.</param>
            /// <param name="action">The action to invoke when the match is found.</param>
            /// <param name="onlyOnce">True to invoke the action only once, false to invoke it every frame while the match is found.</param>
            /// <returns>A rule key that can be used to remove the rule.</returns>
            /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
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

            internal static void RestoreMicrophone(UFeelSession running)
            {
                string id = PlayerPrefs.GetString(MicrophonePreferenceKey, string.Empty);
                if (id.Length == 0)
                    return;

                running.SelectMicrophoneAsync(id).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        Debug.LogException(task.Exception);
                    else if (task.IsCompletedSuccessfully && !task.Result)
                        Debug.LogWarning($"Saved microphone \"{id}\" is unavailable, using the default one.");
                });
            }

            internal static void WatchMicrophones(UFeelSession running)
            {
                running.MicrophonesChanged += () => Interlocked.Exchange(ref microphonesChanged, 1);
            }

            internal static void RaiseMicrophonesChanged()
            {
                if (Interlocked.Exchange(ref microphonesChanged, 0) == 1)
                    MicrophonesChanged?.Invoke();
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
