using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public static class RhythmTheme
    {
        public static readonly Color Missed = new(0.45f, 0.5f, 0.5f, 0.35f);

        private static readonly Color Cyan = new(0.38f, 0.82f, 1f, 1f);

        private static readonly float[] LevelStops = { 1f, 4.5f, 8f, 12f, 16f };

        private static readonly Color[] LevelColors =
        {
            Theme.Accent,
            Theme.Warning,
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
            Theme.Danger,
        };

        private static readonly Color[] GradeColors =
        {
            new(1f, 0.92f, 0.55f, 1f),
            new(1f, 0.8f, 0.38f, 1f),
            new(0.42f, 0.95f, 0.56f, 1f),
            new(0.42f, 0.8f, 1f, 1f),
            new(0.75f, 0.62f, 1f, 1f),
            new(1f, 0.58f, 0.42f, 1f),
            Theme.Danger,
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
            return lane is 0 or LaneMask.LaneCount - 1 ? Theme.Accent : Cyan;
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
    }
}
