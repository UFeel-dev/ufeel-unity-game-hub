#nullable enable

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UFeel
{
    public sealed partial class UFeelAPI
    {
        public static class EyeTracking
        {
            public static KeyCode CalibrationKey = KeyCode.F8;

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

            public static bool IsCalibrating => Overlay != null && Overlay.IsRunning;

            internal static bool Calibrated { get; set; }

            internal static float LastReadTime { get; private set; } = float.NegativeInfinity;

            private static UFeelCalibrationOverlay? Overlay => instance != null ? instance.calibrationOverlay : null;

            /// <summary>Saves the profile on success.</summary>
            /// <returns>A task that completes with the calibration result.</returns>
            /// <exception cref="InvalidOperationException">Thrown when UFeel is unavailable.</exception>
            public static Task<CalibrationResult> CalibrateAsync()
            {
                UFeelCalibrationOverlay? overlay = Overlay;
                if (overlay == null || session is null)
                    throw new InvalidOperationException("UFeel is unavailable.");

                return overlay.Run(session, profiles);
            }

            /// <summary>Logs failures instead of throwing.</summary>
            public static void Calibrate()
            {
                if (IsCalibrating)
                    return;

                try
                {
                    CalibrateAsync().ContinueWith(
                        task => Debug.LogException(task.Exception),
                        TaskContinuationOptions.OnlyOnFaulted);
                }
                catch (InvalidOperationException exception)
                {
                    Debug.LogWarning(exception.Message);
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
