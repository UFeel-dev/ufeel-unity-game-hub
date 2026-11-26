using System;

namespace GameHub.RhythmGame
{
    public static class DownbeatFinder
    {
        private const float HighWeight = 0.5f;

        public static double[] Align(double[] beats, OnsetCurves curves)
        {
            if (beats.Length < BeatGrid.BeatsPerMeasure * 2)
                return beats;

            float[] scores = new float[BeatGrid.BeatsPerMeasure];
            for (int beat = 0; beat < beats.Length; beat++)
            {
                double time = beats[beat];
                scores[beat % BeatGrid.BeatsPerMeasure] += curves.Peak(curves.Low, time) + (HighWeight * curves.Peak(curves.High, time)) + curves.Peak(curves.Onsets, time);
            }

            int phase = 0;
            for (int candidate = 1; candidate < scores.Length; candidate++)
            {
                if (scores[candidate] > scores[phase])
                    phase = candidate;
            }

            int prepend = (BeatGrid.BeatsPerMeasure - phase) % BeatGrid.BeatsPerMeasure;
            if (prepend == 0)
                return beats;

            double interval = beats[1] - beats[0];
            double[] aligned = new double[beats.Length + prepend];
            for (int i = 0; i < prepend; i++)
                aligned[i] = beats[0] - ((prepend - i) * interval);
            Array.Copy(beats, 0, aligned, prepend, beats.Length);
            return aligned;
        }
    }
}
