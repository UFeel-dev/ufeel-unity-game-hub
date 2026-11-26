using System;

namespace GameHub.RhythmGame
{
    public static class DifficultySettings
    {
        public const float MinLevel = 1f;
        public const float MaxLevel = 16f;

        public const float BaseMin = 1f;
        public const float BaseMax = 14f;
        public const float BaseDefault = 3f;

        public const float ScalingMin = 0f;
        public const float ScalingMax = 8f;
        public const float ScalingDefault = 3f;

        public const float SliderStep = 0.5f;
        public const float SliderFineStep = 0.1f;

        public const float HeartRateSpan = 50f;
        public const float DefaultRestingBpm = 70f;

        public const float LayerStep = 0.5f;
        public const float LayerHysteresis = 0.35f;

        public static int LayerCount => (int)Math.Round((MaxLevel - MinLevel) / LayerStep) + 1;

        public static float Exertion(float bpm, float restingBpm)
        {
            return Math.Clamp((bpm - restingBpm) / HeartRateSpan, 0f, 1f);
        }

        public static float Level(float baseLevel, float scaling, float exertion)
        {
            return Math.Clamp(baseLevel + (scaling * exertion), MinLevel, MaxLevel);
        }

        public static float ScoreMultiplier(float level)
        {
            return 1f + (0.1f * (level - MinLevel));
        }

        public static float LayerLevel(int layer)
        {
            return MinLevel + (layer * LayerStep);
        }

        public static int NearestLayer(float level)
        {
            return Math.Clamp((int)Math.Round((level - MinLevel) / LayerStep), 0, LayerCount - 1);
        }
    }
}
