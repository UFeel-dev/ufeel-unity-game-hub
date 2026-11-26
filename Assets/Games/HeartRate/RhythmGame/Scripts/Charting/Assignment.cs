using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class Assignment
    {
        private readonly SongAnalysis analysis;
        private readonly DifficultyParameters parameters;
        private readonly List<NoteData> notes = new(1024);
        private readonly List<int> noteMeasures = new(1024);
        private readonly double release;

        private PlayabilityGuard guard = new();

        public Assignment(SongAnalysis analysis, DifficultyParameters parameters)
        {
            this.analysis = analysis;
            this.parameters = parameters;
            release = HoldPlanner.ReleaseSlots(analysis) * analysis.SlotDuration;
        }

        public int Place(ChartEvent chartEvent, int mask, int measure)
        {
            int placed = Commit(chartEvent, mask, out int holdMask);
            double time = analysis.SlotTime(chartEvent.Slot);
            double endTime = holdMask != 0 ? analysis.SlotTime(chartEvent.HoldEnd) : time;
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                if (!LaneMask.Contains(placed, lane))
                    continue;

                notes.Add(new NoteData(lane, time, LaneMask.Contains(holdMask, lane) ? endTime : time, parameters.Level));
                noteMeasures.Add(measure);
            }
            return placed;
        }

        public int[] PlaceCopy(List<ChartEvent> events, int[] source, bool mirror, int measure)
        {
            if (Conflicts(events, source, mirror) > Conflicts(events, source, !mirror))
                mirror = !mirror;

            int[] placed = new int[events.Count];
            for (int i = 0; i < events.Count; i++)
            {
                int mask = mirror ? LaneMask.Mirror(source[i]) : source[i];
                placed[i] = mask == 0 ? 0 : Place(events[i], mask, measure);
            }
            return placed;
        }

        public ChartLayer ToLayer()
        {
            return new ChartLayer(parameters, notes, noteMeasures, analysis.MeasureCount);
        }

        private int Commit(ChartEvent chartEvent, int mask, out int holdMask)
        {
            double time = analysis.SlotTime(chartEvent.Slot);
            int placed = guard.Resolve(mask, time, parameters.MinJack, parameters.MaxChord);
            holdMask = placed != 0 && chartEvent.IsHold ? PlayabilityGuard.HoldLane(placed) : 0;
            if (placed != 0)
                guard.Commit(placed, holdMask, time, holdMask != 0 ? analysis.SlotTime(chartEvent.HoldEnd) : time, release);
            return placed;
        }

        private int Conflicts(List<ChartEvent> events, int[] source, bool mirror)
        {
            PlayabilityGuard saved = guard;
            guard = saved.Clone();
            int conflicts = 0;
            for (int i = 0; i < events.Count; i++)
            {
                int mask = mirror ? LaneMask.Mirror(source[i]) : source[i];
                if (mask != 0 && Commit(events[i], mask, out _) != mask)
                    conflicts++;
            }
            guard = saved;
            return conflicts;
        }
    }
}
