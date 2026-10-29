using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace UFeel
{
    public sealed class UFeelProcessor : IDisposable
    {
        private static IntPtr handle;
        private static bool disposed;

        private static readonly Lazy<UFeelProcessor> _instance = new(CreateInstance);

        public static UFeelProcessor Instance => _instance.Value;

        private UFeelProcessor(
            string modelDirectory,
            string speechModelPath)
        {
            ValidatePath(modelDirectory, nameof(modelDirectory));
            ValidatePath(speechModelPath, nameof(speechModelPath));

            handle = UFeelNative.ufeel_create(modelDirectory, speechModelPath);

            if (handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "Failed to create the native UFeel processor.");
            }
        }

        private static UFeelProcessor CreateInstance()
        {
            string projectRoot = Directory.GetParent(
                UnityEngine.Application.dataPath)?.FullName
                ?? throw new InvalidOperationException(
                    "Cannot determine the Unity project directory.");

            string packageDirectory = Path.Combine(
                projectRoot,
                "Packages",
                "com.ufcorp.ufeel");

            if (!Directory.Exists(packageDirectory))
            {
                string cacheDirectory = Path.Combine(
                    projectRoot,
                    "Library",
                    "PackageCache");

                if (Directory.Exists(cacheDirectory))
                {
                    string[] cachedPackages = Directory.GetDirectories(
                        cacheDirectory,
                        "com.ufcorp.ufeel@*");

                    packageDirectory = cachedPackages.Length > 0
                        ? cachedPackages[0]
                        : throw new DirectoryNotFoundException(
                            $"UFeel package not found in: {cacheDirectory}");
                }
                else
                {
                    throw new DirectoryNotFoundException(
                        $"Package directory not found: {packageDirectory}");
                }
            }

            string pluginsDirectory = Path.Combine(
                packageDirectory,
                "Runtime",
                "Plugins");

            string pluginsDirectorySystem = Path.Combine(
                pluginsDirectory,
                "Linux-x86_64");

            string modelDirectory = Path.Combine(
                pluginsDirectorySystem,
                "models");

            string speechModelPath = Path.Combine(
                modelDirectory,
                "vosk-model-small-fr-0.22");

            if (!Directory.Exists(modelDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"Model directory not found: {modelDirectory}");
            }

            if (!Directory.Exists(speechModelPath))
            {
                throw new DirectoryNotFoundException(
                    $"Vosk model directory not found: {speechModelPath}");
            }

            string localDataDirectory = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrWhiteSpace(localDataDirectory))
            {
                throw new InvalidOperationException(
                    "Cannot determine the per-user application data directory for the gaze profile.");
            }

            return new UFeelProcessor(
                modelDirectory,
                speechModelPath);
        }

        public static bool Update() =>
            UFeelNative.ufeel_update(Handle) != 0;

        public static FrameData? GetFrame()
        {
            IntPtr pointer = UFeelNative.ufeel_get_frame(Handle);
            if (pointer == IntPtr.Zero)
                return null;

            try
            {
                UFeelNative.NativeFrame frame =
                    Marshal.PtrToStructure<UFeelNative.NativeFrame>(pointer);

                if (frame.Data == IntPtr.Zero ||
                    frame.Width <= 0 ||
                    frame.Height <= 0 ||
                    frame.Channels <= 0 ||
                    frame.Stride <= 0)
                {
                    return null;
                }

                int byteCount = checked(frame.Stride * frame.Height);
                byte[] data = new byte[byteCount];
                Marshal.Copy(frame.Data, data, 0, byteCount);

                return new FrameData(
                    data,
                    frame.Width,
                    frame.Height,
                    frame.Channels,
                    frame.Stride);
            }
            finally
            {
                UFeelNative.ufeel_free_frame(pointer);
            }
        }

        public static Dictionary<string, float> GetEmotions()
        {
            IntPtr pointer = UFeelNative.ufeel_get_emotions(
                Handle, out int count);

            if (pointer == IntPtr.Zero || count <= 0)
            {
                if (pointer != IntPtr.Zero)
                    UFeelNative.ufeel_free_emotions(pointer, count);

                return new Dictionary<string, float>();
            }

            try
            {
                Dictionary<string, float> emotions = new(count);
                int itemSize = Marshal.SizeOf<UFeelNative.NativeEmotion>();

                for (int i = 0; i < count; i++)
                {
                    IntPtr itemPointer = IntPtr.Add(
                        pointer, checked(i * itemSize));

                    UFeelNative.NativeEmotion item =
                        Marshal.PtrToStructure<UFeelNative.NativeEmotion>(
                            itemPointer);

                    string name =
                        Marshal.PtrToStringUTF8(item.Name) ?? string.Empty;

                    emotions[name] = item.Value;
                }

                return emotions;
            }
            finally
            {
                UFeelNative.ufeel_free_emotions(pointer, count);
            }
        }

        public static GazeResultData GetGaze()
        {
            UFeelNative.Status status =
                (UFeelNative.Status)UFeelNative.ufeel_get_gaze_result(
                    Handle, out UFeelNative.NativeGazeResult result);

            if (status is not UFeelNative.Status.Ok and
                not UFeelNative.Status.NoGaze)
            {
                ThrowIfFailed(status, "retrieve the gaze estimate");
            }

            return new GazeResultData(
                (GazeStatus)result.Status,
                result.X,
                result.Y,
                result.RecordedX,
                result.RecordedY,
                result.Confidence,
                (GazeDirection)result.Directions);
        }

        public static void BeginEyeTrackingCalibration()
        {
            ThrowIfFailed(
                (UFeelNative.Status)UFeelNative.ufeel_begin_eye_tracking_calibration(Handle),
                "begin eye-tracking calibration");
        }

        public static void SetEyeTrackingCalibrationTarget(float screenX, float screenY)
        {
            ThrowIfFailed(
                (UFeelNative.Status)UFeelNative.ufeel_set_eye_tracking_calibration_target(
                        Handle, screenX, screenY),
                "set the eye-tracking calibration target");
        }

        public static bool FinishEyeTrackingCalibration()
        {
            UFeelNative.Status status =
                (UFeelNative.Status)UFeelNative.ufeel_finish_eye_tracking_calibration(Handle);

            if (status == UFeelNative.Status.Ok)
                return true;

            if (status == UFeelNative.Status.EyeTrackingCalibrationFailed)
                return false;

            ThrowIfFailed(status, "finish eye-tracking calibration");
            return false; // Unreachable; satisfies definite-return analysis.
        }

        public static void CancelEyeTrackingCalibration()
        {
            ThrowIfFailed(
                (UFeelNative.Status)UFeelNative.ufeel_cancel_eye_tracking_calibration(Handle),
                "cancel eye-tracking calibration");
        }

        public static EyeTrackingCalibrationProgress
            GetEyeTrackingCalibrationProgress()
        {
            ThrowIfFailed(
                (UFeelNative.Status)UFeelNative.ufeel_get_eye_tracking_calibration_progress(
                        Handle,
                        out UFeelNative.NativeEyeTrackingCalibrationProgress progress),
                "retrieve eye-tracking calibration progress");

            return new EyeTrackingCalibrationProgress(
                progress.TargetsWithEnoughSamples,
                progress.TargetCount,
                progress.SamplesForCurrentTarget,
                progress.RequiredSamplesPerTarget);
        }

        public static EyeTrackingCalibrationDiagnostics
            GetEyeTrackingCalibrationDiagnostics()
        {
            ThrowIfFailed(
                (UFeelNative.Status)UFeelNative.ufeel_get_eye_tracking_calibration_diagnostics(
                        Handle,
                        out UFeelNative.NativeEyeTrackingCalibrationDiagnostics diagnostics),
                "retrieve eye-tracking calibration diagnostics");

            return new EyeTrackingCalibrationDiagnostics(
                (CalibrationFailureReason)diagnostics.FailureReason,
                diagnostics.InputTargetCount,
                diagnostics.TargetsWithEnoughRawSamples,
                diagnostics.TargetsWithEnoughRetainedSamples,
                diagnostics.ValidTargetCount,
                diagnostics.MinimumRetainedSamples,
                diagnostics.MaximumRetainedSamples,
                diagnostics.HorizontalSpan,
                diagnostics.VerticalSpan,
                diagnostics.MeanCrossValidationError,
                diagnostics.MaximumCrossValidationError,
                diagnostics.CrossValidationTargetsEvaluated,
                diagnostics.MeanErrorExceeded != 0,
                diagnostics.MaximumErrorExceeded != 0,
                diagnostics.RegressionFailures,
                diagnostics.InvalidTargetCount,
                diagnostics.TargetsWithInsufficientRawSamples,
                diagnostics.TargetsWithInsufficientRetainedSamples);
        }

        public static bool SaveEyeTrackingCalibrationProfile(string path)
        {
            ValidatePath(path, nameof(path));

            UFeelNative.Status status =
                (UFeelNative.Status)UFeelNative.ufeel_save_eye_tracking_calibration_profile(
                        Handle, path);

            if (status == UFeelNative.Status.Ok)
                return true;

            if (status == UFeelNative.Status.EyeTrackingProfileFailed)
                return false;

            ThrowIfFailed(status, "save the eye-tracking calibration profile");
            return false;
        }

        public static bool LoadEyeTrackingCalibrationProfile(string path)
        {
            ValidatePath(path, nameof(path));

            UFeelNative.Status status =
                (UFeelNative.Status)UFeelNative.ufeel_load_eye_tracking_calibration_profile(
                        Handle, path);

            if (status == UFeelNative.Status.Ok)
                return true;

            if (status == UFeelNative.Status.EyeTrackingProfileFailed)
                return false;

            ThrowIfFailed(status, "load the eye-tracking calibration profile");
            return false;
        }

        public static void SetSpeechToTextEnabled(bool enabled)
        {
            ThrowIfFailed(
                (UFeelNative.Status)UFeelNative.ufeel_set_speech_to_text_enabled(
                        Handle, enabled ? 1 : 0),
                "change speech-to-text state");
        }

        public static string GetSpeechToText()
        {
            IntPtr pointer = UFeelNative.ufeel_get_speech_to_text(Handle);
            if (pointer == IntPtr.Zero)
                return string.Empty;

            try
            {
                return Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
            }
            finally
            {
                UFeelNative.ufeel_free_string(pointer);
            }
        }

        public static HeartRateResultData GetHeartRate()
        {
            UFeelNative.Status status =
                (UFeelNative.Status)UFeelNative.ufeel_get_heartrate(
                    Handle, out UFeelNative.NativeHeartRateResult result);

            if (status is not UFeelNative.Status.Ok and
                not UFeelNative.Status.NoHeartRate)
            {
                ThrowIfFailed(status, "retrieve the heartrate value");
            }

            return new HeartRateResultData(
                (HeartRateStatus)result.Status,
                result.Bpm);
        }

        private static IntPtr Handle
        {
            get
            {
                // Déclenche l'initialisation lors du premier appel.
                _ = Instance;

                if (disposed || handle == IntPtr.Zero)
                {
                    throw new ObjectDisposedException(
                        nameof(UFeelProcessor));
                }

                return handle;
            }
        }

        private static void ValidatePath(string path, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "A non-empty path is required.", parameterName);
            }
        }

        private static void ThrowIfFailed(
            UFeelNative.Status status, string operation)
        {
            if (status == UFeelNative.Status.Ok)
                return;

            string message = status switch
            {
                UFeelNative.Status.NoGaze =>
                    "A valid gaze estimate is not available.",
                UFeelNative.Status.NoHeartRate =>
                    "A valid heartrate is not available.",
                UFeelNative.Status.InvalidArgument =>
                    "One or more arguments are invalid.",
                UFeelNative.Status.InvalidState =>
                    "The processor is not in a state that permits this operation.",
                UFeelNative.Status.EyeTrackingCalibrationFailed =>
                    "Eye-tracking calibration failed.",
                UFeelNative.Status.EyeTrackingProfileFailed =>
                    "The eye-tracking calibration profile operation failed.",
                UFeelNative.Status.InternalError =>
                    "An internal native-library error occurred.",
                _ => "An unknown native-library error occurred."
            };

            throw new InvalidOperationException(
                $"Failed to {operation}: {message} (status {(int)status}).");
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            IntPtr nativeHandle = handle;
            handle = IntPtr.Zero;

            if (nativeHandle != IntPtr.Zero)
            {
                UFeelNative.ufeel_destroy(nativeHandle);
            }

            GC.SuppressFinalize(this);
        }

        public static void Shutdown()
        {
            if (!_instance.IsValueCreated)
                return;

            _instance.Value.Dispose();
        }
    }



    public sealed class FrameData
    {
        private readonly byte[] data;

        public byte[] GetData()
        {
            return this.data;
        }

        public int Width { get; }
        public int Height { get; }
        public int Channels { get; }
        public int Stride { get; }

        public FrameData(
            byte[] data, int width, int height, int channels, int stride)
        {
            this.data = data;
            Width = width;
            Height = height;
            Channels = channels;
            Stride = stride;
        }
    }

    public sealed class GazeResultData
    {
        public GazeStatus Status { get; }
        public float EstimatedX { get; }
        public float EstimatedY { get; }
        public float RecordedX { get; }
        public float RecordedY { get; }
        public float Confidence { get; }
        public GazeDirection Directions { get; }

        public bool IsValid => Status == GazeStatus.Valid;

        public GazeResultData(
            GazeStatus status,
            float estimatedX,
            float estimatedY,
            float recordedX,
            float recordedY,
            float confidence,
            GazeDirection directions)
        {
            Status = status;
            EstimatedX = estimatedX;
            EstimatedY = estimatedY;
            RecordedX = recordedX;
            RecordedY = recordedY;
            Confidence = confidence;
            Directions = directions;
        }
    }

    public sealed class EyeTrackingCalibrationProgress
    {
        public int TargetsWithEnoughSamples { get; }
        public int TargetCount { get; }
        public int SamplesForCurrentTarget { get; }
        public int RequiredSamplesPerTarget { get; }

        public EyeTrackingCalibrationProgress(
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
    }

    public enum CalibrationFailureReason
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
        InvalidCrossValidationResult = 10
    }

    public sealed class EyeTrackingCalibrationDiagnostics
    {
        public CalibrationFailureReason FailureReason { get; }
        public int InputTargetCount { get; }
        public int TargetsWithEnoughRawSamples { get; }
        public int TargetsWithEnoughRetainedSamples { get; }
        public int ValidTargetCount { get; }
        public int MinimumRetainedSamples { get; }
        public int MaximumRetainedSamples { get; }
        public float HorizontalSpan { get; }
        public float VerticalSpan { get; }
        public float MeanCrossValidationError { get; }
        public float MaximumCrossValidationError { get; }
        public int CrossValidationTargetsEvaluated { get; }
        public bool MeanErrorExceeded { get; }
        public bool MaximumErrorExceeded { get; }
        public int RegressionFailures { get; }
        public int InvalidTargetCount { get; }
        public int TargetsWithInsufficientRawSamples { get; }
        public int TargetsWithInsufficientRetainedSamples { get; }

        public EyeTrackingCalibrationDiagnostics(
            CalibrationFailureReason failureReason,
            int inputTargetCount,
            int targetsWithEnoughRawSamples,
            int targetsWithEnoughRetainedSamples,
            int validTargetCount,
            int minimumRetainedSamples,
            int maximumRetainedSamples,
            float horizontalSpan,
            float verticalSpan,
            float meanCrossValidationError,
            float maximumCrossValidationError,
            int crossValidationTargetsEvaluated,
            bool meanErrorExceeded,
            bool maximumErrorExceeded,
            int regressionFailures,
            int invalidTargetCount,
            int targetsWithInsufficientRawSamples,
            int targetsWithInsufficientRetainedSamples)
        {
            FailureReason = failureReason;
            InputTargetCount = inputTargetCount;
            TargetsWithEnoughRawSamples = targetsWithEnoughRawSamples;
            TargetsWithEnoughRetainedSamples = targetsWithEnoughRetainedSamples;
            ValidTargetCount = validTargetCount;
            MinimumRetainedSamples = minimumRetainedSamples;
            MaximumRetainedSamples = maximumRetainedSamples;
            HorizontalSpan = horizontalSpan;
            VerticalSpan = verticalSpan;
            MeanCrossValidationError = meanCrossValidationError;
            MaximumCrossValidationError = maximumCrossValidationError;
            CrossValidationTargetsEvaluated = crossValidationTargetsEvaluated;
            MeanErrorExceeded = meanErrorExceeded;
            MaximumErrorExceeded = maximumErrorExceeded;
            RegressionFailures = regressionFailures;
            InvalidTargetCount = invalidTargetCount;
            TargetsWithInsufficientRawSamples = targetsWithInsufficientRawSamples;
            TargetsWithInsufficientRetainedSamples = targetsWithInsufficientRetainedSamples;
        }
    }

    public sealed class HeartRateResultData
    {
        public HeartRateStatus Status { get; }
        public uint Bpm { get; }

        public bool IsValid => Status == HeartRateStatus.Valid;

        public HeartRateResultData(
            HeartRateStatus status,
            uint bpm)
        {
            Status = status;
            Bpm = bpm;
        }
    }
}