using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class RunResult
    {
        private readonly int[] counts = new int[5];

        public RunResult(RhythmSong song, ScoreKeeper score, HeartRateStats heart, RunSettings settings, bool failed)
        {
            Song = song;
            Score = score.Score;
            Accuracy = score.Accuracy;
            MaxCombo = score.MaxCombo;
            Failed = failed;
            Grade = GradeCalculator.From(score.Accuracy, failed);
            BaseLevel = settings.BaseLevel;
            Scaling = settings.Scaling;
            RestingBpm = settings.RestingBpm;
            HasHeartRate = heart.HasReadings;
            AverageBpm = heart.AverageBpm;
            PeakBpm = heart.PeakBpm;
            BpmSeries = heart.BpmSeries;
            LevelSeries = heart.LevelSeries;
            for (int i = 0; i < counts.Length; i++)
                counts[i] = score.Count((Judgement)i);
        }

        public RhythmSong Song { get; }

        public long Score { get; }

        public double Accuracy { get; }

        public int MaxCombo { get; }

        public bool Failed { get; }

        public Grade Grade { get; }

        public float BaseLevel { get; }

        public float Scaling { get; }

        public float RestingBpm { get; }

        public bool HasHeartRate { get; }

        public float AverageBpm { get; }

        public float PeakBpm { get; }

        public IReadOnlyList<float> BpmSeries { get; }

        public IReadOnlyList<float> LevelSeries { get; }

        public int Count(Judgement judgement)
        {
            return counts[(int)judgement];
        }
    }
}
