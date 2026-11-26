using System;
using TMPro;
using UFeel;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class CalibrationScreen : ICalibrationPresenter
    {
        private const float ScrimOpacity = 0.94f;
        private const float RingSize = 64f;
        private const float DiscSize = 40f;
        private const float CoreSize = 10f;
        private const float PulseSpeed = 4f;
        private const float ResultSeconds = 6f;
        private const string Hint = "Eye tracking is not calibrated: press <b>F8</b>";
        private const string Unavailable = "UFeel is unavailable";
        private const string Failed = "Calibration failed, see the console";

        private static readonly KeyHint[] Hints = { new("Cancel", 0, "Esc") };

        private readonly RectTransform root;
        private readonly RectTransform target;
        private readonly Image ring;
        private readonly TextMeshProUGUI instruction;
        private readonly TextMeshProUGUI status;
        private readonly Toast toast;

        private Vector2 targetPosition = new(0.5f, 0.5f);
        private string instructionText = string.Empty;
        private string statusText = string.Empty;
        private bool visible;
        private bool shown;
        private bool hinting;
        private float resultRemaining;
        private float time;

        public CalibrationScreen(UiKit kit, Transform parent, Toast toast)
        {
            this.toast = toast;
            root = UiKit.NewRect("Calibration", parent);
            UiLayout.Stretch(root);

            Image scrim = kit.NewSolid("Scrim", root, Theme.Fade(Theme.Backdrop, ScrimOpacity));
            UiLayout.Stretch(scrim.rectTransform);

            Vector2 center = new(0.5f, 0.5f);
            TextMeshProUGUI title = kit.NewText("Title", root, Theme.Title, true, TextAlignmentOptions.Center, Theme.Text);
            UiLayout.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -Theme.Margin), new Vector2(1200f, 60f));
            title.text = "Eye tracking calibration";

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

            root.gameObject.SetActive(false);
        }

        void ICalibrationPresenter.Show() => visible = true;

        void ICalibrationPresenter.Hide() => visible = false;

        void ICalibrationPresenter.ShowTarget(float x, float y) => targetPosition = new Vector2(x, y);

        void ICalibrationPresenter.ShowInstruction(string text) => instructionText = text;

        void ICalibrationPresenter.ShowStatus(string text) => statusText = text;

        public async void Begin()
        {
            if (UFeelAPI.EyeTracking.IsCalibrating)
                return;

            if (!UFeelAPI.IsAvailable)
            {
                ShowResult(Unavailable);
                return;
            }

            try
            {
                CalibrationResult outcome = await UFeelAPI.EyeTracking.CalibrateAsync(this);
                ShowResult(outcome.Succeeded || outcome.Cancelled
                    ? outcome.Message
                    : UFeelText.FailureReason(outcome.Diagnostics?.FailureReason ?? CalibrationFailureReason.None));
            }
            catch (Exception)
            {
                ShowResult(Failed);
                throw;
            }
        }

        public void Tick(float deltaTime)
        {
            if (visible != shown)
                SetShown(visible);

            if (shown)
            {
                if (LayoutInput.GetKeyDown(KeyCode.Escape))
                    UFeelAPI.EyeTracking.CancelCalibration();

                time += deltaTime;
                float pulse = 0.5f + (0.5f * Mathf.Sin(time * PulseSpeed));
                UiLayout.Place(target, new Vector2(targetPosition.x, 1f - targetPosition.y), Vector2.zero, Vector2.one * RingSize);
                target.localScale = Vector3.one * (1f + (0.12f * pulse));
                ring.color = Theme.Fade(Theme.Accent, 0.6f + (0.4f * pulse));
                instruction.text = instructionText;
                status.text = statusText;
            }

            resultRemaining -= deltaTime;
            UpdateHint();
        }

        private void SetShown(bool value)
        {
            shown = value;
            root.gameObject.SetActive(value);
            if (value)
                UiFocus.Capture(this);
            else
                UiFocus.Release(this);
        }

        private void ShowResult(string message)
        {
            hinting = false;
            resultRemaining = ResultSeconds;
            toast.Show(message, ResultSeconds);
        }

        private void UpdateHint()
        {
            if (resultRemaining > 0f)
                return;

            bool wanted = !shown && UFeelAPI.IsAvailable && !UFeelAPI.EyeTracking.IsCalibrated && UFeelAPI.EyeTracking.IsInUse;
            if (wanted == hinting)
                return;

            hinting = wanted;
            if (wanted)
                toast.Show(Hint, float.PositiveInfinity);
            else
                toast.Hide();
        }
    }
}
