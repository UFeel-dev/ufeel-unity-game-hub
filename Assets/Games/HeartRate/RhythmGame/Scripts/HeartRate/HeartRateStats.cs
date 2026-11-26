using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class HeartRateStats
    {
        private const float SampleInterval = 1f;

        private readonly List<float> bpmSeries = new(256);
        private readonly List<float> levelSeries = new(256);

        private double weightedBpm;
        private double readingTime;
        private float sampleTimer;

        public bool HasReadings => readingTime > 0d;

        public float AverageBpm => HasReadings ? (float)(weightedBpm / readingTime) : 0f;

        public float PeakBpm { get; private set; }

        public IReadOnlyList<float> BpmSeries => bpmSeries;

        public IReadOnlyList<float> LevelSeries => levelSeries;

        public void Record(float deltaTime, float bpm, bool hasReading, float level)
        {
            if (hasReading)
            {
                readingTime += deltaTime;
                weightedBpm += bpm * deltaTime;
                PeakBpm = Math.Max(PeakBpm, bpm);
            }

            sampleTimer -= deltaTime;
            if (sampleTimer > 0f)
                return;

            sampleTimer += SampleInterval;
            bpmSeries.Add(hasReading ? bpm : float.NaN);
            levelSeries.Add(level);
        }
    }
}
