#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UFeel
{
    internal sealed class UFeelCalibrationOverlay : MonoBehaviour, ICalibrationPresenter
    {
        private const float HintWindowSeconds = 1.0F;
        private const float ResultSeconds = 6.0F;
        private const float TargetSize = 40.0F;
        private const float ReferenceHeight = 1080.0F;

        private static readonly Color Backdrop = new(0.0F, 0.0F, 0.0F, 0.92F);
        private static readonly Color BannerBackdrop = new(0.0F, 0.0F, 0.0F, 0.7F);
        private static readonly Color TargetColor = new(1.0F, 0.45F, 0.1F);

        private Texture2D? dot;
        private GUIStyle? textStyle;
        private Task<CalibrationResult>? running;
        private CancellationTokenSource? cancellation;

        private bool visible;
        private Vector2 target = new(0.5F, 0.5F);
        private string instruction = string.Empty;
        private string status = string.Empty;
        private string? result;
        private float resultUntil;

        public bool IsRunning => running != null && !running.IsCompleted;

        public Task<CalibrationResult> Run(UFeelSession session, CalibrationProfileStore? profiles)
        {
            if (running != null && !running.IsCompleted)
                return running;

            cancellation = new CancellationTokenSource();
            running = RunAsync(new EyeTrackingCalibration(session, this, profiles), cancellation.Token);
            return running;
        }

        void ICalibrationPresenter.Show() => visible = true;

        void ICalibrationPresenter.Hide() => visible = false;

        void ICalibrationPresenter.ShowTarget(float x, float y) => target = new Vector2(x, y);

        void ICalibrationPresenter.ShowInstruction(string text) => instruction = text;

        void ICalibrationPresenter.ShowStatus(string text) => status = text;

        private async Task<CalibrationResult> RunAsync(EyeTrackingCalibration calibration, CancellationToken token)
        {
            try
            {
                CalibrationResult outcome = await calibration.RunAsync(token);
                if (outcome.Succeeded)
                    UFeelAPI.EyeTracking.Calibrated = true;

                ShowResult(outcome.Succeeded || outcome.Cancelled
                    ? outcome.Message
                    : UFeelText.FailureReason(outcome.Diagnostics?.FailureReason ?? CalibrationFailureReason.None));
                Debug.Log(outcome.Message);
                return outcome;
            }
            catch (Exception exception)
            {
                ShowResult($"Calibration failed: {exception.Message}");
                throw;
            }
            finally
            {
                cancellation?.Dispose();
                cancellation = null;
            }
        }

        private void ShowResult(string message)
        {
            result = message;
            resultUntil = Time.unscaledTime + ResultSeconds;
        }

        private void Awake() => dot = CreateDot(64);

        private void OnDestroy()
        {
            cancellation?.Cancel();
            if (dot != null)
                Destroy(dot);
        }

        private void OnGUI()
        {
            HandleKeys(Event.current);

            if (Event.current.type != EventType.Repaint)
                return;

            float scale = Screen.height / ReferenceHeight;
            GUIStyle style = TextStyle(scale);

            if (visible)
                DrawCalibration(style, scale);

            if (result != null && Time.unscaledTime < resultUntil)
                DrawBanner(result, style, scale);
            else if (!visible && ShouldHint())
                DrawBanner($"Eye tracking is not calibrated: press {UFeelAPI.EyeTracking.CalibrationKey}", style, scale);
        }

        private void HandleKeys(Event current)
        {
            if (current.type != EventType.KeyDown)
                return;

            if (current.keyCode == KeyCode.Escape && IsRunning)
            {
                cancellation?.Cancel();
                current.Use();
            }
            else if (current.keyCode == UFeelAPI.EyeTracking.CalibrationKey && !IsRunning)
            {
                UFeelAPI.EyeTracking.Calibrate();
                current.Use();
            }
        }

        private static bool ShouldHint()
        {
            return UFeelAPI.IsAvailable &&
                !UFeelAPI.EyeTracking.IsCalibrated &&
                Time.unscaledTime - UFeelAPI.EyeTracking.LastReadTime < HintWindowSeconds;
        }

        private void DrawCalibration(GUIStyle style, float scale)
        {
            Color previous = GUI.color;

            GUI.color = Backdrop;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            float size = TargetSize * scale;
            Vector2 center = new(target.x * Screen.width, target.y * Screen.height);
            GUI.color = TargetColor;
            GUI.DrawTexture(new Rect(center.x - (size / 2), center.y - (size / 2), size, size), dot);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(center.x - (size / 8), center.y - (size / 8), size / 4, size / 4), dot);

            GUI.color = previous;

            float line = 48 * scale;
            GUI.Label(new Rect(0, (Screen.height * 0.5F) + (2 * line), Screen.width, line), instruction, style);
            GUI.Label(new Rect(0, (Screen.height * 0.5F) + (3 * line), Screen.width, line), status, style);
        }

        private static void DrawBanner(string text, GUIStyle style, float scale)
        {
            float height = style.CalcHeight(new GUIContent(text), Screen.width * 0.8F) + (16 * scale);
            Rect area = new(Screen.width * 0.1F, 16 * scale, Screen.width * 0.8F, height);

            Color previous = GUI.color;
            GUI.color = BannerBackdrop;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = previous;

            GUI.Label(area, text, style);
        }

        private GUIStyle TextStyle(float scale)
        {
            textStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = Color.white },
            };

            textStyle.fontSize = Mathf.Max(12, Mathf.RoundToInt(26 * scale));
            return textStyle;
        }

        private static Texture2D CreateDot(int size)
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };

            float radius = size / 2.0F;
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5F, y + 0.5F), new Vector2(radius, radius));
                    byte alpha = (byte)(Mathf.Clamp01(radius - distance) * 255);
                    pixels[(y * size) + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }
    }
}
