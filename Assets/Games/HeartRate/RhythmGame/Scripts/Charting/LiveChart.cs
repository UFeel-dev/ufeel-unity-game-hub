using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class LiveChart
    {
        private readonly ChartSet set;
        private readonly PlayabilityGuard guard = new();
        private readonly double release;

        private int nextMeasure;
        private int layer = -1;

        public LiveChart(ChartSet set)
        {
            this.set = set;
            release = HoldPlanner.ReleaseSlots(set.Analysis) * set.Analysis.SlotDuration;
        }

        public bool IsComplete => nextMeasure >= set.Analysis.MeasureCount;

        public double LastNoteEnd { get; private set; }

        public void ScheduleUntil(double horizon, float level, float rate, List<NoteData> output)
        {
            while (!IsComplete && set.Analysis.MeasureTime(nextMeasure) < horizon)
            {
                float equivalent = set.EquivalentLevel(level, rate);
                layer = SelectLayer(equivalent);
                ChartLayer chosen = set.Layer(layer);
                float scoredLevel = Math.Clamp(chosen.Level + level - equivalent, DifficultySettings.MinLevel, DifficultySettings.MaxLevel);
                Emit(chosen, nextMeasure, scoredLevel, output);
                nextMeasure++;
            }
        }

        private int SelectLayer(float level)
        {
            if (layer < 0 || Math.Abs(level - DifficultySettings.LayerLevel(layer)) > DifficultySettings.LayerHysteresis)
                return DifficultySettings.NearestLayer(level);
            return layer;
        }

        private void Emit(ChartLayer source, int measure, float scoredLevel, List<NoteData> output)
        {
            int index = source.MeasureStart(measure);
            int end = source.MeasureEnd(measure);
            while (index < end)
            {
                NoteData first = source.Note(index);
                int mask = 0;
                int holdMask = 0;
                double endTime = first.Time;
                int groupEnd = index;
                while (groupEnd < end && source.Note(groupEnd).Time == first.Time)
                {
                    NoteData note = source.Note(groupEnd);
                    mask |= 1 << note.Lane;
                    if (note.IsHold)
                        holdMask |= 1 << note.Lane;
                    endTime = Math.Max(endTime, note.EndTime);
                    groupEnd++;
                }

                int placed = guard.Resolve(mask, first.Time, source.Parameters.MinJack, source.Parameters.MaxChord);
                if (placed != mask)
                    holdMask = 0;
                if (placed != 0)
                {
                    guard.Commit(placed, holdMask, first.Time, holdMask != 0 ? endTime : first.Time, release);
                    for (int lane = 0; lane < LaneMask.LaneCount; lane++)
                    {
                        if (!LaneMask.Contains(placed, lane))
                            continue;

                        double noteEnd = LaneMask.Contains(holdMask, lane) ? endTime : first.Time;
                        output.Add(new NoteData(lane, first.Time, noteEnd, scoredLevel));
                        LastNoteEnd = Math.Max(LastNoteEnd, noteEnd);
                    }
                }
                index = groupEnd;
            }
        }
    }
}
