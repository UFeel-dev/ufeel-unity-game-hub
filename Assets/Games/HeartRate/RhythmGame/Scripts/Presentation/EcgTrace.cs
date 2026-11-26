using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class EcgTrace
    {
        private const int SegmentCount = 96;
        private const int GapSegments = 6;
        private const float SweepSeconds = 2.4f;
        private const float Thickness = 3f;
        private const float Amplitude = 0.42f;

        private readonly Image[] segments = new Image[SegmentCount];
        private readonly Vector2 size;

        private float pending;
        private float phase;
        private float lastValue;
        private int cursor;
        private Color color = Color.clear;

        public EcgTrace(RectTransform area, SpriteLibrary sprites)
        {
            size = area.sizeDelta;
            for (int i = 0; i < SegmentCount; i++)
            {
                segments[i] = UiKit.NewImage("Segment", area, sprites.Solid, Color.clear);
                RectTransform rect = segments[i].rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = new Vector2(0f, 0.5f);
                segments[i].enabled = false;
            }
        }

        public void SetColor(Color tint)
        {
            if (tint == color)
                return;

            color = tint;
            foreach (Image segment in segments)
                segment.color = tint;
        }

        public void Advance(float deltaTime, float bpm, bool flat)
        {
            const float segmentSeconds = SweepSeconds / SegmentCount;
            pending += deltaTime;
            while (pending >= segmentSeconds)
            {
                pending -= segmentSeconds;
                phase += segmentSeconds * bpm / 60f;
                phase -= Mathf.Floor(phase);
                float value = flat ? 0f : Waveform(phase);
                Draw(cursor, lastValue, value);
                lastValue = value;
                cursor = (cursor + 1) % SegmentCount;
            }
        }

        private static float Waveform(float t)
        {
            return Bump(t, 0.12f, 0.025f, 0.12f)
                + Bump(t, 0.22f, 0.008f, -0.15f)
                + Bump(t, 0.25f, 0.012f, 1f)
                + Bump(t, 0.29f, 0.012f, -0.3f)
                + Bump(t, 0.48f, 0.045f, 0.25f);
        }

        private static float Bump(float t, float center, float width, float height)
        {
            float distance = (t - center) / width;
            return height * Mathf.Exp(-distance * distance);
        }

        private void Draw(int index, float from, float to)
        {
            float step = size.x / SegmentCount;
            Vector2 start = new(index * step, (size.y / 2f) + (from * Amplitude * size.y));
            Vector2 end = new((index + 1) * step, (size.y / 2f) + (to * Amplitude * size.y));
            Vector2 delta = end - start;

            RectTransform rect = segments[index].rectTransform;
            rect.anchoredPosition = start;
            rect.sizeDelta = new Vector2(delta.magnitude + 1f, Thickness);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            segments[index].enabled = true;

            for (int gap = 1; gap <= GapSegments; gap++)
                segments[(index + gap) % SegmentCount].enabled = false;
        }
    }
}
