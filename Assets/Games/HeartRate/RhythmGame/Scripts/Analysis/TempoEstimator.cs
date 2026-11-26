using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public static class TempoEstimator
    {
        private const double MinBpm = 70d;
        private const double MaxBpm = 200d;
        private const double BpmStep = 0.1;
        private const double PriorCenter = 130d;
        private const double PriorWidth = 0.6;
        private const double PhaseStep = 0.004;
        private const int Candidates = 5;
        private const int Harmonics = 4;

        public static GridFit Estimate(OnsetCurves curves)
        {
            float[] onsets = curves.Onsets;
            double hopDuration = curves.HopDuration;
            int maxLag = (int)Math.Ceiling(Harmonics * 60d / MinBpm / hopDuration) + 2;
            double[] correlation = Autocorrelation(onsets, maxLag);

            GridFit best = new(PriorCenter, 0d);
            double bestScore = double.MinValue;
            foreach (double bpm in Peaks(correlation, hopDuration))
            {
                GridFit fit = GridFitter.Fit(onsets, hopDuration, new GridFit(bpm, BestPhase(onsets, hopDuration, bpm)));
                double beats = onsets.Length * hopDuration * fit.Bpm / 60d;
                double score = GridFitter.Score(onsets, hopDuration, fit) / Math.Max(1d, beats) * Prior(fit.Bpm);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = fit;
                }
            }

            double beat = 60d / best.Bpm;
            double first = best.FirstBeatTime;
            if (BeatWeight(curves, best.Bpm, first + (beat / 2d)) > BeatWeight(curves, best.Bpm, first))
                first += beat / 2d;
            first %= beat;
            return new GridFit(best.Bpm, first < 0d ? first + beat : first);
        }

        private static double BeatWeight(OnsetCurves curves, double bpm, double firstBeat)
        {
            double beat = 60d / bpm;
            double length = curves.Low.Length * curves.HopDuration;
            double sum = 0d;
            for (double time = firstBeat; time < length; time += beat)
                sum += curves.Peak(curves.Low, time);
            return sum;
        }

        private static double Prior(double bpm)
        {
            double octaves = Math.Log(bpm / PriorCenter, 2d) / PriorWidth;
            return Math.Exp(-0.5 * octaves * octaves);
        }

        private static double[] Autocorrelation(float[] onsets, int maxLag)
        {
            double[] correlation = new double[maxLag + 1];
            for (int lag = 1; lag <= maxLag; lag++)
            {
                double sum = 0d;
                for (int i = 0; i + lag < onsets.Length; i++)
                    sum += onsets[i] * onsets[i + lag];
                correlation[lag] = sum;
            }
            return correlation;
        }

        private static double Interpolate(double[] values, double index)
        {
            int lower = (int)Math.Floor(index);
            if (lower < 0 || lower + 1 >= values.Length)
                return 0d;
            double blend = index - lower;
            return (values[lower] * (1d - blend)) + (values[lower + 1] * blend);
        }

        private static List<double> Peaks(double[] correlation, double hopDuration)
        {
            int count = (int)Math.Round((MaxBpm - MinBpm) / BpmStep) + 1;
            double[] scores = new double[count];
            for (int i = 0; i < count; i++)
            {
                double lag = 60d / (MinBpm + (i * BpmStep)) / hopDuration;
                double comb = 0d;
                for (int harmonic = 1; harmonic <= Harmonics; harmonic++)
                    comb += Interpolate(correlation, lag * harmonic) / harmonic;
                scores[i] = comb * Prior(MinBpm + (i * BpmStep));
            }

            List<int> peaks = new();
            for (int i = 1; i < count - 1; i++)
            {
                if (scores[i] >= scores[i - 1] && scores[i] > scores[i + 1])
                    peaks.Add(i);
            }
            peaks.Sort((a, b) => scores[b].CompareTo(scores[a]));

            List<double> result = new(Candidates);
            for (int i = 0; i < Math.Min(Candidates, peaks.Count); i++)
                result.Add(MinBpm + (peaks[i] * BpmStep));
            return result;
        }

        private static double BestPhase(float[] onsets, double hopDuration, double bpm)
        {
            double beat = 60d / bpm;
            double best = 0d;
            double bestScore = double.MinValue;
            for (double phase = 0d; phase < beat; phase += PhaseStep)
            {
                double score = GridFitter.Score(onsets, hopDuration, new GridFit(bpm, phase));
                if (score > bestScore)
                {
                    bestScore = score;
                    best = phase;
                }
            }
            return best;
        }
    }
}
