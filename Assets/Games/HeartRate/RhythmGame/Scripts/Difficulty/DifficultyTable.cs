namespace GameHub.RhythmGame
{
    public static class DifficultyTable
    {
        private static readonly DifficultyParameters[] Levels =
        {
            Row(level: 1f, notesPerSecond: 1.2f, minGapMs: 450f, minJackMs: 900f, maxChord: 1, chordShare: 0f, holdShare: 0.2f, maxBurst: 1, overmap: 0),
            Row(level: 3f, notesPerSecond: 2.5f, minGapMs: 250f, minJackMs: 500f, maxChord: 2, chordShare: 0.08f, holdShare: 0.22f, maxBurst: 2, overmap: 0),
            Row(level: 5f, notesPerSecond: 4.2f, minGapMs: 160f, minJackMs: 340f, maxChord: 2, chordShare: 0.15f, holdShare: 0.25f, maxBurst: 3, overmap: 0),
            Row(level: 7f, notesPerSecond: 6.2f, minGapMs: 110f, minJackMs: 230f, maxChord: 3, chordShare: 0.2f, holdShare: 0.3f, maxBurst: 4, overmap: 0),
            Row(level: 9f, notesPerSecond: 8.5f, minGapMs: 78f, minJackMs: 160f, maxChord: 3, chordShare: 0.25f, holdShare: 0.35f, maxBurst: 8, overmap: 0),
            Row(level: 12f, notesPerSecond: 13f, minGapMs: 40f, minJackMs: 100f, maxChord: 4, chordShare: 0.3f, holdShare: 0.4f, maxBurst: 12, overmap: 2),
            Row(level: 14f, notesPerSecond: 16f, minGapMs: 38f, minJackMs: 88f, maxChord: 4, chordShare: 0.34f, holdShare: 0.42f, maxBurst: 16, overmap: 2),
            Row(level: 16f, notesPerSecond: 20f, minGapMs: 36f, minJackMs: 78f, maxChord: 4, chordShare: 0.4f, holdShare: 0.45f, maxBurst: 24, overmap: 2),
        };

        public static DifficultyParameters At(float level)
        {
            if (level <= Levels[0].Level)
                return Levels[0];

            for (int i = 1; i < Levels.Length; i++)
            {
                if (level <= Levels[i].Level)
                    return DifficultyParameters.Lerp(Levels[i - 1], Levels[i], level);
            }
            return Levels[^1];
        }

        private static DifficultyParameters Row(float level, float notesPerSecond, float minGapMs, float minJackMs, int maxChord, float chordShare, float holdShare, int maxBurst, int overmap)
        {
            return new DifficultyParameters(level, notesPerSecond, minGapMs, minJackMs, maxChord, chordShare, holdShare, maxBurst, overmap);
        }
    }
}
