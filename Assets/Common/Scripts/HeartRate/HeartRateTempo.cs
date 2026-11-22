using System;
using UFeel;

namespace GameHub.Common
{
    public static class HeartRateTempo
    {
        public const uint MinBpm = 60;
        public const uint MaxBpm = 180;

        public static bool TryGetBpm(out uint bpm)
        {
            uint current = UFeelAPI.HeartRate.Current;
            bpm = current == 0 ? 0 : Math.Clamp(current, MinBpm, MaxBpm);
            return current != 0;
        }
    }
}
