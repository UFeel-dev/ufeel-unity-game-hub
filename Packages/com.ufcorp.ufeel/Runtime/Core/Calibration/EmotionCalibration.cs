#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UFeel
{
    public sealed class EmotionCalibration
    {
        public static readonly EmotionType[] Emotions =
        {
            EmotionType.Neutral,
            EmotionType.Happy,
            EmotionType.Sad,
            EmotionType.Angry,
            EmotionType.Surprised,
            EmotionType.Fearful,
            EmotionType.Disgusted,
        };

        private const float HardestEmotionAccuracy = 0.5F;

        private static readonly TimeSpan SteadyFaceDuration = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan ReadinessPoll = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan EmotionDuration = TimeSpan.FromSeconds(8);
        private static readonly TimeSpan MaxEmotionDuration = TimeSpan.FromSeconds(12);
        private static readonly TimeSpan ProgressPoll = TimeSpan.FromMilliseconds(200);

        private readonly UFeelSession session;
        private readonly IEmotionCalibrationPresenter presenter;
        private readonly EmotionCalibrationProfileStore? profiles;
        private int running;

        public EmotionCalibration(
            UFeelSession session,
            IEmotionCalibrationPresenter presenter,
            EmotionCalibrationProfileStore? profiles = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            this.profiles = profiles;
        }

        public bool IsRunning => Volatile.Read(ref running) != 0;

        /// <summary>Cancelling or failing keeps the previously active emotion calibration.</summary>
        public async Task<EmotionCalibrationResult> RunAsync(CancellationToken token)
        {
            if (Interlocked.Exchange(ref running, 1) != 0)
                throw new InvalidOperationException("Emotion calibration is already running.");

            bool nativeActive = false;

            session.Demand.Pin(DetectionType.Emotions);
            session.Scheduler.SetOverride(DetectionType.Emotions, TimeSpan.Zero);

            try
            {
                EmotionCalibrationDiagnostics? unsupported = await session.ExecuteAsync(
                    p => p.BeginEmotionCalibration() ? null : p.GetEmotionCalibrationDiagnostics());
                if (unsupported != null)
                    return EmotionCalibrationResult.Failure(unsupported);
                nativeActive = true;

                presenter.Show();
                presenter.ShowEmotion(Emotions[0]);
                presenter.ShowProgress(0F);
                await WaitForSteadyFaceAsync(token);

                for (int i = 0; i < Emotions.Length; i++)
                    await CollectEmotionAsync(i, token);

                EmotionCalibrationResult result = await session.ExecuteAsync(Finish);
                nativeActive = false;
                return result;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                return EmotionCalibrationResult.Cancel();
            }
            finally
            {
                session.Scheduler.ClearOverride(DetectionType.Emotions);
                session.Demand.Unpin(DetectionType.Emotions);

                if (nativeActive)
                    await CancelNativeCalibrationAsync();

                presenter.Hide();
                Volatile.Write(ref running, 0);
            }
        }

        private async Task WaitForSteadyFaceAsync(CancellationToken token)
        {
            long? steadySince = null;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (session.Latest.Emotions.Status == EmotionStatus.Calibrating)
                {
                    steadySince ??= Clock.Now;
                    presenter.ShowInstruction("Face detected. Get ready to act each emotion");
                    presenter.ShowStatus("Hold still before emotion calibration");

                    if (Clock.Since(steadySince.Value) >= SteadyFaceDuration)
                        return;
                }
                else
                {
                    steadySince = null;
                    presenter.ShowInstruction("Position your face in the camera view");
                    presenter.ShowStatus("Waiting for a face");
                }

                await Task.Delay(ReadinessPoll, token);
            }
        }

        private async Task CollectEmotionAsync(int index, CancellationToken token)
        {
            EmotionType emotion = Emotions[index];
            string position = $"{index + 1}/{Emotions.Length}";

            presenter.ShowEmotion(emotion);
            presenter.ShowProgress(0F);
            presenter.ShowInstruction(UFeelText.EmotionCalibrationTip(emotion));
            await session.ExecuteAsync(p => p.SetEmotionCalibrationTarget(emotion));

            long start = Clock.Now;
            while (true)
            {
                EmotionCalibrationProgress progress = await session.ExecuteAsync(p => p.GetEmotionCalibrationProgress());
                TimeSpan elapsed = Clock.Since(start);

                presenter.ShowProgress((float)Math.Min(1.0, elapsed.TotalSeconds / EmotionDuration.TotalSeconds));
                presenter.ShowStatus(
                    $"Emotion {position}: {progress.SamplesForCurrentTarget}/{progress.RequiredSamplesPerTarget} usable frames");

                bool held = elapsed >= EmotionDuration && progress.CurrentTargetComplete;
                if (held || elapsed >= MaxEmotionDuration)
                    return;

                await Task.Delay(ProgressPoll, token);
            }
        }

        private EmotionCalibrationResult Finish(UFeelProcessor processor)
        {
            if (!processor.FinishEmotionCalibration())
                return EmotionCalibrationResult.Failure(processor.GetEmotionCalibrationDiagnostics());

            EmotionCalibrationDiagnostics diagnostics = processor.GetEmotionCalibrationDiagnostics();
            bool saved = false;
            if (profiles != null)
            {
                try
                {
                    saved = profiles.Save(processor);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    saved = false;
                }
            }

            return EmotionCalibrationResult.Success(saved, profiles != null, diagnostics, HardestEmotion(diagnostics));
        }

        private static EmotionType? HardestEmotion(EmotionCalibrationDiagnostics diagnostics)
        {
            int? weakest = diagnostics.WeakestTarget;
            if (!weakest.HasValue || weakest.Value >= Emotions.Length || diagnostics.WeakestTargetAccuracy >= HardestEmotionAccuracy)
                return null;

            return Emotions[weakest.Value];
        }

        private async Task CancelNativeCalibrationAsync()
        {
            try
            {
                await session.ExecuteAsync(p => p.CancelEmotionCalibration());
            }
            catch (Exception exception) when (exception is UFeelException or OperationCanceledException)
            {
            }
        }
    }
}
