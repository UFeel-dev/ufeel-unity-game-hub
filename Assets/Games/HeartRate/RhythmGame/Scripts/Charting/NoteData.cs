namespace GameHub.RhythmGame
{
    public readonly struct NoteData
    {
        public NoteData(int lane, double time, double endTime, float level)
        {
            Lane = lane;
            Time = time;
            EndTime = endTime;
            Level = level;
        }

        public int Lane { get; }

        public double Time { get; }

        public double EndTime { get; }

        public float Level { get; }

        public bool IsHold => EndTime > Time;
    }
}
