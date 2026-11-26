using UnityEngine;

namespace GameHub.RhythmGame
{
    public static class TempoSettings
    {
        public const float ChangePerSecond = 0.05f;

        private const int MinPercent = 50;
        private const int MaxPercent = 200;
        private const int StepPercent = 5;
        private const int DefaultPercent = 100;

        private const string RestKey = "GameHub.RhythmGame.SpeedAtRest";
        private const string PeakKey = "GameHub.RhythmGame.SpeedAtPeak";

        public static int RestPercent => Read(RestKey);

        public static int PeakPercent => Read(PeakKey);

        public static bool IsActive => RestPercent != DefaultPercent || PeakPercent != DefaultPercent;

        public static float Rate(float exertion)
        {
            return Mathf.Lerp(RestPercent, PeakPercent, exertion) / 100f;
        }

        public static int AdjustRest(int steps, bool fine)
        {
            return Adjust(RestKey, steps, fine);
        }

        public static int AdjustPeak(int steps, bool fine)
        {
            return Adjust(PeakKey, steps, fine);
        }

        private static int Read(string key)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(key, DefaultPercent), MinPercent, MaxPercent);
        }

        private static int Adjust(string key, int steps, bool fine)
        {
            int value = Mathf.Clamp(Read(key) + (steps * (fine ? 1 : StepPercent)), MinPercent, MaxPercent);
            PlayerPrefs.SetInt(key, value);
            return value;
        }
    }
}
