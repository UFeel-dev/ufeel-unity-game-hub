#nullable enable

using System;

namespace UFeel
{
    public sealed partial class UFeelAPI
    {
        public static class HeartRate
        {
            public static HeartRateResult Result => Read(DetectionType.HeartRate).HeartRate;

            /// <summary>Beats per minute, or 0 without a valid reading.</summary>
            public static uint Current
            {
                get
                {
                    HeartRateResult result = Result;
                    return result.IsValid ? result.Bpm : 0;
                }
            }

            public static RuleKey TriggerActionOnHeartRate(int rate, Action action, bool onlyOnce, int tolerance = 0)
            {
                return AddRule(
                    () =>
                    {
                        HeartRateResult result = Result;
                        return result.IsValid && Math.Abs(result.Bpm - rate) <= tolerance;
                    },
                    action,
                    onlyOnce);
            }

            public static RuleKey TriggerActionOnHeartRateOnce(int rate, Action action, int tolerance = 0)
            {
                return TriggerActionOnHeartRate(rate, action, true, tolerance);
            }

            public static RuleKey TriggerActionOnHeartRateContinuous(int rate, Action action, int tolerance = 0)
            {
                return TriggerActionOnHeartRate(rate, action, false, tolerance);
            }
        }
    }
}
