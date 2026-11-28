#nullable enable

namespace UFeel
{
    public sealed class EmotionCalibrationResult
    {
        private EmotionCalibrationResult(
            bool succeeded,
            bool cancelled,
            bool profileSaved,
            EmotionCalibrationDiagnostics? diagnostics,
            EmotionType? hardestEmotion,
            string message)
        {
            Succeeded = succeeded;
            Cancelled = cancelled;
            ProfileSaved = profileSaved;
            Diagnostics = diagnostics;
            HardestEmotion = hardestEmotion;
            Message = message;
        }

        public bool Succeeded { get; }
        public bool Cancelled { get; }
        public bool ProfileSaved { get; }
        public EmotionCalibrationDiagnostics? Diagnostics { get; }

        /// <summary>Gets the emotion whose expression was hard to tell apart, null otherwise.</summary>
        public EmotionType? HardestEmotion { get; }

        public string Message { get; }

        internal static EmotionCalibrationResult Success(
            bool profileSaved,
            bool hasProfileStore,
            EmotionCalibrationDiagnostics diagnostics,
            EmotionType? hardestEmotion)
        {
            string message;
            if (!hasProfileStore)
                message = "Emotions calibrated";
            else if (profileSaved)
                message = "Emotions calibrated and profile saved";
            else
                message = "Emotions calibrated, but the profile could not be saved";
            if (hardestEmotion.HasValue)
                message += ". " + UFeelText.EmotionCalibrationHint(hardestEmotion.Value);

            return new EmotionCalibrationResult(true, false, profileSaved, diagnostics, hardestEmotion, message);
        }

        internal static EmotionCalibrationResult Failure(EmotionCalibrationDiagnostics diagnostics)
        {
            return new EmotionCalibrationResult(
                false, false, false, diagnostics, null, UFeelText.EmotionCalibrationFailure(diagnostics));
        }

        internal static EmotionCalibrationResult Cancel()
        {
            return new EmotionCalibrationResult(false, true, false, null, null, "Emotion calibration cancelled");
        }
    }
}
