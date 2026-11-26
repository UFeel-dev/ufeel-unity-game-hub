using System;

namespace GameHub.RhythmGame
{
    public static class SongAnalyzer
    {
        private const float SustainOnsetCeiling = 0.3f;
        private const float SustainLoudnessRatio = 0.6f;
        private const int MaxSustainSlots = BeatGrid.SlotsPerBeat * 4;

        public static SongAnalysis Analyze(OnsetCurves curves, GridFit hint, double length)
        {
            GridFit grid = GridFitter.Fit(curves.Onsets, curves.HopDuration, hint);
            double[] beats = DownbeatFinder.Align(BeatTracker.Track(curves.Onsets, curves.HopDuration, grid, length), curves);
            double[] slotTimes = Trim(BeatTracker.Subdivide(beats, BeatGrid.SlotsPerBeat), length);

            SlotData slots = new(slotTimes.Length);
            PeakSnapper.Snap(curves, beats, slots);
            float[] tonal = new float[slots.Count];
            for (int slot = 0; slot < slots.Count; slot++)
            {
                double time = Math.Max(0d, slotTimes[slot]);
                slots.Intensity[slot] = curves.Value(curves.Loudness, time);
                tonal[slot] = curves.Peak(curves.Tonal, time);
            }
            FillSustain(slots, tonal);

            return new SongAnalysis(length, slotTimes, slots, MeasureAnalyzer.Analyze(slots));
        }

        private static double[] Trim(double[] slotTimes, double length)
        {
            int count = 0;
            while (count < slotTimes.Length && slotTimes[count] < length)
                count++;

            double[] trimmed = new double[count];
            Array.Copy(slotTimes, trimmed, count);
            return trimmed;
        }

        private static void FillSustain(SlotData slots, float[] tonal)
        {
            for (int slot = 0; slot < slots.Count; slot++)
            {
                float floor = slots.Intensity[slot] * SustainLoudnessRatio;
                int length = 0;
                for (int next = slot + 1; next < slots.Count && length < MaxSustainSlots; next++)
                {
                    if (tonal[next] >= SustainOnsetCeiling || slots.Intensity[next] < floor)
                        break;
                    length++;
                }
                slots.Sustain[slot] = length;
            }
        }
    }
}
