using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class FlatlineSequence
    {
        private const float Duration = 2.8f;
        private const float WindDown = 1.2f;
        private const float MinPitch = 0.35f;
        private const float FlashAlpha = 0.45f;
        private const float FlashSeconds = 0.9f;

        private readonly SessionContext context;

        private float elapsed;
        private float startVolume;
        private float startPitch;
        private float level;

        public FlatlineSequence(SessionContext context)
        {
            this.context = context;
        }

        public void Begin(float currentLevel)
        {
            elapsed = 0f;
            level = currentLevel;
            startVolume = context.Music.volume;
            startPitch = context.Music.pitch;
            context.Sounds.Flatline();
            context.Stage.Overlay.Flash(Theme.Fade(Theme.Danger, FlashAlpha), FlashSeconds);
            context.Stage.Overlay.SetHeadline("Flatline", Theme.Danger);
        }

        public bool Tick(float deltaTime)
        {
            elapsed += deltaTime;
            float windDown = Mathf.Clamp01(elapsed / WindDown);
            context.Music.pitch = Mathf.Lerp(startPitch, MinPitch, windDown);
            context.Music.volume = Mathf.Lerp(startVolume, 0f, windDown);
            HeartRateMonitor monitor = context.Monitor;
            context.Stage.Hud.TickHeart(deltaTime, monitor.SmoothedBpm, monitor.HasReading, 0f, level, 0f, true);
            return elapsed >= Duration;
        }

        public void End()
        {
            context.Music.Stop();
            context.Music.pitch = 1f;
            context.Stage.Overlay.SetHeadline(string.Empty, Color.clear);
        }
    }
}
