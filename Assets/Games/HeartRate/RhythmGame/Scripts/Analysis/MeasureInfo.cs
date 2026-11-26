namespace GameHub.RhythmGame
{
    public readonly struct MeasureInfo
    {
        public MeasureInfo(int cluster, int reference, bool isTernary, int fillDivisor, float intensity)
        {
            Cluster = cluster;
            Reference = reference;
            IsTernary = isTernary;
            FillDivisor = fillDivisor;
            Intensity = intensity;
        }

        public int Cluster { get; }

        public int Reference { get; }

        public bool IsTernary { get; }

        public int FillDivisor { get; }

        public float Intensity { get; }

        public bool IsSilent => Cluster < 0;
    }
}
