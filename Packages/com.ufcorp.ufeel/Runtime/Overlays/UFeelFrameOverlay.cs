#nullable enable

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UFeel
{
    internal sealed class UFeelFrameOverlay : MonoBehaviour, IFrameObserver
    {
        private const KeyCode ToggleKey = KeyCode.F1;
        private const float ReferenceHeight = 1080.0F;
        private const float LineSpacing = 1.5F;
        private const float PanelWidth = 320.0F;
        private const float Margin = 10.0F;
        private const float Padding = 8.0F;
        private const int TextSize = 16;

        private static readonly Color Backdrop = new(0.0F, 0.0F, 0.0F, 0.6F);

        private readonly object gate = new();
        private byte[] pixels = Array.Empty<byte>();
        private int frameWidth;
        private int frameHeight;
        private long frameVersion;

        private long uploadedVersion;
        private Texture2D? texture;
        private GUIStyle? textStyle;
        private bool visible;

        private volatile bool wanted;

        void IFrameObserver.OnFrameCaptured(UFeelProcessor processor)
        {
            if (!wanted || !processor.TryGetFrameSize(out int width, out int height))
                return;

            lock (gate)
            {
                int byteCount = checked(width * height * 4);
                if (pixels.Length != byteCount)
                    pixels = new byte[byteCount];

                GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                try
                {
                    if (!processor.TryCopyFrameBgra(handle.AddrOfPinnedObject(), width * 4, width, height))
                        return;
                }
                finally
                {
                    handle.Free();
                }

                frameWidth = width;
                frameHeight = height;
                frameVersion++;
            }
        }

        private void OnDestroy()
        {
            if (texture != null)
                Destroy(texture);
        }

        private void OnGUI()
        {
            HandleKeys(Event.current);

            if (Event.current.type != EventType.Repaint || !visible)
                return;

            float scale = Screen.height / ReferenceHeight;
            DrawPanel(scale);
        }

        private void HandleKeys(Event current)
        {
            if (current.type != EventType.KeyDown || current.keyCode != ToggleKey)
                return;

            SetVisible(!visible);
            current.Use();
        }

        private void SetVisible(bool value)
        {
            visible = value;
            wanted = value;
        }

        private void DrawPanel(float scale)
        {
            Texture2D? image = RefreshTexture();
            float x = Margin * scale;
            float y = Margin * scale;
            float panelWidth = PanelWidth * scale;
            float imageHeight = image != null ? panelWidth * image.height / image.width : 0.0F;

            string[] lines =
            {
                $"Emotion: {UFeelAPI.Emotion.Dominant?.ToString() ?? "None"}",
                $"Eye tracking: {UFeelAPI.EyeTracking.Directions}",
                $"Speech: {SpeechLine()}",
                $"Heart rate: {HeartRateLine()}",
            };

            GUIStyle style = TextStyle(scale);
            float lineHeight = TextSize * scale * LineSpacing;
            float textHeight = lines.Length * lineHeight + Padding * 2 * scale;
            var box = new Rect(x, y, panelWidth, imageHeight + textHeight);

            Color previous = GUI.color;
            GUI.color = Backdrop;
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (image != null)
            {
                GUI.DrawTextureWithTexCoords(
                    new Rect(x, y, panelWidth, imageHeight),
                    image,
                    new Rect(0.0F, 1.0F, 1.0F, -1.0F));
            }

            float textY = y + imageHeight + Padding * scale;
            foreach (string line in lines)
            {
                GUI.Label(new Rect(x + Padding * scale, textY, panelWidth - Padding * 2 * scale, lineHeight), line, style);
                textY += lineHeight;
            }

            GUI.color = previous;
        }

        private Texture2D? RefreshTexture()
        {
            lock (gate)
            {
                if (frameVersion == 0)
                    return null;

                if (frameVersion != uploadedVersion)
                {
                    if (texture == null || texture.width != frameWidth || texture.height != frameHeight)
                    {
                        if (texture != null)
                            Destroy(texture);

                        texture = new Texture2D(frameWidth, frameHeight, TextureFormat.BGRA32, false);
                    }

                    texture.LoadRawTextureData(pixels);
                    texture.Apply(false);
                    uploadedVersion = frameVersion;
                }
            }

            return texture;
        }

        private static string SpeechLine()
        {
            if (!UFeelAPI.SpeechToText.IsRunning)
                return "Disabled";

            return string.IsNullOrEmpty(UFeelAPI.SpeechToText.Current) ? "..." : UFeelAPI.SpeechToText.Current!;
        }

        private static string HeartRateLine()
        {
            uint bpm = UFeelAPI.HeartRate.Current;
            return bpm == 0 ? "--" : $"{bpm} bpm";
        }

        private GUIStyle TextStyle(float scale)
        {
            textStyle ??= new GUIStyle(GUI.skin.label) { normal = { textColor = Color.white } };
            textStyle.fontSize = Mathf.RoundToInt(TextSize * scale);
            return textStyle;
        }
    }
}
