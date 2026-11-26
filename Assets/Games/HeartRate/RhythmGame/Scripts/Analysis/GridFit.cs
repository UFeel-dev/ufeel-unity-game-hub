namespace GameHub.RhythmGame
{
    public readonly struct GridFit
    {
        public GridFit(double bpm, double firstBeatTime)
        {
            Bpm = bpm;
            FirstBeatTime = firstBeatTime;
        }

        public double Bpm { get; }

        public double FirstBeatTime { get; }
    }
}
