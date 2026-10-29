using System;
using UnityEngine;

namespace UFeel
{
    public partial class UFeelAPI : MonoBehaviour
    {
        public static class HeartRate
        {
            // private static bool _heartRateSensorIsRunning;

            public static uint Current
            {
                get
                {
                    HeartRateResultData currentHeartRateSensorData = UFeelProcessor.GetHeartRate();
                    return currentHeartRateSensorData.IsValid ? currentHeartRateSensorData.Bpm : 0;
                }
            }

            public static void ToggleHeartRateSimulation(bool simulated)
            {
                // TODO: Add method to toggle simulation
            }

            public static RuleKey TriggerActionOnHeartRate(int rate, Action action, bool onlyOnce, int tolerance = 0)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));

                return RulesProcessor.AddRule(
                    condition: () =>
                    {
                        HeartRateResultData currentHeartRateSensorData = UFeelProcessor.GetHeartRate();

                        if (!currentHeartRateSensorData.IsValid)
                            return false;

                        uint current = currentHeartRateSensorData.Bpm;

                        int min = rate - tolerance;
                        int max = rate + tolerance;

                        return current >= min && current <= max;
                    },
                    action: action,
                    onlyOnce: onlyOnce
                );
            }

            public static RuleKey TriggerActionOnHeartRateOnce(int rate, Action action, int tolerance = 0)
            {
                return TriggerActionOnHeartRate(rate, action, true, tolerance);
            }

            public static RuleKey TriggerActionOnDirectionContinuous(int rate, Action action, int tolerance = 0)
            {
                return TriggerActionOnHeartRate(rate, action, false, tolerance);
            }
        }
    }
}