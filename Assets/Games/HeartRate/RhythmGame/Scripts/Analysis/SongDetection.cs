namespace GameHub.RhythmGame
{
    public readonly struct SongDetection
    {
        public SongDetection(double bpm, double firstBeat, double previewStart)
        {
            Bpm = bpm;
            FirstBeat = firstBeat;
            PreviewStart = previewStart;
        }

        public double Bpm { get; }

        public double FirstBeat { get; }

        public double PreviewStart { get; }
    }
}
