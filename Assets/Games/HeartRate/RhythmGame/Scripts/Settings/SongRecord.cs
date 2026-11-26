namespace GameHub.RhythmGame
{
    public readonly struct SongRecord
    {
        public SongRecord(int score, float accuracy, Grade grade, float baseLevel, float scaling)
        {
            HasRecord = true;
            Score = score;
            Accuracy = accuracy;
            Grade = grade;
            Base = baseLevel;
            Scaling = scaling;
        }

        public bool HasRecord { get; }

        public int Score { get; }

        public float Accuracy { get; }

        public Grade Grade { get; }

        public float Base { get; }

        public float Scaling { get; }
    }
}
