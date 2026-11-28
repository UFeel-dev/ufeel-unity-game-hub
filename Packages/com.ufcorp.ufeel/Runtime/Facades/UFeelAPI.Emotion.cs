#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UFeel
{
    public sealed partial class UFeelAPI
    {
        public static class Emotion
        {
            private const float InUseSeconds = 1.0F;

            private static Task<EmotionCalibrationResult>? calibration;
            private static CancellationTokenSource? calibrationCancellation;

            public static EmotionResult Current
            {
                get
                {
                    LastReadTime = Time.unscaledTime;
                    return Read(DetectionType.Emotions).Emotions;
                }
            }

            /// <summary>Gets the highest-scoring emotion, or null without a valid face.</summary>
            public static EmotionType? Dominant => Current.Dominant;

            public static bool IsCalibrated => Calibrated;

            public static bool IsCalibrating => calibration != null && !calibration.IsCompleted;

            public static bool IsInUse => Time.unscaledTime - LastReadTime < InUseSeconds;

            internal static bool Calibrated { get; set; }

            internal static float LastReadTime { get; private set; } = float.NegativeInfinity;

            public static Task<EmotionCalibrationResult> CalibrateAsync(IEmotionCalibrationPresenter presenter)
            {
                if (calibration != null && !calibration.IsCompleted)
                    return calibration;

                if (session is null)
                    throw new InvalidOperationException("UFeel is unavailable.");

                calibrationCancellation = new CancellationTokenSource();
                calibration = RunCalibrationAsync(new EmotionCalibration(session, presenter, emotionProfiles), calibrationCancellation.Token);
                return calibration;
            }

            public static void CancelCalibration() => calibrationCancellation?.Cancel();

            public static async Task<bool> ResetCalibrationAsync()
            {
                if (session is null)
                    throw new InvalidOperationException("UFeel is unavailable.");

                await session.ExecuteAsync(processor => processor.ClearEmotionCalibration());
                Calibrated = false;
                return emotionProfiles?.Remove() ?? true;
            }

            private static async Task<EmotionCalibrationResult> RunCalibrationAsync(EmotionCalibration run, CancellationToken token)
            {
                try
                {
                    EmotionCalibrationResult outcome = await run.RunAsync(token);
                    if (outcome.Succeeded)
                        Calibrated = true;

                    Debug.Log(outcome.Message);
                    return outcome;
                }
                finally
                {
                    calibrationCancellation?.Dispose();
                    calibrationCancellation = null;
                }
            }

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
