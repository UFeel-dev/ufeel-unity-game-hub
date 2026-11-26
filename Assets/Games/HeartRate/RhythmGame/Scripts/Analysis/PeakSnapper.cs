using System;

namespace GameHub.RhythmGame
{
    public static class PeakSnapper
    {
        private const float PeakFloor = 0.06f;
        private const int PeakRadius = 2;
        private const double MaxDistance = 0.03;
        private const double SpacingShare = 0.4;
        private const float MidWeight = 0.8f;
        private const float HighWeight = 0.6f;

        private static readonly int[] Divisors = { 1, 2, 4, 3, 8, 6 };
        private static readonly double[] Penalties = { 0d, 0.002, 0.004, 0.006, 0.008, 0.009 };

        public static void Snap(OnsetCurves curves, double[] beats, SlotData slots)
        {
            float[] onsets = curves.Onsets;
            for (int i = 0; i < onsets.Length; i++)
            {
                if (!IsPeak(onsets, i))
                    continue;

                int slot = SlotFor(beats, curves.TimeAt(i));
                if (slot < 0 || slot >= slots.Count || onsets[i] <= slots.Strength[slot])
                    continue;

                slots.Strength[slot] = onsets[i];
                slots.Bands[slot] = BandAt(curves, i);
            }
        }

        private static bool IsPeak(float[] values, int index)
        {
            float value = values[index];
            if (value < PeakFloor)
                return false;

            for (int offset = 1; offset <= PeakRadius; offset++)
            {
                if (index - offset >= 0 && values[index - offset] >= value)
                    return false;
                if (index + offset < values.Length && values[index + offset] > value)
                    return false;
            }
            return true;
        }

        private static int SlotFor(double[] beats, double time)
        {
            int beat = Array.BinarySearch(beats, time);
            if (beat < 0)
                beat = ~beat - 1;
            if (beat < 0 || beat >= beats.Length - 1)
                return -1;

            double duration = beats[beat + 1] - beats[beat];
            double fraction = (time - beats[beat]) / duration;
            int best = -1;
            double bestCost = double.MaxValue;
            for (int i = 0; i < Divisors.Length; i++)
            {
                int divisor = Divisors[i];
                int step = (int)Math.Round(fraction * divisor);
                double distance = Math.Abs(fraction - (step / (double)divisor)) * duration;
                double cost = distance + Penalties[i];
                if (distance > Math.Min(MaxDistance, SpacingShare * duration / divisor) || cost >= bestCost)
                    continue;

                bestCost = cost;
                best = (beat * BeatGrid.SlotsPerBeat) + (step * (BeatGrid.SlotsPerBeat / divisor));
            }
            return best;
        }

        private static OnsetBand BandAt(OnsetCurves curves, int index)
        {
            float low = Around(curves.Low, index);
            float mid = MidWeight * Around(curves.Mid, index);
            float high = HighWeight * Around(curves.High, index);
            if (low >= mid && low >= high)
                return OnsetBand.Low;
            return mid >= high ? OnsetBand.Mid : OnsetBand.High;
        }

        private static float Around(float[] values, int index)
        {
            float max = 0f;
            for (int i = Math.Max(0, index - 1); i <= Math.Min(values.Length - 1, index + 1); i++)
                max = Math.Max(max, values[i]);
            return max;
        }
    }
}
