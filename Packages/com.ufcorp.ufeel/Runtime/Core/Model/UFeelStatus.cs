#nullable enable

namespace UFeel
{
    public enum UFeelStatus
    {
        Ok = 0,
        NoEmotions = 1,
        NoEyeTracking = 2,
        NoSpeech = 3,
        NoHeartRate = 4,
        InvalidArgument = 5,
        InvalidState = 6,
        EyeTrackingCalibrationFailed = 7,
        EyeTrackingProfileFailed = 8,
        InternalError = 9,
        InvalidProcessor = 10,
        InvalidFrame = 11,
        UnsupportedFeature = 12,
        EmotionCalibrationFailed = 13,
        EmotionProfileFailed = 14,
    }
}
