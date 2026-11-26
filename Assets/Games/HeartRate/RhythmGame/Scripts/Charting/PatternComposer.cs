using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class PatternComposer
    {
        private const int RecentMemory = 3;
        private const float RecentPenalty = 0.25f;
        private const float MinSizeFit = 0.1f;
        private const float SizeFitSlope = 1.5f;
        private const float StaleLevelGap = 6f;
        private const float StalePenalty = 0.4f;
        private const float UsageDecay = 0.97f;
        private const float JackCost = 1000f;
        private const int BalanceLookahead = 8;
        private const int TransformOptions = LaneMask.LaneCount * 2;
        private const int ShortPhrase = 2;
        private const int LongPhrase = 4;
        private const float ShortPhraseLevel = 8f;

        private readonly PatternCatalogue catalogue;
        private readonly List<PatternDefinition> candidates = new();
        private readonly List<float> candidateWeights = new();
        private readonly PatternDefinition?[] recent = new PatternDefinition?[RecentMemory];
        private readonly float[] laneUsage = new float[LaneMask.LaneCount];

        private PatternDefinition? lead;
        private PatternDefinition? current;
        private PatternDefinition? burst;
        private bool leadMirror;
        private int leadShift;
        private int phraseStart;
        private int recentCursor;
        private bool mirror;
        private int shift;
        private bool burstMirror;
        private int burstShift;
        private int lastMask;
        private int measureEvents;

        public PatternComposer(PatternCatalogue catalogue)
        {
            this.catalogue = catalogue;
        }

        public void BeginMeasure(int measure, MeasureShape shape, DifficultyParameters parameters, SeededRandom random)
        {
            measureEvents = shape.EventCount;
            int phraseLength = parameters.Level >= ShortPhraseLevel ? ShortPhrase : LongPhrase;
            PatternDefinition? variant = lead != null && measure - phraseStart < phraseLength && Allowed(lead, shape, parameters)
                ? catalogue.ClosestInFamily(lead.Family, lead.Order, parameters.Level, shape.NotesPerSecond)
                : null;

            if (variant == null || !Allowed(variant, shape, parameters))
            {
                StartPhrase(measure, shape, parameters, random);
                return;
            }

            current = variant;
            bool answer = (measure - phraseStart) % 2 == 1 && variant.CanMirror;
            mirror = answer ? !leadMirror : leadMirror;
            shift = variant.CanShift ? leadShift : 0;
            if (EntryOverlap(variant, mirror, shift) > 0)
                ChooseTransform(variant, random, out mirror, out shift);
        }

        public void BeginBurst(DifficultyParameters parameters, SeededRandom random)
        {
            burst = PickBurst(parameters, random);
            ChooseTransform(burst, random, out burstMirror, out burstShift);
        }

        public int LaneMaskFor(int eventIndex, int size)
        {
            return current == null ? 0 : Adapt(LaneMask.Transform(current.Step(eventIndex), mirror, shift), size);
        }

        public int BurstMaskFor(int burstIndex, int size)
        {
            return burst == null ? 0 : Adapt(LaneMask.Transform(burst.Step(burstIndex), burstMirror, burstShift), size);
        }

        public void Commit(int mask)
        {
            if (mask == 0)
                return;

            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
                laneUsage[lane] = (laneUsage[lane] * UsageDecay) + (LaneMask.Contains(mask, lane) ? 1f : 0f);
            lastMask = mask;
        }

        private static bool Allowed(PatternDefinition pattern, MeasureShape shape, DifficultyParameters parameters)
        {
            if (!pattern.IsUnlocked(parameters.Level) || pattern.Family == PatternFamily.Burst)
                return false;
            if (pattern.MaxStepSize > Math.Max(1, parameters.MaxChord))
                return false;
            return !pattern.HasJacks || shape.ShortestGap >= parameters.MinJack;
        }

        private static int HandLoad(int step, int lane)
        {
            return LaneMask.Count(step & LaneMask.HandMask(LaneMask.Hand(lane)));
        }

        private int Adapt(int step, int size)
        {
            while (LaneMask.Count(step) > size)
                step &= ~(1 << LaneToRemove(step));
            while (LaneMask.Count(step) < size && step != LaneMask.Full)
                step |= 1 << LaneToAdd(step);
            return step;
        }

        private int LaneToRemove(int step)
        {
            int best = -1;
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                if (LaneMask.Contains(step, lane) && (best < 0 || LaneCost(step, lane) > LaneCost(step, best)))
                    best = lane;
            }
            return best;
        }

        private int LaneToAdd(int step)
        {
            int best = -1;
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                if (!LaneMask.Contains(step, lane) && (best < 0 || LaneCost(step, lane) < LaneCost(step, best)))
                    best = lane;
            }
            return best;
        }

        private float LaneCost(int step, int lane)
        {
            return (LaneMask.Contains(lastMask, lane) ? JackCost : 0f) + HandLoad(step, lane) + laneUsage[lane];
        }

        private void StartPhrase(int measure, MeasureShape shape, DifficultyParameters parameters, SeededRandom random)
        {
            lead = Pick(shape, parameters, random);
            phraseStart = measure;
            current = lead;
            ChooseTransform(lead, random, out mirror, out shift);
            leadMirror = mirror;
            leadShift = shift;
            Remember(lead);
        }

        private PatternDefinition Pick(MeasureShape shape, DifficultyParameters parameters, SeededRandom random)
        {
            CollectCandidates(shape, parameters, true);
            if (candidates.Count == 0)
                CollectCandidates(shape, parameters, false);
            return candidates.Count == 0 ? catalogue.Patterns[0] : Roll(random);
        }

        private PatternDefinition PickBurst(DifficultyParameters parameters, SeededRandom random)
        {
            candidates.Clear();
            candidateWeights.Clear();
            foreach (PatternDefinition pattern in catalogue.Patterns)
            {
                if (pattern.Family != PatternFamily.Burst || !pattern.IsUnlocked(parameters.Level))
                    continue;

                candidates.Add(pattern);
                candidateWeights.Add(pattern.Weight);
            }
            return candidates.Count == 0 ? catalogue.Patterns[0] : Roll(random);
        }

        private PatternDefinition Roll(SeededRandom random)
        {
            float total = 0f;
            foreach (float weight in candidateWeights)
                total += weight;

            float roll = random.NextFloat() * total;
            for (int i = 0; i < candidates.Count; i++)
            {
                roll -= candidateWeights[i];
                if (roll <= 0f)
                    return candidates[i];
            }
            return candidates[^1];
        }

        private void CollectCandidates(MeasureShape shape, DifficultyParameters parameters, bool requireFit)
        {
            candidates.Clear();
            candidateWeights.Clear();
            foreach (PatternDefinition pattern in catalogue.Patterns)
            {
                if (!Allowed(pattern, shape, parameters) || (requireFit && !pattern.Fits(shape.NotesPerSecond)))
                    continue;

                float sizeFit = Math.Max(MinSizeFit, 1f - (SizeFitSlope * Math.Abs(pattern.ChordRatio - shape.ChordRatio)));
                float recency = Array.IndexOf(recent, pattern) >= 0 ? RecentPenalty : 1f;
                float freshness = parameters.Level - pattern.MinLevel > StaleLevelGap ? StalePenalty : 1f;
                candidates.Add(pattern);
                candidateWeights.Add(pattern.Weight * sizeFit * recency * freshness);
            }
        }

        private void ChooseTransform(PatternDefinition pattern, SeededRandom random, out bool chosenMirror, out int chosenShift)
        {
            chosenMirror = false;
            chosenShift = 0;
            float bestCost = float.MaxValue;
            int start = random.Next(TransformOptions);
            for (int i = 0; i < TransformOptions; i++)
            {
                int option = (start + i) % TransformOptions;
                bool optionMirror = option >= LaneMask.LaneCount;
                int optionShift = option % LaneMask.LaneCount;
                if ((optionMirror && !pattern.CanMirror) || (optionShift > 0 && !pattern.CanShift))
                    continue;

                float cost = (EntryOverlap(pattern, optionMirror, optionShift) * JackCost) + BalanceCost(pattern, optionMirror, optionShift);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    chosenMirror = optionMirror;
                    chosenShift = optionShift;
                }
            }
        }

        private int EntryOverlap(PatternDefinition pattern, bool withMirror, int withShift)
        {
            return LaneMask.Count(LaneMask.Transform(pattern.Step(0), withMirror, withShift) & lastMask);
        }

        private float BalanceCost(PatternDefinition pattern, bool withMirror, int withShift)
        {
            float cost = 0f;
            int steps = Math.Min(Math.Max(measureEvents, 1), BalanceLookahead);
            for (int i = 0; i < steps; i++)
            {
                int step = LaneMask.Transform(pattern.Step(i), withMirror, withShift);
                for (int lane = 0; lane < LaneMask.LaneCount; lane++)
                {
                    if (LaneMask.Contains(step, lane))
                        cost += laneUsage[lane];
                }
            }
            return cost;
        }

        private void Remember(PatternDefinition pattern)
        {
            recent[recentCursor] = pattern;
            recentCursor = (recentCursor + 1) % RecentMemory;
        }
    }
}
