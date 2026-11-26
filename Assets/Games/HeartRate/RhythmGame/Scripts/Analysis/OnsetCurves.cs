using System;

namespace GameHub.RhythmGame
{
    public sealed class OnsetCurves
    {
        public readonly double HopDuration;
        public readonly float[] Onsets;
        public readonly float[] Tonal;
        public readonly float[] Low;
        public readonly float[] Mid;
        public readonly float[] High;
        public readonly float[] Loudness;

        public OnsetCurves(double hopDuration, float[] onsets, float[] tonal, float[] low, float[] mid, float[] high, float[] loudness)
        {
            HopDuration = hopDuration;
            Onsets = onsets;
            Tonal = tonal;
            Low = low;
            Mid = mid;
            High = high;
            Loudness = loudness;
        }

        public double TimeAt(int index)
        {
            return (index + 0.5) * HopDuration;
        }

        public float Peak(float[] curve, double time)
        {
            int index = IndexAt(time);
            float max = 0f;
            for (int i = Math.Max(0, index - 1); i <= Math.Min(curve.Length - 1, index + 1); i++)
                max = Math.Max(max, curve[i]);
            return max;
        }

        public float Value(float[] curve, double time)
        {
            int index = IndexAt(time);
            return index >= 0 && index < curve.Length ? curve[index] : 0f;
        }

        private int IndexAt(double time)
        {
            return (int)Math.Floor(time / HopDuration);
        }
    }
}
