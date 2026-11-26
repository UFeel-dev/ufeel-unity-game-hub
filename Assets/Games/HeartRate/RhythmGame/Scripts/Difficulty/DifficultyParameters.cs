using System;

namespace GameHub.RhythmGame
{
    public sealed class DifficultyParameters
    {
        public DifficultyParameters(float level, float notesPerSecond, float minGapMs, float minJackMs, int maxChord, float chordShare, float holdShare, int maxBurst, int overmap)
        {
            Level = level;
            NotesPerSecond = notesPerSecond;
            MinGapMs = minGapMs;
            MinJackMs = minJackMs;
            MaxChord = maxChord;
            ChordShare = chordShare;
            HoldShare = holdShare;
            MaxBurst = maxBurst;
            Overmap = overmap;
        }

        public float Level { get; }

        public float NotesPerSecond { get; }

        public float MinGapMs { get; }

        public float MinJackMs { get; }

        public int MaxChord { get; }

        public float ChordShare { get; }

        public float HoldShare { get; }

        public int MaxBurst { get; }

        public int Overmap { get; }

        public double MinGap => MinGapMs / 1000d;

        public double MinJack => MinJackMs / 1000d;

        public static DifficultyParameters Lerp(DifficultyParameters from, DifficultyParameters to, float level)
        {
            float t = to.Level > from.Level ? Math.Clamp((level - from.Level) / (to.Level - from.Level), 0f, 1f) : 0f;
            return new DifficultyParameters(
                level,
                Mix(from.NotesPerSecond, to.NotesPerSecond, t),
                Mix(from.MinGapMs, to.MinGapMs, t),
                Mix(from.MinJackMs, to.MinJackMs, t),
                (int)Math.Floor(Mix(from.MaxChord, to.MaxChord, t) + 0.001f),
                Mix(from.ChordShare, to.ChordShare, t),
                Mix(from.HoldShare, to.HoldShare, t),
                (int)Math.Floor(Mix(from.MaxBurst, to.MaxBurst, t) + 0.001f),
                (int)Math.Floor(Mix(from.Overmap, to.Overmap, t) + 0.001f));
        }

        private static float Mix(float from, float to, float t)
        {
            return from + ((to - from) * t);
        }
    }
}
