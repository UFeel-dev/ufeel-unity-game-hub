using System;

namespace GameHub.RhythmGame
{
    public static class GridFitter
    {
        private const double BpmRange = 0.5;
        private const double CoarseBpmStep = 0.02;
        private const double FineBpmStep = 0.002;
        private const double OffsetRange = 0.06;
        private const double OffsetStep = 0.002;
        private const float OffbeatWeight = 0.5f;

        public static GridFit Fit(float[] onsets, double hopDuration, GridFit hint)
        {
            float[] widened = Widen(onsets);
            GridFit coarse = Search(widened, hopDuration, hint, BpmRange, CoarseBpmStep);
            return Search(widened, hopDuration, coarse, CoarseBpmStep, FineBpmStep);
        }

        private static GridFit Search(float[] onsets, double hopDuration, GridFit center, double bpmRange, double bpmStep)
        {
            GridFit best = center;
            double bestScore = double.MinValue;
            int bpmSteps = (int)Math.Round(bpmRange / bpmStep);
            int offsetSteps = (int)Math.Round(OffsetRange / OffsetStep);
            for (int b = -bpmSteps; b <= bpmSteps; b++)
            {
                double bpm = center.Bpm + (b * bpmStep);
                for (int o = -offsetSteps; o <= offsetSteps; o++)
                {
                    GridFit candidate = new(bpm, center.FirstBeatTime + (o * OffsetStep));
                    double score = Score(onsets, hopDuration, candidate);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = candidate;
                    }
                }
            }
            return best;
        }

        public static double Score(float[] onsets, double hopDuration, GridFit grid)
        {
            double beat = 60d / grid.Bpm;
            double length = onsets.Length * hopDuration;
            double score = 0d;
            for (double time = grid.FirstBeatTime; time < length; time += beat)
            {
                score += Sample(onsets, hopDuration, time);
                score += OffbeatWeight * Sample(onsets, hopDuration, time + (beat / 2d));
            }
            return score;
        }

        private static float Sample(float[] onsets, double hopDuration, double time)
        {
            int index = (int)Math.Floor(time / hopDuration);
            return index >= 0 && index < onsets.Length ? onsets[index] : 0f;
        }

        private static float[] Widen(float[] onsets)
        {
            float[] widened = new float[onsets.Length];
            for (int i = 0; i < onsets.Length; i++)
            {
                float previous = i > 0 ? onsets[i - 1] : 0f;
                float next = i < onsets.Length - 1 ? onsets[i + 1] : 0f;
                widened[i] = Math.Max(onsets[i], Math.Max(previous, next));
            }
            return widened;
        }
    }
}
