using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class LaneAssigner
    {
        private readonly SongAnalysis analysis;
        private readonly PatternCatalogue catalogue;
        private readonly int seed;
        private readonly List<ChartEvent> measureEvents = new(32);

        public LaneAssigner(SongAnalysis analysis, PatternCatalogue catalogue, int seed)
        {
            this.analysis = analysis;
            this.catalogue = catalogue;
            this.seed = seed;
        }

        public ChartLayer Assign(DifficultyParameters parameters, int layer, List<ChartEvent> events)
        {
            Assignment assignment = new(analysis, parameters);
            PatternComposer composer = new(catalogue);
            int[][] masks = new int[analysis.MeasureCount][];
            List<ChartEvent>[] eventsByMeasure = Split(events);

            for (int measure = 0; measure < analysis.MeasureCount; measure++)
            {
                List<ChartEvent> current = eventsByMeasure[measure];
                MeasureInfo info = analysis.Measure(measure);
                int[]? copied = CopySource(info, measure, eventsByMeasure, masks);
                bool mirrorCopy = RunIndex(measure) % 2 == 1;
                masks[measure] = copied != null
                    ? assignment.PlaceCopy(current, copied, mirrorCopy, measure)
                    : PlaceFresh(assignment, composer, current, parameters, measure, layer);
            }
            return assignment.ToLayer();
        }

        private static bool SameShape(List<ChartEvent> a, List<ChartEvent> b, int shift)
        {
            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Slot != b[i].Slot + shift || a[i].Size != b[i].Size)
                    return false;
            }
            return true;
        }

        private static int[]? CopySource(MeasureInfo info, int measure, List<ChartEvent>[] eventsByMeasure, int[][] masks)
        {
            if (info.IsSilent || info.Reference >= measure)
                return null;

            int shift = (measure - info.Reference) * BeatGrid.SlotsPerMeasure;
            return SameShape(eventsByMeasure[measure], eventsByMeasure[info.Reference], shift) ? masks[info.Reference] : null;
        }

        private int RunIndex(int measure)
        {
            MeasureInfo info = analysis.Measure(measure);
            int index = 0;
            for (int previous = measure - 1; previous >= 0; previous--)
            {
                MeasureInfo other = analysis.Measure(previous);
                if (info.IsSilent || other.Cluster != info.Cluster)
                    break;
                index++;
            }
            return index;
        }

        private List<ChartEvent>[] Split(List<ChartEvent> events)
        {
            List<ChartEvent>[] split = new List<ChartEvent>[analysis.MeasureCount];
            for (int measure = 0; measure < split.Length; measure++)
                split[measure] = new List<ChartEvent>();
            foreach (ChartEvent chartEvent in events)
            {
                int measure = chartEvent.Slot / BeatGrid.SlotsPerMeasure;
                if (measure >= 0 && measure < split.Length)
                    split[measure].Add(chartEvent);
            }
            return split;
        }

        private int[] PlaceFresh(Assignment assignment, PatternComposer composer, List<ChartEvent> events, DifficultyParameters parameters, int measure, int layer)
        {
            int[] placed = new int[events.Count];
            if (events.Count == 0)
                return placed;

            SeededRandom random = new(SeededRandom.Mix(seed, measure, layer));
            measureEvents.Clear();
            measureEvents.AddRange(events);
            composer.BeginMeasure(measure, MeasureShape.From(measureEvents, analysis), parameters, random);

            int regular = 0;
            int burstStep = 0;
            for (int i = 0; i < events.Count; i++)
            {
                bool inBurst = InBurst(events, i);
                if (inBurst && (i == 0 || !Adjacent(events, i - 1)))
                {
                    composer.BeginBurst(parameters, random);
                    burstStep = 0;
                }

                int mask = inBurst ? composer.BurstMaskFor(burstStep++, events[i].Size) : composer.LaneMaskFor(regular++, events[i].Size);
                placed[i] = assignment.Place(events[i], mask, measure);
                composer.Commit(placed[i]);
            }
            return placed;
        }

        private bool Adjacent(List<ChartEvent> events, int index)
        {
            return index + 1 < events.Count && analysis.SlotTime(events[index + 1].Slot) - analysis.SlotTime(events[index].Slot) < RhythmLayers.BurstGap;
        }

        private bool InBurst(List<ChartEvent> events, int index)
        {
            return Adjacent(events, index) || (index > 0 && Adjacent(events, index - 1));
        }
    }
}
