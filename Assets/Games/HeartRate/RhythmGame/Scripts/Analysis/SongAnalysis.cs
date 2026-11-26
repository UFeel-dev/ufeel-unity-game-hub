using System;

namespace GameHub.RhythmGame
{
    public sealed class SongAnalysis
    {
        private readonly double[] slotTimes;
        private readonly float[] strength;
        private readonly OnsetBand[] bands;
        private readonly int[] sustain;
        private readonly MeasureInfo[] measures;

        public SongAnalysis(double length, double[] slotTimes, SlotData slots, MeasureInfo[] measures)
        {
            Length = length;
            this.slotTimes = slotTimes;
            strength = slots.Strength;
            bands = slots.Bands;
            sustain = slots.Sustain;
            this.measures = measures;
            SlotDuration = slotTimes.Length > 1 ? (slotTimes[^1] - slotTimes[0]) / (slotTimes.Length - 1) : 0.01;
        }

        public double Length { get; }

        public double SlotDuration { get; }

        public double MeasureDuration => SlotDuration * BeatGrid.SlotsPerMeasure;

        public double Bpm => 60d / (SlotDuration * BeatGrid.SlotsPerBeat);

        public double FirstBeatTime => slotTimes.Length > 0 ? slotTimes[0] : 0d;

        public int SlotCount => slotTimes.Length;

        public int MeasureCount => measures.Length;

        public double SlotTime(int slot)
        {
            if (slotTimes.Length == 0)
                return slot * SlotDuration;
            if (slot < 0)
                return slotTimes[0] + (slot * SlotDuration);
            if (slot >= slotTimes.Length)
                return slotTimes[^1] + ((slot - slotTimes.Length + 1) * SlotDuration);
            return slotTimes[slot];
        }

        public double MeasureTime(int measure)
        {
            return SlotTime(measure * BeatGrid.SlotsPerMeasure);
        }

        public int MeasureAt(double time)
        {
            if (slotTimes.Length == 0 || time < FirstBeatTime || time >= slotTimes[^1])
                return (int)Math.Floor((time - FirstBeatTime) / MeasureDuration);

            int low = 0;
            int high = MeasureCount - 1;
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (MeasureTime(middle) <= time)
                    low = middle;
                else
                    high = middle - 1;
            }
            return low;
        }

        public MeasureInfo Measure(int measure)
        {
            return measures[measure];
        }

        public float Strength(int slot)
        {
            return slot >= 0 && slot < strength.Length ? strength[slot] : 0f;
        }

        public OnsetBand Band(int slot)
        {
            return slot >= 0 && slot < bands.Length ? bands[slot] : OnsetBand.None;
        }

        public int SustainSlots(int slot)
        {
            return slot >= 0 && slot < sustain.Length ? sustain[slot] : 0;
        }
    }
}
