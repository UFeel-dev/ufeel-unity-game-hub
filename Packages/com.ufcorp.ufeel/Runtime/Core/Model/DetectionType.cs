#nullable enable

using System;

namespace UFeel
{
    [Flags]
    public enum DetectionType
    {
        None = 0,
        Emotions = 1 << 0,
        EyeTracking = 1 << 1,
        SpeechToText = 1 << 2,
        HeartRate = 1 << 3,
    }
}
