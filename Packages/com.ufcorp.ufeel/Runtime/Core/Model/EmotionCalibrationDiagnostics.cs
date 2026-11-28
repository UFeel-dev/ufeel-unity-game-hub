#nullable enable

namespace UFeel
{
    public sealed class EmotionCalibrationDiagnostics
    {
        public EmotionCalibrationFailureReason FailureReason { get; internal set; }
        public int TargetCount { get; internal set; }
        public int TargetsWithEnoughSamples { get; internal set; }
        public int MinimumSamples { get; internal set; }
        public int MaximumSamples { get; internal set; }
        public float MeanAccuracy { get; internal set; }

        /// <summary>Gets the index of the emotion in the order the targets were set, null when unknown.</summary>
        public int? WeakestTarget { get; internal set; }

        public float WeakestTargetAccuracy { get; internal set; }
    }

    public enum EmotionCalibrationFailureReason
    {
        None = 0,
        NotCalibrating = 1,
        UnsupportedModel = 2,
        MissingEmotions = 3,
        InsufficientSamples = 4,
        SolveFailed = 5,
        IndistinctExpressions = 6,
    }
}
