using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public readonly struct MeasureShape
    {
        public MeasureShape(int eventCount, float notesPerSecond, float chordRatio, double shortestGap)
        {
            EventCount = eventCount;
            NotesPerSecond = notesPerSecond;
            ChordRatio = chordRatio;
            ShortestGap = shortestGap;
        }

        public int EventCount { get; }

        public float NotesPerSecond { get; }

        public float ChordRatio { get; }

        public double ShortestGap { get; }

        public static MeasureShape From(IReadOnlyList<ChartEvent> events, SongAnalysis analysis)
        {
            int notes = 0;
            int chords = 0;
            double shortest = double.MaxValue;
            for (int i = 0; i < events.Count; i++)
            {
                notes += events[i].Size;
                if (events[i].Size > 1)
                    chords++;
                if (i > 0)
                    shortest = Math.Min(shortest, analysis.SlotTime(events[i].Slot) - analysis.SlotTime(events[i - 1].Slot));
            }

            float chordRatio = events.Count == 0 ? 0f : chords / (float)events.Count;
            return new MeasureShape(events.Count, (float)(notes / analysis.MeasureDuration), chordRatio, shortest);
        }
    }
}
