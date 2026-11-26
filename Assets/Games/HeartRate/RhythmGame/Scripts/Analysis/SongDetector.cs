using System;

namespace GameHub.RhythmGame
{
    public static class SongDetector
    {
        private const double PreviewLength = 20.0;
        private const double PreviewSkip = 0.1;

        public static SongDetection Detect(OnsetCurves curves, double length)
        {
            GridFit grid = TempoEstimator.Estimate(curves);
            return new SongDetection(grid.Bpm, grid.FirstBeatTime, PreviewStart(curves, length, grid));
        }

        private static double PreviewStart(OnsetCurves curves, double length, GridFit grid)
        {
            float[] loudness = curves.Loudness;
            int window = (int)(PreviewLength / curves.HopDuration);
            int first = (int)(length * PreviewSkip / curves.HopDuration);
            int last = loudness.Length - window;
            if (last <= first)
                return 0d;

            double sum = 0d;
            for (int i = first; i < first + window; i++)
                sum += loudness[i];

            double best = sum;
            int bestStart = first;
            for (int start = first + 1; start <= last; start++)
            {
                sum += loudness[start + window - 1] - loudness[start - 1];
                if (sum > best)
                {
                    best = sum;
                    bestStart = start;
                }
            }

            double measure = 60d / grid.Bpm * BeatGrid.BeatsPerMeasure;
            double time = bestStart * curves.HopDuration;
            return Math.Max(0d, grid.FirstBeatTime + (Math.Floor((time - grid.FirstBeatTime) / measure) * measure));
        }
    }
}
