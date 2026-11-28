using System;
using TMPro;
using UFeel;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class EyeTrackingCalibrationScreen : IEyeTrackingCalibrationPresenter
    {
        private const float ScrimOpacity = 0.94f;
        private const float RingSize = 64f;
        private const float DiscSize = 40f;
        private const float CoreSize = 10f;
        private const float PulseSpeed = 4f;
        private const string HintText = "Eye tracking is not calibrated: press <b>F8</b>";
        private const string Unavailable = "UFeel is unavailable";
        private const string Failed = "Eye-tracking calibration failed, see the console";

        private static readonly KeyHint[] Hints = { new("Cancel", 0, "Esc") };

        private readonly CalibrationOverlay overlay;
        private readonly RectTransform target;
        private readonly Image ring;
        private readonly TextMeshProUGUI instruction;
        private readonly TextMeshProUGUI status;
        private readonly CalibrationToast toast;

        private Vector2 targetPosition = new(0.5f, 0.5f);
        private string instructionText = string.Empty;
        private string statusText = string.Empty;
        private bool visible;
        private float time;

        public EyeTrackingCalibrationScreen(UiKit kit, Transform parent, CalibrationToast toast)
        {
            this.toast = toast;
            RectTransform root = UiKit.NewRect("EyeTrackingCalibration", parent);
            UiLayout.Stretch(root);

            Image scrim = kit.NewSolid("Scrim", root, Theme.Fade(Theme.Backdrop, ScrimOpacity));
            UiLayout.Stretch(scrim.rectTransform);

            Vector2 center = new(0.5f, 0.5f);
            TextMeshProUGUI title = kit.NewText("Title", root, Theme.Title, true, TextAlignmentOptions.Center, Theme.Text);
            UiLayout.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -Theme.Margin), new Vector2(1200f, 60f));
            title.text = "Eye-tracking calibration";

            instruction = kit.NewText("Instruction", root, Theme.Body, false, TextAlignmentOptions.Center, Theme.Text);
            UiLayout.Place(instruction.rectTransform, center, new Vector2(0f, -110f), new Vector2(1400f, 40f));
            status = kit.NewText("Status", root, Theme.Small, false, TextAlignmentOptions.Center, Theme.TextDim);
            UiLayout.Place(status.rectTransform, center, new Vector2(0f, -156f), new Vector2(1400f, 32f));

            target = UiKit.NewRect("Target", root);
            target.sizeDelta = Vector2.one * RingSize;
            ring = UiKit.NewImage("Ring", target, kit.Sprites.Ring, Theme.Accent);
            UiLayout.Stretch(ring.rectTransform);
            Image disc = UiKit.NewImage("Disc", target, kit.Sprites.Circle, Theme.Fade(Theme.Accent, 0.3f));
            UiLayout.Place(disc.rectTransform, center, Vector2.zero, Vector2.one * DiscSize);
            Image core = UiKit.NewImage("Core", target, kit.Sprites.Circle, Theme.Text);
            UiLayout.Place(core.rectTransform, center, Vector2.zero, Vector2.one * CoreSize);

            KeyBar keys = new(kit, root);
            keys.Set(Hints);

            overlay = new CalibrationOverlay(root.gameObject);
        }

        public string? Hint =>
            !overlay.IsShown && UFeelAPI.IsAvailable && !UFeelAPI.EyeTracking.IsCalibrated && UFeelAPI.EyeTracking.IsInUse
                ? HintText
                : null;

        void IEyeTrackingCalibrationPresenter.Show() => visible = true;

        void IEyeTrackingCalibrationPresenter.Hide() => visible = false;

        void IEyeTrackingCalibrationPresenter.ShowTarget(float x, float y) => targetPosition = new Vector2(x, y);

        void IEyeTrackingCalibrationPresenter.ShowInstruction(string text) => instructionText = text;

        void IEyeTrackingCalibrationPresenter.ShowStatus(string text) => statusText = text;

        public async void Begin()
        {
            if (UFeelAPI.EyeTracking.IsCalibrating)
                return;

            if (!UFeelAPI.IsAvailable)
            {
                toast.ShowMessage(Unavailable);
                return;
            }

            try
            {
                EyeTrackingCalibrationResult outcome = await UFeelAPI.EyeTracking.CalibrateAsync(this);
                toast.ShowMessage(outcome.Succeeded || outcome.Cancelled
                    ? outcome.Message
                    : UFeelText.EyeTrackingCalibrationFailure(outcome.Diagnostics?.FailureReason ?? EyeTrackingCalibrationFailureReason.None));
            }
            catch (Exception)
            {
                toast.ShowMessage(Failed);
                throw;
            }
        }

        public void Tick(float deltaTime)
        {
            overlay.SetShown(visible);
            if (!overlay.IsShown)
                return;

            if (MenuKeys.Back())
            {
                GameSounds.Play(SoundCue.MenuBack);
                UFeelAPI.EyeTracking.CancelCalibration();
            }

            time += deltaTime;
            float pulse = 0.5f + (0.5f * Mathf.Sin(time * PulseSpeed));
            UiLayout.Place(target, new Vector2(targetPosition.x, 1f - targetPosition.y), Vector2.zero, Vector2.one * RingSize);
            target.localScale = Vector3.one * (1f + (0.12f * pulse));
            ring.color = Theme.Fade(Theme.Accent, 0.6f + (0.4f * pulse));
            instruction.text = instructionText;
            status.text = statusText;
        }
    }
}
