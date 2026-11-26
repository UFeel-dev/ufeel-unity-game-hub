using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class HeartRateMonitor
    {
        private const float PollInterval = 0.25f;
        private const float SmoothingTime = 1.5f;
        private const float FallbackBpm = 60f;
        private const float PulseDecay = 7f;

        private float pollTimer;
        private float phase;

        public bool HasReading { get; private set; }

        public float SmoothedBpm { get; private set; }

        public float Pulse { get; private set; }

        public float Exertion(float restingBpm)
        {
            return HasReading ? DifficultySettings.Exertion(SmoothedBpm, restingBpm) : 0f;
        }

        public void Tick(float deltaTime)
        {
            pollTimer -= deltaTime;
            if (pollTimer <= 0f)
            {
                pollTimer = PollInterval;
                Poll();
            }

            float bpm = HasReading ? SmoothedBpm : FallbackBpm;
            phase += deltaTime * bpm / 60f;
            phase -= Mathf.Floor(phase);
            Pulse = Mathf.Exp(-phase * PulseDecay);
        }

        private void Poll()
        {
            if (!HeartRateTempo.TryGetBpm(out uint bpm))
            {
                HasReading = false;
                return;
            }

            if (!HasReading)
            {
                HasReading = true;
                SmoothedBpm = bpm;
                return;
            }

            float blend = 1f - Mathf.Exp(-PollInterval / SmoothingTime);
            SmoothedBpm += blend * (bpm - SmoothedBpm);
        }
    }
}
