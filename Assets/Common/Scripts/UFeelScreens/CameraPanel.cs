using UFeel;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class CameraPanel
    {
        private const float Width = 420f;
        private const float Padding = 18f;
        private const float Gap = 14f;
        private const float Offset = 24f;
        private const float PanelOpacity = 0.96f;
        private const float Inner = Width - (2f * Padding);
        private const float Column = (Inner - Gap) / 2f;

        private readonly RectTransform root;
        private readonly RawImage image;
        private readonly StatView emotion;
        private readonly StatView eyes;
        private readonly StatView speech;
        private readonly StatView heartRate;

        private Vector2Int frameSize;

        public CameraPanel(UiKit kit, Transform parent)
        {
            root = UiKit.NewRect("Camera", parent);

            Image panel = kit.NewRounded("Panel", root, Theme.Fade(Theme.Backdrop, PanelOpacity), 14f);
            UiLayout.Stretch(panel.rectTransform);

            image = UiKit.NewRect("Frame", root).gameObject.AddComponent<RawImage>();
            image.raycastTarget = false;
            image.uvRect = new Rect(0f, 1f, 1f, -1f);

            emotion = Stat(kit, "Emotion");
            eyes = Stat(kit, "Eye tracking");
            speech = Stat(kit, "Speech");
            heartRate = Stat(kit, "Heart rate");

            Layout();
            root.gameObject.SetActive(false);
        }

        public bool IsVisible => root.gameObject.activeSelf;

        public void Toggle()
        {
            root.gameObject.SetActive(!IsVisible);
        }

        public void Tick()
        {
            if (!IsVisible)
                return;

            Texture2D? frame = UFeelAPI.CameraFeed.Frame;
            image.texture = frame;
            Vector2Int size = frame != null ? new Vector2Int(frame.width, frame.height) : Vector2Int.zero;
            if (size != frameSize)
            {
                frameSize = size;
                Layout();
            }

            emotion.Set(UFeelAPI.Emotion.Dominant?.ToString() ?? "None");
            eyes.Set(UFeelAPI.EyeTracking.Directions.ToString());
            speech.Set(SpeechText());
            uint bpm = UFeelAPI.HeartRate.Current;
            heartRate.Set(bpm == 0 ? "--" : $"{bpm} BPM");
        }

        private static string SpeechText()
        {
            if (!UFeelAPI.SpeechToText.IsRunning)
                return "Off";

            string? current = UFeelAPI.SpeechToText.Current;
            return string.IsNullOrEmpty(current) ? "..." : current;
        }

        private StatView Stat(UiKit kit, string caption)
        {
            StatView stat = new(kit, root, caption, false, Theme.Body);
            stat.Root.sizeDelta = new Vector2(Column, stat.Height);
            return stat;
        }

        private void Layout()
        {
            float imageHeight = frameSize.x > 0 ? Inner * frameSize.y / frameSize.x : 0f;
            image.enabled = imageHeight > 0f;
            Vector2 topLeft = new(0f, 1f);
            UiLayout.Place(image.rectTransform, topLeft, topLeft, new Vector2(Padding, -Padding), new Vector2(Inner, imageHeight));

            float top = Padding + (imageHeight > 0f ? imageHeight + Gap : 0f);
            StatView[] stats = { emotion, eyes, speech, heartRate };
            for (int i = 0; i < stats.Length; i++)
            {
                float x = Padding + (i % 2 * (Column + Gap));
                float y = top + (i / 2 * (stats[i].Height + Gap));
                UiLayout.Place(stats[i].Root, topLeft, topLeft, new Vector2(x, -y), stats[i].Root.sizeDelta);
            }

            float height = top + (2f * emotion.Height) + Gap + Padding;
            UiLayout.Place(root, Vector2.one, Vector2.one, new Vector2(-Offset, -Offset), new Vector2(Width, height));
        }
    }
}
