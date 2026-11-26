using UnityEngine;

namespace GameHub.RhythmGame
{
    public static class Theme
    {
        public const float Display = 72f;
        public const float Title = 44f;
        public const float Heading = 32f;
        public const float Body = 26f;
        public const float Small = 21f;
        public const float Tiny = 17f;
        public const float Margin = 72f;

        public static readonly Color Backdrop = new(0.02f, 0.03f, 0.035f, 1f);
        public static readonly Color Text = new(0.93f, 0.95f, 0.94f, 1f);
        public static readonly Color TextDim = new(0.6f, 0.66f, 0.66f, 1f);
        public static readonly Color TextFaint = new(0.36f, 0.42f, 0.42f, 1f);
        public static readonly Color Line = new(1f, 1f, 1f, 0.09f);
        public static readonly Color Accent = new(0.33f, 0.93f, 0.58f, 1f);
        public static readonly Color Danger = new(1f, 0.33f, 0.38f, 1f);
        public static readonly Color Missed = new(0.45f, 0.5f, 0.5f, 0.35f);

        private static readonly Color Cyan = new(0.38f, 0.82f, 1f, 1f);

        private static readonly float[] LevelStops = { 1f, 4.5f, 8f, 12f, 16f };

        private static readonly Color[] LevelColors =
        {
            new(0.33f, 0.93f, 0.58f, 1f),
            new(0.98f, 0.86f, 0.35f, 1f),
            new(1f, 0.58f, 0.25f, 1f),
            new(1f, 0.3f, 0.38f, 1f),
            new(0.82f, 0.38f, 1f, 1f),
        };

        private static readonly Color[] JudgementColors =
        {
            new(0.62f, 0.95f, 1f, 1f),
            new(0.42f, 0.95f, 0.56f, 1f),
            new(0.98f, 0.85f, 0.35f, 1f),
            new(1f, 0.6f, 0.3f, 1f),
            new(1f, 0.33f, 0.38f, 1f),
        };

        private static readonly Color[] GradeColors =
        {
            new(1f, 0.92f, 0.55f, 1f),
            new(1f, 0.8f, 0.38f, 1f),
            new(0.42f, 0.95f, 0.56f, 1f),
            new(0.42f, 0.8f, 1f, 1f),
            new(0.75f, 0.62f, 1f, 1f),
            new(1f, 0.58f, 0.42f, 1f),
            new(1f, 0.33f, 0.38f, 1f),
        };

        private static readonly string[] JudgementLabels = { "Perfect", "Great", "Good", "Meh", "Miss" };

        public static Color Level(float level)
        {
            if (level <= LevelStops[0])
                return LevelColors[0];

            for (int i = 1; i < LevelStops.Length; i++)
            {
                if (level <= LevelStops[i])
                    return Color.Lerp(LevelColors[i - 1], LevelColors[i], (level - LevelStops[i - 1]) / (LevelStops[i] - LevelStops[i - 1]));
            }
            return LevelColors[^1];
        }

        public static Color Lane(int lane)
        {
            return lane is 0 or (LaneMask.LaneCount - 1) ? Accent : Cyan;
        }

        public static Color Judgement(Judgement judgement)
        {
            return JudgementColors[(int)judgement];
        }

        public static string JudgementLabel(Judgement judgement)
        {
            return JudgementLabels[(int)judgement];
        }

        public static Color Grade(Grade grade)
        {
            return GradeColors[(int)grade];
        }

        public static Color Fade(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static string Hex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
    }
}
