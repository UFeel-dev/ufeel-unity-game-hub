#nullable enable

namespace UFeel
{
    public sealed class EyeTrackingCalibrationResult
    {
        private EyeTrackingCalibrationResult(bool succeeded, bool cancelled, bool profileSaved, EyeTrackingCalibrationDiagnostics? diagnostics, string message)
        {
            Succeeded = succeeded;
            Cancelled = cancelled;
            ProfileSaved = profileSaved;
            Diagnostics = diagnostics;
            Message = message;
        }

        public bool Succeeded { get; }
        public bool Cancelled { get; }
        public bool ProfileSaved { get; }

        /// <summary>Gets the diagnostics of a failed native fit, null otherwise.</summary>
        public EyeTrackingCalibrationDiagnostics? Diagnostics { get; }

        public string Message { get; }

        internal static EyeTrackingCalibrationResult Success(bool profileSaved, bool hasProfileStore)
        {
            string message;
            if (!hasProfileStore)
                message = "Eye tracking calibrated";
            else if (profileSaved)
                message = "Eye tracking calibrated and profile saved";
            else
                message = "Eye tracking calibrated, but the profile could not be saved";

            return new EyeTrackingCalibrationResult(true, false, profileSaved, null, message);
        }

        internal static EyeTrackingCalibrationResult Failure(EyeTrackingCalibrationDiagnostics diagnostics)
        {
            return new EyeTrackingCalibrationResult(false, false, false, diagnostics, UFeelText.EyeTrackingCalibrationFailure(diagnostics));
        }

        internal static EyeTrackingCalibrationResult Cancel()
        {
            return new EyeTrackingCalibrationResult(false, true, false, null, "Eye tracking calibration cancelled");
        }
    }
}
