using System;

namespace GameHub.RhythmGame
{
    public sealed class ChartSet
    {
        private const int SearchSteps = 16;
        private const float IntervalSlack = 0.9f;

        private readonly ChartLayer[] layers;

        public ChartSet(SongAnalysis analysis, ChartLayer[] layers)
        {
            Analysis = analysis;
            this.layers = layers;
        }

        public SongAnalysis Analysis { get; }

        public ChartLayer Layer(int index)
        {
            return layers[index];
        }

        public ChartLayer LayerFor(float level)
        {
            return layers[DifficultySettings.NearestLayer(level)];
        }

        public float EquivalentLevel(float level, float rate)
        {
            if (rate == 1f)
                return level;

            float targetNotes = NoteCount(level);
            DifficultyParameters target = DifficultyTable.At(level);
            float low = rate > 1f ? DifficultySettings.MinLevel : level;
            float high = rate > 1f ? level : DifficultySettings.MaxLevel;
            for (int i = 0; i < SearchSteps; i++)
            {
                float middle = (low + high) / 2f;
                DifficultyParameters candidate = DifficultyTable.At(middle);
                bool fits = NoteCount(middle) * rate <= targetNotes
                    && candidate.MinGapMs / rate >= target.MinGapMs * IntervalSlack
                    && candidate.MinJackMs / rate >= target.MinJackMs * IntervalSlack;
                if (fits)
                    low = middle;
                else
                    high = middle;
            }
            return low;
        }

        private float NoteCount(float level)
        {
            float position = Math.Clamp((level - DifficultySettings.MinLevel) / DifficultySettings.LayerStep, 0f, layers.Length - 1);
            int below = (int)Math.Floor(position);
            int above = Math.Min(below + 1, layers.Length - 1);
            float blend = position - below;
            return layers[below].NoteCount + ((layers[above].NoteCount - layers[below].NoteCount) * blend);
        }
    }
}
