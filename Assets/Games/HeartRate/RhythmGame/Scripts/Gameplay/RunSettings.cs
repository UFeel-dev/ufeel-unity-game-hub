namespace GameHub.RhythmGame
{
    public readonly struct RunSettings
    {
        public RunSettings(float baseLevel, float scaling, float restingBpm)
        {
            BaseLevel = baseLevel;
            Scaling = scaling;
            RestingBpm = restingBpm;
        }

        public float BaseLevel { get; }

        public float Scaling { get; }

        public float RestingBpm { get; }

        public float Level(float exertion)
        {
            return DifficultySettings.Level(BaseLevel, Scaling, exertion);
        }
    }
}
