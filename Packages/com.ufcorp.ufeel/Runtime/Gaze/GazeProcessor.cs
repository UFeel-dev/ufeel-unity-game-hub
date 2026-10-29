using System;
using System.Diagnostics;
using UnityEngine;

namespace UFeel
{
    public partial class UFeelAPI : MonoBehaviour
    {
        public static class Gaze
        {
            private const double GazeSmoothingTimeConstant = 0.08;
            private const double GazeResetGapSeconds = 0.5;

            private static double? smoothedGazeX;
            private static double? smoothedGazeY;
            private static long? lastGazeTimestamp;

            private static GazeResultData SmoothGaze(GazeResultData gaze)
            {
                if (!gaze.IsValid)
                {
                    smoothedGazeX = null;
                    smoothedGazeY = null;
                    lastGazeTimestamp = null;
                    return gaze;
                }

                long timestamp = Stopwatch.GetTimestamp();

                double dt = lastGazeTimestamp is null
                    ? 0.0
                    : (timestamp - lastGazeTimestamp.Value) /
                        (double)Stopwatch.Frequency;

                lastGazeTimestamp = timestamp;

                if (smoothedGazeX is null ||
                    smoothedGazeY is null ||
                    dt <= 0.0 ||
                    dt > GazeResetGapSeconds)
                {
                    smoothedGazeX = gaze.EstimatedX;
                    smoothedGazeY = gaze.EstimatedY;
                    return gaze;
                }

                double alpha =
                    1.0 - Math.Exp(-dt / GazeSmoothingTimeConstant);

                smoothedGazeX += alpha * (gaze.EstimatedX - smoothedGazeX.Value);
                smoothedGazeY += alpha * (gaze.EstimatedY - smoothedGazeY.Value);

                return new GazeResultData(
                    gaze.Status,
                    (float)smoothedGazeX.Value,
                    (float)smoothedGazeY.Value,
                    gaze.RecordedX,
                    gaze.RecordedY,
                    gaze.Confidence,
                    gaze.Directions
                );
            }

            public static GazeResultData CurrentGaze
            {
                get => SmoothGaze(UFeelProcessor.GetGaze());
            }

            public static GazeDirection CurrentDirections
            {
                get => CurrentGaze.Directions;
            }

            public static Vector2 CurrentPositions
            {
                get => new(
                    CurrentGaze.EstimatedX,
                    CurrentGaze.EstimatedY
                );
            }

            public static bool IsBlinking
            {
                get => UFeelProcessor.GetGaze().Status == GazeStatus.Blinking;
            }

            public static RuleKey TriggerActionOnDirection(GazeDirection direction, Action action, bool onlyOnce)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));

                return RulesProcessor.AddRule(
                    condition: () => CurrentDirections == direction,
                    action: action,
                    onlyOnce: onlyOnce
                );
            }

            public static RuleKey TriggerActionOnDirectionOnce(GazeDirection direction, Action action)
            {
                return TriggerActionOnDirection(direction, action, true);
            }

            public static RuleKey TriggerActionOnDirectionContinuous(GazeDirection direction, Action action)
            {
                return TriggerActionOnDirection(direction, action, false);
            }
        }
    }
}
