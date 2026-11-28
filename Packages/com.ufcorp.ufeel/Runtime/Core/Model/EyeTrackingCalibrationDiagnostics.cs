#nullable enable

namespace UFeel
{
    public sealed class EyeTrackingCalibrationDiagnostics
    {
        public EyeTrackingCalibrationFailureReason FailureReason { get; internal set; }
        public int InputTargetCount { get; internal set; }
        public int TargetsWithEnoughRawSamples { get; internal set; }
        public int TargetsWithEnoughRetainedSamples { get; internal set; }
        public int ValidTargetCount { get; internal set; }
        public int MinimumRetainedSamples { get; internal set; }
        public int MaximumRetainedSamples { get; internal set; }
        public float HorizontalSpan { get; internal set; }
        public float VerticalSpan { get; internal set; }
        public float MeanCrossValidationError { get; internal set; }
        public float MaximumCrossValidationError { get; internal set; }
        public int CrossValidationTargetsEvaluated { get; internal set; }
        public bool MeanErrorExceeded { get; internal set; }
        public bool MaximumErrorExceeded { get; internal set; }
        public int RegressionFailures { get; internal set; }
        public int InvalidTargetCount { get; internal set; }
        public int TargetsWithInsufficientRawSamples { get; internal set; }
        public int TargetsWithInsufficientRetainedSamples { get; internal set; }
    }

    public enum EyeTrackingCalibrationFailureReason
    {
        None = 0,
        NotCalibrating = 1,
        TooFewValidTargets = 2,
        InsufficientSamples = 3,
        InsufficientHorizontalCoverage = 4,
        InsufficientVerticalCoverage = 5,
        InsufficientSpatialGridCoverage = 6,
        RegressionFailed = 7,
        CrossValidationMeanErrorExceeded = 8,
        CrossValidationMaxErrorExceeded = 9,
        InvalidCrossValidationResult = 10,
    }
}
