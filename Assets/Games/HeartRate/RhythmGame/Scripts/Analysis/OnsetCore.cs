using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class OnsetCore
    {
        private const double HopSeconds = 0.0116;
        private const double LowCutoff = 150d;
        private const double MidCutoff = 2000d;
        private const float LowWeight = 1f;
        private const float MidWeight = 0.8f;
        private const float HighWeight = 0.6f;
        private const float LogScale = 1000f;
        private const int FluxHistory = 3;
        private const double ThresholdWindow = 0.75;
        private const double IntensityWindow = 1.0;
        private const float OnsetPercentile = 0.98f;
        private const float IntensityPercentile = 0.95f;

        private readonly int sampleRate;
        private readonly int hopSize;
        private readonly double lowCoefficient;
        private readonly double midCoefficient;
        private readonly List<float> lowEnergy;
        private readonly List<float> midEnergy;
        private readonly List<float> highEnergy;
        private readonly List<float> fullEnergy;

        private double lowState;
        private double midState;
        private double lowSum;
        private double midSum;
        private double highSum;
        private double fullSum;
        private int hopFill;

        public OnsetCore(int sampleRate, int totalFrames)
        {
            this.sampleRate = sampleRate;
            hopSize = Math.Max(1, (int)Math.Round(sampleRate * HopSeconds));
            lowCoefficient = 1d - Math.Exp(-2d * Math.PI * LowCutoff / sampleRate);
            midCoefficient = 1d - Math.Exp(-2d * Math.PI * MidCutoff / sampleRate);

            int expectedHops = (totalFrames / hopSize) + 1;
            lowEnergy = new List<float>(expectedHops);
            midEnergy = new List<float>(expectedHops);
            highEnergy = new List<float>(expectedHops);
            fullEnergy = new List<float>(expectedHops);
        }

        private double HopDuration => hopSize / (double)sampleRate;

        public void Feed(float[] interleaved, int frames, int channels)
        {
            float channelScale = 1f / channels;
            for (int frame = 0; frame < frames; frame++)
            {
                float mono = 0f;
                for (int channel = 0; channel < channels; channel++)
                    mono += interleaved[(frame * channels) + channel];
                FeedSample(mono * channelScale);
            }
        }

        public SongAnalysis Finish(GridFit hint, double length)
        {
            return SongAnalyzer.Analyze(Curves(), hint, length);
        }

        public OnsetCurves Curves()
        {
            float[] low = Flux(lowEnergy);
            float[] mid = Flux(midEnergy);
            float[] high = Flux(highEnergy);
            return new OnsetCurves(
                HopDuration,
                OnsetCurve(low, mid, high, HighWeight),
                OnsetCurve(low, mid, high, 0f),
                Normalized(low),
                Normalized(mid),
                Normalized(high),
                LoudnessCurve());
        }

        private static float[] Flux(List<float> energy)
        {
            int count = energy.Count;
            float[] logEnergy = new float[count];
            for (int i = 0; i < count; i++)
                logEnergy[i] = (float)Math.Log10(1d + (LogScale * energy[i]));

            float[] flux = new float[count];
            for (int i = 1; i < count; i++)
            {
                int from = Math.Max(0, i - FluxHistory);
                float mean = 0f;
                for (int j = from; j < i; j++)
                    mean += logEnergy[j];
                mean /= i - from;
                flux[i] = Math.Max(0f, logEnergy[i] - mean);
            }
            return flux;
        }

        private static float[] MovingAverage(float[] values, int radius)
        {
            double[] prefix = new double[values.Length + 1];
            for (int i = 0; i < values.Length; i++)
                prefix[i + 1] = prefix[i] + values[i];

            float[] averaged = new float[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                int from = Math.Max(0, i - radius);
                int to = Math.Min(values.Length, i + radius + 1);
                averaged[i] = (float)((prefix[to] - prefix[from]) / (to - from));
            }
            return averaged;
        }

        private static void Normalize(float[] values, float percentile)
        {
            if (values.Length == 0)
                return;

            float[] sorted = (float[])values.Clone();
            Array.Sort(sorted);
            float reference = sorted[(int)((sorted.Length - 1) * percentile)];
            if (reference <= 0f)
                return;

            for (int i = 0; i < values.Length; i++)
                values[i] = Math.Min(1f, values[i] / reference);
        }

        private void FeedSample(float sample)
        {
            lowState += lowCoefficient * (sample - lowState);
            midState += midCoefficient * (sample - midState);
            double mid = midState - lowState;
            double high = sample - midState;

            lowSum += lowState * lowState;
            midSum += mid * mid;
            highSum += high * high;
            fullSum += sample * sample;

            if (++hopFill < hopSize)
                return;

            lowEnergy.Add((float)(lowSum / hopSize));
            midEnergy.Add((float)(midSum / hopSize));
            highEnergy.Add((float)(highSum / hopSize));
            fullEnergy.Add((float)(fullSum / hopSize));
            lowSum = 0d;
            midSum = 0d;
            highSum = 0d;
            fullSum = 0d;
            hopFill = 0;
        }

        private static float[] Normalized(float[] values)
        {
            float[] copy = (float[])values.Clone();
            Normalize(copy, OnsetPercentile);
            return copy;
        }

        private float[] OnsetCurve(float[] low, float[] mid, float[] high, float highWeight)
        {
            float[] combined = new float[low.Length];
            for (int i = 0; i < combined.Length; i++)
                combined[i] = (LowWeight * low[i]) + (MidWeight * mid[i]) + (highWeight * high[i]);

            float[] localMean = MovingAverage(combined, (int)Math.Round(ThresholdWindow / HopDuration));
            for (int i = 0; i < combined.Length; i++)
                combined[i] = Math.Max(0f, combined[i] - localMean[i]);

            Normalize(combined, OnsetPercentile);
            return combined;
        }

        private float[] LoudnessCurve()
        {
            float[] rms = new float[fullEnergy.Count];
            for (int i = 0; i < rms.Length; i++)
                rms[i] = (float)Math.Sqrt(fullEnergy[i]);

            float[] smoothed = MovingAverage(rms, (int)Math.Round(IntensityWindow / 2d / HopDuration));
            Normalize(smoothed, IntensityPercentile);
            return smoothed;
        }
    }
}
