#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UFeel
{
    public sealed partial class UFeelAPI
    {
        public static class EyeTracking
        {
            private const float InUseSeconds = 1.0F;

            private static Task<EyeTrackingCalibrationResult>? calibration;
            private static CancellationTokenSource? calibrationCancellation;

            public static EyeTrackingResult Current
            {
                get
                {
                    LastReadTime = Time.unscaledTime;
                    return Read(DetectionType.EyeTracking).EyeTracking;
                }
            }

            public static EyeTrackingDirection Directions => Current.Directions;

            public static Vector2 Gaze
            {
                get
                {
                    EyeTrackingResult current = Current;
                    return new Vector2(current.GazeX, current.GazeY);
                }
            }

            /// <summary>Gets the normalized screen position (0..1, origin top-left).</summary>
            public static Vector2 Position
            {
                get
                {
                    EyeTrackingResult current = Current;
                    return new Vector2(current.EstimatedX, current.EstimatedY);
                }
            }

            public static bool IsBlinking => Current.AreEyesClosed;

            public static uint BlinkCount => Current.BlinkCount;

            public static float HeadWeight
            {
                get => headWeight;
                set
                {
                    headWeight = Mathf.Clamp01(value);
                    session?.ExecuteAsync(processor => processor.SetEyeTrackingHeadWeight(headWeight)).ContinueWith(
                        task => Debug.LogException(task.Exception),
                        TaskContinuationOptions.OnlyOnFaulted);
                }
            }

            private static float headWeight;

            /// <summary>Gets a value indicating whether the eye tracking is calibrated.</summary>
            /// <remarks>Also true when a saved profile was loaded at startup.</remarks>
            public static bool IsCalibrated => Calibrated;

            public static bool IsCalibrating => calibration != null && !calibration.IsCompleted;

            /// <summary>Gets a value indicating whether the eye tracking was read during the last second.</summary>
            public static bool IsInUse => Time.unscaledTime - LastReadTime < InUseSeconds;

            internal static bool Calibrated { get; set; }

            internal static float LastReadTime { get; private set; } = float.NegativeInfinity;

            /// <summary>Saves the profile on success. Returns the running calibration if one is in progress.</summary>
            /// <exception cref="InvalidOperationException">Thrown when UFeel is unavailable.</exception>
            public static Task<EyeTrackingCalibrationResult> CalibrateAsync(IEyeTrackingCalibrationPresenter presenter)
            {
                if (calibration != null && !calibration.IsCompleted)
                    return calibration;

                if (session is null)
                    throw new InvalidOperationException("UFeel is unavailable.");

                calibrationCancellation = new CancellationTokenSource();
                calibration = RunCalibrationAsync(new EyeTrackingCalibration(session, presenter, eyeTrackingProfiles), calibrationCancellation.Token);
                return calibration;
            }

            public static void CancelCalibration() => calibrationCancellation?.Cancel();

            private static async Task<EyeTrackingCalibrationResult> RunCalibrationAsync(EyeTrackingCalibration run, CancellationToken token)
            {
                try
                {
                    EyeTrackingCalibrationResult outcome = await run.RunAsync(token);
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

            public static RuleKey TriggerActionOnDirection(EyeTrackingDirection direction, Action action, bool onlyOnce)
            {
                return AddRule(() => Directions.HasDirection(direction), action, onlyOnce);
            }

            public static RuleKey TriggerActionOnDirectionOnce(EyeTrackingDirection direction, Action action)
            {
                return TriggerActionOnDirection(direction, action, true);
            }

            public static RuleKey TriggerActionOnDirectionContinuous(EyeTrackingDirection direction, Action action)
            {
                return TriggerActionOnDirection(direction, action, false);
            }
        }
    }
}
