using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class RhythmLayers
    {
        public const double BurstGap = 0.1;

        private const float MusicalStrength = 0.2f;
        private const float QuarterStrength = 0.25f;
        private const float EighthStrength = 0.35f;
        private const float OffFamilyStrength = 0.5f;
        private const float MetricImportance = 0.45f;
        private const float LowBandBonus = 0.1f;
        private const float MidBandBonus = 0.08f;
        private const float FillerImportance = 0.3f;
        private const double StartMargin = 0.5;
        private const double EndMargin = 1.0;
        private const float QuietShare = 0.35f;
        private const float ChordStrength = 0.35f;
        private const float TripleStrength = 0.6f;
        private const float QuadStrength = 0.85f;
        private const float BeatWeight = 0.75f;
        private const float HalfMeasureWeight = 0.85f;
        private const double Epsilon = 1e-6;
        private const int MaxOvermap = 2;

        private readonly SongAnalysis analysis;
        private readonly int[][] candidates;

        public RhythmLayers(SongAnalysis analysis)
        {
            this.analysis = analysis;
            candidates = new int[analysis.MeasureCount][];
            for (int measure = 0; measure < analysis.MeasureCount; measure++)
                candidates[measure] = OrderCandidates(measure);
        }

        public List<ChartEvent>[] Plan(int layer, List<ChartEvent>[]? previousLayer)
        {
            DifficultyParameters parameters = DifficultyTable.At(DifficultySettings.LayerLevel(layer));
            List<ChartEvent>[] measures = new List<ChartEvent>[analysis.MeasureCount];
            int tail = int.MinValue;
            for (int measure = 0; measure < measures.Length; measure++)
            {
                MeasureInfo info = analysis.Measure(measure);
                int floor = NextFloor(measure, measures, previousLayer);
                List<ChartEvent> events = !info.IsSilent && info.Reference < measure
                    ? Copy(measures[info.Reference], measure - info.Reference, tail, floor, parameters)
                    : Fresh(measure, previousLayer, tail, floor, parameters);
                measures[measure] = events;
                if (events.Count > 0)
                    tail = events[^1].Slot;
            }
            return measures;
        }

        private static float Threshold(int divisor)
        {
            return divisor switch
            {
                3 or 4 => QuarterStrength,
                6 or 8 => EighthStrength,
                _ => MusicalStrength,
            };
        }

        private static int Rank(int divisor, bool ternary)
        {
            if (divisor == 1)
                return 0;
            if (ternary)
            {
                return divisor switch
                {
                    3 => 1,
                    6 => 2,
                    _ => int.MaxValue,
                };
            }
            return divisor switch
            {
                2 => 1,
                4 => 2,
                8 => 3,
                _ => int.MaxValue,
            };
        }

        private static bool Contains(List<ChartEvent> events, int slot)
        {
            foreach (ChartEvent chartEvent in events)
            {
                if (chartEvent.Slot == slot)
                    return true;
            }
            return false;
        }

        private static void Insert(List<ChartEvent> events, ChartEvent chartEvent)
        {
            int index = 0;
            while (index < events.Count && events[index].Slot < chartEvent.Slot)
                index++;
            events.Insert(index, chartEvent);
        }

        private float Importance(int slot)
        {
            float strength = analysis.Strength(slot);
            float bonus = analysis.Band(slot) switch
            {
                OnsetBand.Low => LowBandBonus,
                OnsetBand.Mid => MidBandBonus,
                _ => 0f,
            };
            return strength >= Threshold(BeatGrid.DivisorOf(slot))
                ? strength + (MetricImportance * BeatGrid.MetricWeight(slot)) + bonus
                : FillerImportance * BeatGrid.MetricWeight(slot);
        }

        private int[] OrderCandidates(int measure)
        {
            MeasureInfo info = analysis.Measure(measure);
            List<int> musical = new();
            List<int> fillers = new();
            int first = measure * BeatGrid.SlotsPerMeasure;
            int fillRank = Rank(info.FillDivisor, info.IsTernary);
            for (int slot = first; slot < first + BeatGrid.SlotsPerMeasure && slot < analysis.SlotCount; slot++)
            {
                double time = analysis.SlotTime(slot);
                int divisor = BeatGrid.DivisorOf(slot);
                if (time < StartMargin || time > analysis.Length - EndMargin || divisor > 8)
                    continue;

                float strength = analysis.Strength(slot);
                bool fits = BeatGrid.Fits(divisor, info.IsTernary);
                if (strength >= Threshold(divisor) && (fits || strength >= OffFamilyStrength))
                    musical.Add(slot);
                else if (fits && Rank(divisor, info.IsTernary) <= fillRank + MaxOvermap)
                    fillers.Add(slot);
            }

            int ByImportance(int a, int b)
            {
                int compare = Importance(b).CompareTo(Importance(a));
                return compare != 0 ? compare : a.CompareTo(b);
            }
            musical.Sort(ByImportance);
            fillers.Sort(ByImportance);
            musical.AddRange(fillers);
            return musical.ToArray();
        }

        private int NextFloor(int measure, List<ChartEvent>[] current, List<ChartEvent>[]? previousLayer)
        {
            int next = measure + 1;
            if (next >= analysis.MeasureCount)
                return int.MaxValue;

            MeasureInfo info = analysis.Measure(next);
            if (!info.IsSilent && info.Reference < next && info.Reference < measure && current[info.Reference].Count > 0)
                return current[info.Reference][0].Slot + ((next - info.Reference) * BeatGrid.SlotsPerMeasure);

            int floor = previousLayer != null && previousLayer[next].Count > 0 ? previousLayer[next][0].Slot : int.MaxValue;
            int downbeat = next * BeatGrid.SlotsPerMeasure;
            return analysis.Strength(downbeat) >= MusicalStrength ? Math.Min(floor, downbeat) : floor;
        }

        private List<ChartEvent> Copy(List<ChartEvent> reference, int measureOffset, int tail, int floor, DifficultyParameters parameters)
        {
            int shift = measureOffset * BeatGrid.SlotsPerMeasure;
            List<ChartEvent> events = new(reference.Count);
            foreach (ChartEvent chartEvent in reference)
            {
                ChartEvent shifted = chartEvent.Shifted(shift);
                double time = analysis.SlotTime(shifted.Slot);
                if (time > analysis.Length - EndMargin || shifted.Slot >= analysis.SlotCount)
                    continue;
                if (events.Count == 0 && tail != int.MinValue && time - analysis.SlotTime(tail) < parameters.MinGap - Epsilon)
                    continue;
                events.Add(shifted);
            }

            double floorTime = floor != int.MaxValue ? analysis.SlotTime(floor) : double.PositiveInfinity;
            while (events.Count > 0 && floorTime - analysis.SlotTime(events[^1].Slot) < parameters.MinGap - Epsilon)
                events.RemoveAt(events.Count - 1);
            return events;
        }

        private List<ChartEvent> Fresh(int measure, List<ChartEvent>[]? previousLayer, int tail, int floor, DifficultyParameters parameters)
        {
            MeasureInfo info = analysis.Measure(measure);
            List<ChartEvent>? previous = previousLayer?[measure];
            List<ChartEvent> events = previous != null ? new List<ChartEvent>(previous) : new List<ChartEvent>();
            int fillRank = Rank(info.FillDivisor, info.IsTernary) + parameters.Overmap;
            while (events.Count > 0 && tail != int.MinValue && analysis.SlotTime(events[0].Slot) - analysis.SlotTime(tail) < parameters.MinGap - Epsilon)
                events.RemoveAt(0);

            int noteBudget = NoteBudget(measure, parameters);
            int eventBudget = Math.Max(events.Count, (int)Math.Round(noteBudget * (1f - parameters.ChordShare)));
            foreach (int slot in candidates[measure])
            {
                if (events.Count >= eventBudget)
                    break;
                int divisor = BeatGrid.DivisorOf(slot);
                bool allowed = analysis.Strength(slot) >= Threshold(divisor) || Rank(divisor, info.IsTernary) <= fillRank;
                if (allowed && !Contains(events, slot) && Fits(events, slot, tail, floor, parameters))
                    Insert(events, new ChartEvent(slot, 1, 0));
            }

            AddChords(events, noteBudget, parameters);
            return events;
        }

        private int NoteBudget(int measure, DifficultyParameters parameters)
        {
            MeasureInfo info = analysis.Measure(measure);
            if (info.IsSilent)
                return 0;

            float loudness = QuietShare + ((1f - QuietShare) * info.Intensity);
            return (int)Math.Round(parameters.NotesPerSecond * analysis.MeasureDuration * loudness);
        }

        private bool Fits(List<ChartEvent> events, int slot, int tail, int floor, DifficultyParameters parameters)
        {
            double time = analysis.SlotTime(slot);
            int index = 0;
            while (index < events.Count && events[index].Slot < slot)
                index++;

            double previous = index > 0 ? analysis.SlotTime(events[index - 1].Slot) : TimeOr(tail, int.MinValue, double.NegativeInfinity);
            double next = index < events.Count ? analysis.SlotTime(events[index].Slot) : TimeOr(floor, int.MaxValue, double.PositiveInfinity);
            if (time - previous < parameters.MinGap - Epsilon || next - time < parameters.MinGap - Epsilon)
                return false;

            int run = 1;
            double cursor = time;
            for (int i = index - 1; i >= 0 && cursor - analysis.SlotTime(events[i].Slot) < BurstGap; i--)
            {
                run++;
                cursor = analysis.SlotTime(events[i].Slot);
            }

            cursor = time;
            for (int i = index; i < events.Count && analysis.SlotTime(events[i].Slot) - cursor < BurstGap; i++)
            {
                run++;
                cursor = analysis.SlotTime(events[i].Slot);
            }
            return run <= parameters.MaxBurst;
        }

        private double TimeOr(int slot, int missing, double fallback)
        {
            return slot == missing ? fallback : analysis.SlotTime(slot);
        }

        private void AddChords(List<ChartEvent> events, int noteBudget, DifficultyParameters parameters)
        {
            int total = 0;
            foreach (ChartEvent chartEvent in events)
                total += chartEvent.Size;

            List<int> order = new(events.Count);
            for (int i = 0; i < events.Count; i++)
                order.Add(i);
            order.Sort((a, b) => Importance(events[b].Slot).CompareTo(Importance(events[a].Slot)));

            bool changed = true;
            while (total < noteBudget && changed)
            {
                changed = false;
                foreach (int index in order)
                {
                    if (total >= noteBudget)
                        break;
                    if (events[index].Size >= ChordCap(events[index].Slot, parameters))
                        continue;

                    events[index] = events[index].WithSize(events[index].Size + 1);
                    total++;
                    changed = true;
                }
            }
        }

        private int ChordCap(int slot, DifficultyParameters parameters)
        {
            float strength = analysis.Strength(slot);
            float metric = BeatGrid.MetricWeight(slot);
            int cap = parameters.MaxChord;
            if (metric < BeatWeight)
                cap = Math.Min(cap, strength >= ChordStrength ? 2 : 1);
            if (strength < TripleStrength)
                cap = Math.Min(cap, 2);
            if (cap >= 4 && (metric < HalfMeasureWeight || strength < QuadStrength))
                cap = 3;
            return Math.Max(1, cap);
        }
    }
}
