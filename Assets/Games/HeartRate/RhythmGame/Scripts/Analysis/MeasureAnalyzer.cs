using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public static class MeasureAnalyzer
    {
        private const int SlotsPerBin = 3;
        private const int Bins = BeatGrid.SlotsPerMeasure / SlotsPerBin;
        private const int BandCount = 3;
        private const float HighBandWeight = 0.5f;
        private const float SimilarityThreshold = 0.82f;
        private const float IntensityTolerance = 0.15f;
        private const float SilentIntensity = 0.08f;
        private const float MusicalStrength = 0.15f;
        private const float FillCoverage = 0.35f;
        private const float TernaryRatio = 1.25f;
        private const float TernaryMinimum = 0.5f;

        private static readonly int[] BinaryFills = { 2, 4, 8 };
        private static readonly int[] TernaryFills = { 3, 6 };

        public static MeasureInfo[] Analyze(SlotData slots)
        {
            int count = (slots.Count + BeatGrid.SlotsPerMeasure - 1) / BeatGrid.SlotsPerMeasure;
            float[][] features = new float[count][];
            float[] intensity = new float[count];
            for (int measure = 0; measure < count; measure++)
            {
                features[measure] = Features(slots, measure);
                intensity[measure] = MeanIntensity(slots, measure);
            }

            MeasureInfo[] measures = new MeasureInfo[count];
            List<int> references = new();
            for (int measure = 0; measure < count; measure++)
            {
                int cluster = Assign(features, intensity, references, measure);
                int reference = cluster < 0 ? measure : references[cluster];
                bool ternary = IsTernary(slots, reference);
                measures[measure] = new MeasureInfo(cluster, reference, ternary, FillDivisor(slots, reference, ternary), intensity[measure]);
            }
            return measures;
        }

        private static int Assign(float[][] features, float[] intensity, List<int> references, int measure)
        {
            if (intensity[measure] < SilentIntensity || Norm(features[measure]) <= 0f)
                return -1;

            int best = -1;
            float bestSimilarity = SimilarityThreshold;
            for (int cluster = 0; cluster < references.Count; cluster++)
            {
                int reference = references[cluster];
                if (Math.Abs(intensity[reference] - intensity[measure]) > IntensityTolerance)
                    continue;

                float similarity = Cosine(features[reference], features[measure]);
                if (similarity >= bestSimilarity)
                {
                    best = cluster;
                    bestSimilarity = similarity;
                }
            }

            if (best >= 0)
                return best;

            references.Add(measure);
            return references.Count - 1;
        }

        private static float[] Features(SlotData slots, int measure)
        {
            float[] vector = new float[Bins * BandCount];
            int first = measure * BeatGrid.SlotsPerMeasure;
            for (int offset = 0; offset < BeatGrid.SlotsPerMeasure && first + offset < slots.Count; offset++)
            {
                int slot = first + offset;
                OnsetBand band = slots.Bands[slot];
                if (band == OnsetBand.None)
                    continue;

                float weight = band == OnsetBand.High ? HighBandWeight : 1f;
                int index = (((int)band - 1) * Bins) + (offset / SlotsPerBin);
                vector[index] = Math.Max(vector[index], slots.Strength[slot] * weight);
            }
            return vector;
        }

        private static float MeanIntensity(SlotData slots, int measure)
        {
            int first = measure * BeatGrid.SlotsPerMeasure;
            int last = Math.Min(slots.Count, first + BeatGrid.SlotsPerMeasure);
            float sum = 0f;
            for (int slot = first; slot < last; slot++)
                sum += slots.Intensity[slot];
            return last > first ? sum / (last - first) : 0f;
        }

        private static bool IsTernary(SlotData slots, int measure)
        {
            float ternary = 0f;
            float binary = 0f;
            int first = measure * BeatGrid.SlotsPerMeasure;
            for (int slot = first; slot < Math.Min(slots.Count, first + BeatGrid.SlotsPerMeasure); slot++)
            {
                int divisor = BeatGrid.DivisorOf(slot);
                if (BeatGrid.IsTernary(divisor))
                    ternary += slots.Strength[slot];
                else if (BeatGrid.IsBinary(divisor))
                    binary += slots.Strength[slot];
            }
            return ternary > TernaryMinimum && ternary > binary * TernaryRatio;
        }

        private static int FillDivisor(SlotData slots, int measure, bool ternary)
        {
            int fill = 1;
            int first = measure * BeatGrid.SlotsPerMeasure;
            foreach (int divisor in ternary ? TernaryFills : BinaryFills)
            {
                int positions = 0;
                int musical = 0;
                for (int slot = first; slot < Math.Min(slots.Count, first + BeatGrid.SlotsPerMeasure); slot++)
                {
                    if (BeatGrid.DivisorOf(slot) != divisor)
                        continue;

                    positions++;
                    if (slots.Strength[slot] >= MusicalStrength)
                        musical++;
                }

                if (positions > 0 && musical >= positions * FillCoverage)
                    fill = divisor;
            }
            return fill;
        }

        private static float Norm(float[] vector)
        {
            float sum = 0f;
            foreach (float value in vector)
                sum += value * value;
            return (float)Math.Sqrt(sum);
        }

        private static float Cosine(float[] a, float[] b)
        {
            float dot = 0f;
            for (int i = 0; i < a.Length; i++)
                dot += a[i] * b[i];
            float norm = Norm(a) * Norm(b);
            return norm > 0f ? dot / norm : 0f;
        }
    }
}
