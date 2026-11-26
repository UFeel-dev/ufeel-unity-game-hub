using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public static class HoldPlanner
    {
        private const int MinHoldSlots = BeatGrid.SlotsPerBeat;
        private const int MaxAnchorSlots = BeatGrid.SlotsPerBeat * 2;
        private const int MinReleaseSlots = BeatGrid.SlotsPerBeat / 4;
        private const double MinRelease = 0.1;
        private const float AnchorShare = 0.6f;
        private const float AnchorStrength = 0.4f;
        private const float BeatWeight = 0.75f;
        private const int CoarsestHoldDivisor = 4;

        public static int ReleaseSlots(SongAnalysis analysis)
        {
            return Math.Max(MinReleaseSlots, (int)Math.Ceiling(MinRelease / analysis.SlotDuration));
        }

        public static void Apply(List<ChartEvent> events, DifficultyParameters parameters, SongAnalysis analysis)
        {
            int release = ReleaseSlots(analysis);
            int holdUntil = int.MinValue;
            for (int i = 0; i < events.Count; i++)
            {
                ChartEvent chartEvent = events[i];
                if (chartEvent.Size > Math.Max(1, parameters.MaxChord - 1) || BeatGrid.DivisorOf(chartEvent.Slot) > CoarsestHoldDivisor || chartEvent.Slot < holdUntil)
                    continue;

                int length = HoldLength(events, i, release, parameters, analysis);
                if (length < MinHoldSlots)
                    continue;

                events[i] = chartEvent.WithHold(chartEvent.Slot + length);
                holdUntil = chartEvent.Slot + length + release;
                LimitOverlaps(events, i, holdUntil, parameters.MaxChord);
            }
        }

        private static int HoldLength(List<ChartEvent> events, int index, int release, DifficultyParameters parameters, SongAnalysis analysis)
        {
            int slot = events[index].Slot;
            int measureEnd = ((slot / BeatGrid.SlotsPerMeasure) + 1) * BeatGrid.SlotsPerMeasure;
            int next = index + 1 < events.Count ? Math.Min(events[index + 1].Slot, measureEnd) : measureEnd;
            float roll = Roll(analysis, slot);
            if (events[index].Size == 1 && next - slot >= MinHoldSlots + release && roll < parameters.HoldShare)
                return next - slot - release;

            bool anchor = parameters.MaxChord >= 2
                && BeatGrid.MetricWeight(slot) >= BeatWeight
                && analysis.Strength(slot) >= AnchorStrength
                && roll < parameters.HoldShare * AnchorShare;
            if (!anchor)
                return 0;

            int span = Math.Clamp(analysis.SustainSlots(slot), MinHoldSlots, MaxAnchorSlots);
            return Math.Min(span, measureEnd - slot - release);
        }

        private static void LimitOverlaps(List<ChartEvent> events, int holdIndex, int holdUntil, int maxChord)
        {
            for (int i = holdIndex + 1; i < events.Count && events[i].Slot < holdUntil; i++)
            {
                if (events[i].Size >= maxChord)
                    events[i] = events[i].WithSize(Math.Max(1, maxChord - 1));
                if (events[i].IsHold)
                    events[i] = events[i].WithHold(0);
            }
        }

        private static float Roll(SongAnalysis analysis, int slot)
        {
            int measure = slot / BeatGrid.SlotsPerMeasure;
            MeasureInfo info = analysis.Measure(Math.Min(measure, analysis.MeasureCount - 1));
            int key = info.IsSilent ? -1 - measure : info.Cluster;
            unchecked
            {
                uint hash = ((uint)key * 73856093u) ^ ((uint)(slot % BeatGrid.SlotsPerMeasure) * 19349663u);
                hash ^= hash >> 13;
                hash *= 0x5bd1e995u;
                hash ^= hash >> 15;
                return (hash & 0xFFFFu) / 65536f;
            }
        }
    }
}
