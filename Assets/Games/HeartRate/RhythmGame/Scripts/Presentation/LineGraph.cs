using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class LineGraph
    {
        private readonly RectTransform area;
        private readonly Sprite solid;
        private readonly float thickness;
        private readonly List<Image> segments = new();

        public LineGraph(RectTransform area, Sprite solid, float thickness)
        {
            this.area = area;
            this.solid = solid;
            this.thickness = thickness;
        }

        public void SetPoints(IReadOnlyList<Vector2> points, Color color)
        {
            Vector2 size = area.rect.size;
            int used = 0;
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 from = points[i - 1];
                Vector2 to = points[i];
                if (float.IsNaN(from.y) || float.IsNaN(to.y))
                    continue;

                if (used == segments.Count)
                    segments.Add(CreateSegment());
                Draw(segments[used++], Vector2.Scale(from, size), Vector2.Scale(to, size), color);
            }

            for (int i = used; i < segments.Count; i++)
                segments[i].enabled = false;
        }

        private Image CreateSegment()
        {
            Image segment = UiKit.NewImage("Segment", area, solid, Color.white);
            RectTransform rect = segment.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0f, 0.5f);
            return segment;
        }

        private void Draw(Image segment, Vector2 from, Vector2 to, Color color)
        {
            Vector2 delta = to - from;
            RectTransform rect = segment.rectTransform;
            rect.anchoredPosition = from;
            rect.sizeDelta = new Vector2(delta.magnitude + (thickness * 0.5f), thickness);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            segment.color = color;
            segment.enabled = true;
        }
    }
}
