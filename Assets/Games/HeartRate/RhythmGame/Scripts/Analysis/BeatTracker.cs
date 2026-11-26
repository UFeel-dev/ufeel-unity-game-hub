using System;

namespace GameHub.RhythmGame
{
    public static class BeatTracker
    {
        private const int WindowBeats = 8;
        private const double InitialRange = 0.06;
        private const double StepRange = 0.025;
        private const double SearchStep = 0.002;
        private const double DriftPenalty = 8.0;
        private const float OffbeatWeight = 0.5f;

        public static double[] Track(float[] onsets, double hopDuration, GridFit grid, double length)
        {
            double beat = 60d / grid.Bpm;
            int beatCount = Math.Max(2, (int)((length - grid.FirstBeatTime) / beat) + 2);
            int windowCount = ((beatCount - 1) / WindowBeats) + 1;
            double[] shifts = new double[windowCount];

            double previous = 0d;
            for (int window = 0; window < windowCount; window++)
            {
                double range = window == 0 ? InitialRange : StepRange;
                shifts[window] = BestShift(onsets, hopDuration, grid, window * WindowBeats, previous, range);
                previous = shifts[window];
            }

            Smooth(shifts);
            double[] beats = new double[beatCount];
            for (int i = 0; i < beatCount; i++)
                beats[i] = grid.FirstBeatTime + (i * beat) + ShiftAt(shifts, i);
            return beats;
        }

        public static double[] Subdivide(double[] beats, int slotsPerBeat)
        {
            double[] slots = new double[(beats.Length - 1) * slotsPerBeat];
            for (int i = 0; i < beats.Length - 1; i++)
            {
                double step = (beats[i + 1] - beats[i]) / slotsPerBeat;
                for (int s = 0; s < slotsPerBeat; s++)
                    slots[(i * slotsPerBeat) + s] = beats[i] + (s * step);
            }
            return slots;
        }

        private static double BestShift(float[] onsets, double hopDuration, GridFit grid, int firstBeat, double center, double range)
        {
            double beat = 60d / grid.Bpm;
            double best = center;
            double bestScore = double.MinValue;
            int steps = (int)Math.Round(range / SearchStep);
            for (int step = -steps; step <= steps; step++)
            {
                double shift = center + (step * SearchStep);
                double score = -DriftPenalty * Math.Abs(shift - center);
                for (int i = firstBeat; i < firstBeat + WindowBeats; i++)
                {
                    double time = grid.FirstBeatTime + (i * beat) + shift;
                    score += Sample(onsets, hopDuration, time) + (OffbeatWeight * Sample(onsets, hopDuration, time + (beat / 2d)));
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    best = shift;
                }
            }
            return best;
        }

        private static float Sample(float[] onsets, double hopDuration, double time)
        {
            int index = (int)Math.Floor(time / hopDuration);
            if (index < 0 || index >= onsets.Length)
                return 0f;
            float next = index + 1 < onsets.Length ? onsets[index + 1] : 0f;
            float previous = index > 0 ? onsets[index - 1] : 0f;
            return Math.Max(onsets[index], Math.Max(previous, next));
        }

        private static void Smooth(double[] shifts)
        {
            if (shifts.Length < 3)
                return;

            double[] source = (double[])shifts.Clone();
            for (int i = 1; i < shifts.Length - 1; i++)
                shifts[i] = (source[i - 1] + (2d * source[i]) + source[i + 1]) / 4d;
        }

        private static double ShiftAt(double[] shifts, int beatIndex)
        {
            double position = ((beatIndex + 0.5) / WindowBeats) - 0.5;
            int lower = (int)Math.Floor(position);
            if (lower < 0)
                return shifts[0];
            if (lower >= shifts.Length - 1)
                return shifts[^1];

            double blend = position - lower;
            return shifts[lower] + ((shifts[lower + 1] - shifts[lower]) * blend);
        }
    }
}
