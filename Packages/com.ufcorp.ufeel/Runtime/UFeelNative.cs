using System;
using System.Runtime.InteropServices;

namespace UFeel
{
    public enum EmotionType
    {
        None,
        Angry,
        Contemptuous,
        Disgusted,
        Fearful,
        Happy,
        Neutral,
        Sad,
        Surprised
    }

    public enum GazeStatus
    {
        NotCalibrated = 0,
        FaceUnavailable = 1,
        Blinking = 2,
        LowQuality = 3,
        Calibrating = 4,
        Valid = 5,
        Disabled = 6,
        FrameUnavailable = 7
    }

    [Flags]
    public enum GazeDirection
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3,
        Center = 1 << 4
    }

    public static class GazeDirectionExtensions
    {
        public static bool IsDirection(
            this GazeDirection value,
            GazeDirection flag)
        {
            return (value & flag) != GazeDirection.None;
        }
    }

    public enum HeartRateStatus
    {
        JsonParseError = 0,
        JsonParseOutOfRange = 1,
        RequestFailed = 2,
        Valid = 3,
    }

    internal static class UFeelNative
    {
        private const string LibraryName = "ufeel_wrapper";

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeFrame
        {
            public IntPtr Data;
            public int Width;
            public int Height;
            public int Channels;
            public int Stride;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeEmotion
        {
            public IntPtr Name;
            public float Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeGazeResult
        {
            public int Status;
            public float X;
            public float Y;
            public float RecordedX;
            public float RecordedY;
            public float Confidence;
            public int Directions;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeEyeTrackingCalibrationProgress
        {
            public int TargetsWithEnoughSamples;
            public int TargetCount;
            public int SamplesForCurrentTarget;
            public int RequiredSamplesPerTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeEyeTrackingCalibrationDiagnostics
        {
            public int FailureReason;

            public int InputTargetCount;
            public int TargetsWithEnoughRawSamples;
            public int TargetsWithEnoughRetainedSamples;
            public int ValidTargetCount;

            public int MinimumRetainedSamples;
            public int MaximumRetainedSamples;

            public float HorizontalSpan;
            public float VerticalSpan;

            public float MeanCrossValidationError;
            public float MaximumCrossValidationError;
            public int CrossValidationTargetsEvaluated;

            public int MeanErrorExceeded;
            public int MaximumErrorExceeded;

            public int RegressionFailures;
            public int InvalidTargetCount;
            public int TargetsWithInsufficientRawSamples;
            public int TargetsWithInsufficientRetainedSamples;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeHeartRateResult
        {
            public int Status;
            public uint Bpm;
        }

        internal enum Status
        {
            Ok = 0,
            NoGaze = 1,
            NoHeartRate = 2,
            InvalidArgument = 3,
            InvalidState = 4,
            EyeTrackingCalibrationFailed = 5,
            EyeTrackingProfileFailed = 6,
            InternalError = 7
        }

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ufeel_create(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string modelDirectory,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string speechModelPath);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ufeel_destroy(IntPtr processor);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_update(IntPtr processor);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ufeel_get_frame(IntPtr processor);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ufeel_free_frame(IntPtr frame);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ufeel_get_emotions(
            IntPtr processor,
            out int count);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ufeel_free_emotions(
            IntPtr emotions,
            int count);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_begin_eye_tracking_calibration(IntPtr processor);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_set_eye_tracking_calibration_target(
            IntPtr processor,
            float screenX,
            float screenY);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_finish_eye_tracking_calibration(IntPtr processor);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_cancel_eye_tracking_calibration(IntPtr processor);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_get_gaze_result(
            IntPtr processor,
            out NativeGazeResult result);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_get_eye_tracking_calibration_progress(
            IntPtr processor,
            out NativeEyeTrackingCalibrationProgress progress);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_get_eye_tracking_calibration_diagnostics(
            IntPtr processor,
            out NativeEyeTrackingCalibrationDiagnostics diagnostics);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_save_eye_tracking_calibration_profile(
            IntPtr processor,
            [MarshalAs(UnmanagedType.LPWStr)] string utf8Path);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_load_eye_tracking_calibration_profile(
            IntPtr processor,
            [MarshalAs(UnmanagedType.LPWStr)] string utf8Path);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ufeel_set_speech_to_text_enabled(
            IntPtr processor,
            int enabled);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ufeel_get_speech_to_text(IntPtr processor);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ufeel_free_string(IntPtr value);

        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ufeel_get_heartrate(
            IntPtr processor,
            out NativeHeartRateResult result);
    }
}
