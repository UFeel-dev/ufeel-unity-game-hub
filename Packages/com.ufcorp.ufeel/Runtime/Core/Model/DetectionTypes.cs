#nullable enable

using System;

namespace UFeel
{
    public static class DetectionTypes
    {
        public const DetectionType All =
            DetectionType.Emotions |
            DetectionType.EyeTracking |
            DetectionType.SpeechToText |
            DetectionType.HeartRate;

        /// <summary>Features that consume camera frames.</summary>
        public const DetectionType Image = DetectionType.Emotions | DetectionType.EyeTracking;

        public static readonly DetectionType[] Each =
        {
            DetectionType.Emotions,
            DetectionType.EyeTracking,
            DetectionType.SpeechToText,
            DetectionType.HeartRate,
        };

        public static int IndexOf(DetectionType feature)
        {
            for (int i = 0; i < Each.Length; i++)
            {
                if (Each[i] == feature)
                    return i;
            }

            throw new ArgumentOutOfRangeException(nameof(feature), feature, "Expected a single feature.");
        }

        public static bool Includes(this DetectionType value, DetectionType feature) => (value & feature) != 0;
    }
}
