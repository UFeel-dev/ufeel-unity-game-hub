using System;
using TMPro;
using UFeel;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class EmotionCalibrationScreen : IEmotionCalibrationPresenter
    {
        private const float ScrimOpacity = 0.94f;
        private const float FrameTop = 170f;
        private const float MaxFrameWidth = 640f;
        private const float MaxFrameHeight = 420f;
        private const float ProgressWidth = 640f;
        private const float ProgressHeight = 8f;
        private const string HintText = "Emotions use the general model: press <b>F7</b> to calibrate them to your face";
        private const string Unavailable = "UFeel is unavailable";
        private const string Failed = "Emotion calibration failed, see the console";
        private const string ResetDone = "Emotion calibration reset: emotions use the general model";
        private const string ResetKeptProfile = "Emotion calibration reset, but its profile could not be deleted";
        private const string ResetFailed = "Emotion calibration reset failed, see the console";

        private static readonly KeyHint[] Hints = { new("Cancel", 0, "Esc") };

        private readonly CalibrationOverlay overlay;
        private readonly RawImage frame;
        private readonly TextMeshProUGUI emotion;
        private readonly TextMeshProUGUI instruction;
        private readonly TextMeshProUGUI status;
        private readonly RectTransform progressFill;
        private readonly CalibrationToast toast;

        private Vector2Int frameSize;
        private string emotionText = string.Empty;
        private string instructionText = string.Empty;
        private string statusText = string.Empty;
        private float progress;
        private bool visible;
        private bool hinted;

        public EmotionCalibrationScreen(UiKit kit, Transform parent, CalibrationToast toast)
        {
            this.toast = toast;
            RectTransform root = UiKit.NewRect("EmotionCalibration", parent);
            UiLayout.Stretch(root);

            Image scrim = kit.NewSolid("Scrim", root, Theme.Fade(Theme.Backdrop, ScrimOpacity));
            UiLayout.Stretch(scrim.rectTransform);

            Vector2 top = new(0.5f, 1f);
            Vector2 center = new(0.5f, 0.5f);
            TextMeshProUGUI title = kit.NewText("Title", root, Theme.Title, true, TextAlignmentOptions.Center, Theme.Text);
            UiLayout.Place(title.rectTransform, top, top, new Vector2(0f, -Theme.Margin), new Vector2(1200f, 60f));
            title.text = "Emotion calibration";

            frame = UiKit.NewRect("Frame", root).gameObject.AddComponent<RawImage>();
            frame.raycastTarget = false;
            frame.uvRect = new Rect(0f, 1f, 1f, -1f);

            emotion = kit.NewText("Emotion", root, Theme.Display, true, TextAlignmentOptions.Center, Theme.Accent);
            UiLayout.Place(emotion.rectTransform, center, new Vector2(0f, -150f), new Vector2(1200f, 90f));
            instruction = kit.NewText("Instruction", root, Theme.Body, false, TextAlignmentOptions.Center, Theme.Text);
            UiLayout.Place(instruction.rectTransform, center, new Vector2(0f, -225f), new Vector2(1400f, 40f));

            Image track = kit.NewSolid("Progress", root, Theme.Line);
            UiLayout.Place(track.rectTransform, center, new Vector2(0f, -280f), new Vector2(ProgressWidth, ProgressHeight));
            progressFill = kit.NewSolid("Fill", track.rectTransform, Theme.Accent).rectTransform;
            UiLayout.Stretch(progressFill);

            status = kit.NewText("Status", root, Theme.Small, false, TextAlignmentOptions.Center, Theme.TextDim);
            UiLayout.Place(status.rectTransform, center, new Vector2(0f, -320f), new Vector2(1400f, 32f));

            KeyBar keys = new(kit, root);
            keys.Set(Hints);

            LayoutFrame();
            overlay = new CalibrationOverlay(root.gameObject);
        }

        public bool IsShown => overlay.IsShown;

        void IEmotionCalibrationPresenter.Show() => visible = true;

        void IEmotionCalibrationPresenter.Hide() => visible = false;

        void IEmotionCalibrationPresenter.ShowEmotion(EmotionType value) => emotionText = value.ToString();

        void IEmotionCalibrationPresenter.ShowProgress(float fraction) => progress = Mathf.Clamp01(fraction);

        void IEmotionCalibrationPresenter.ShowInstruction(string text) => instructionText = text;

        void IEmotionCalibrationPresenter.ShowStatus(string text) => statusText = text;

        public async void Begin()
        {
            if (UFeelAPI.Emotion.IsCalibrating)
                return;

            if (!UFeelAPI.IsAvailable)
            {
                toast.ShowMessage(Unavailable);
                return;
            }

            try
            {
                EmotionCalibrationResult outcome = await UFeelAPI.Emotion.CalibrateAsync(this);
                toast.ShowMessage(outcome.Succeeded || outcome.Cancelled
                    ? outcome.Message
                    : UFeelText.EmotionCalibrationFailure(outcome.Diagnostics?.FailureReason ?? EmotionCalibrationFailureReason.None));
            }
            catch (Exception)
            {
                toast.ShowMessage(Failed);
                throw;
            }
        }

        public async void Reset()
        {
            if (!UFeelAPI.IsAvailable)
            {
                toast.ShowMessage(Unavailable);
                return;
            }

            try
            {
                toast.ShowMessage(await UFeelAPI.Emotion.ResetCalibrationAsync() ? ResetDone : ResetKeptProfile);
            }
            catch (Exception)
            {
                toast.ShowMessage(ResetFailed);
                throw;
            }
        }

        public void Tick()
        {
            overlay.SetShown(visible);
            if (!overlay.IsShown)
            {
                ShowHintOnce();
                return;
            }

            if (MenuKeys.Back())
            {
                GameSounds.Play(SoundCue.MenuBack);
                UFeelAPI.Emotion.CancelCalibration();
            }

            Texture2D? texture = UFeelAPI.CameraFeed.Frame;
            frame.texture = texture;
            Vector2Int size = texture != null ? new Vector2Int(texture.width, texture.height) : Vector2Int.zero;
            if (size != frameSize)
            {
                frameSize = size;
                LayoutFrame();
            }

            emotion.text = emotionText;
            instruction.text = instructionText;
            status.text = statusText;
            progressFill.anchorMax = new Vector2(progress, 1f);
            progressFill.offsetMax = Vector2.zero;
        }

        private void ShowHintOnce()
        {
            if (hinted || toast.IsShowingMessage || !UFeelAPI.IsAvailable || UFeelAPI.Emotion.IsCalibrated || !UFeelAPI.Emotion.IsInUse)
                return;

            hinted = true;
            toast.ShowMessage(HintText);
        }

        private void LayoutFrame()
        {
            Vector2 size = Vector2.zero;
            if (frameSize.x > 0 && frameSize.y > 0)
            {
                float scale = Mathf.Min(MaxFrameWidth / frameSize.x, MaxFrameHeight / frameSize.y);
                size = new Vector2(frameSize.x * scale, frameSize.y * scale);
            }

            frame.enabled = size.x > 0f;
            Vector2 top = new(0.5f, 1f);
            UiLayout.Place(frame.rectTransform, top, top, new Vector2(0f, -FrameTop), size);
        }
    }
}
