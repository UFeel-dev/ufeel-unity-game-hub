#nullable enable

namespace UFeel
{
    public readonly struct EmotionCalibrationProgress
    {
        public EmotionCalibrationProgress(
            int targetsWithEnoughSamples,
            int targetCount,
            int samplesForCurrentTarget,
            int requiredSamplesPerTarget)
        {
            TargetsWithEnoughSamples = targetsWithEnoughSamples;
            TargetCount = targetCount;
            SamplesForCurrentTarget = samplesForCurrentTarget;
            RequiredSamplesPerTarget = requiredSamplesPerTarget;
        }

        public int TargetsWithEnoughSamples { get; }
        public int TargetCount { get; }
        public int SamplesForCurrentTarget { get; }
        public int RequiredSamplesPerTarget { get; }

        public bool CurrentTargetComplete => SamplesForCurrentTarget >= RequiredSamplesPerTarget;
    }
}
