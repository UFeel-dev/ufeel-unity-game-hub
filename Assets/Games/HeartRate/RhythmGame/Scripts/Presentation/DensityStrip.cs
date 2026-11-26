using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class DensityStrip
    {
        private const float BarGap = 1f;
        private const float ProgressHeight = 2f;

        private readonly UiKit kit;
        private readonly RectTransform area;
        private readonly List<Image> highBars = new();
        private readonly List<Image> lowBars = new();
        private readonly RectTransform progress;

        public DensityStrip(UiKit kit, RectTransform area)
        {
            this.kit = kit;
            this.area = area;
            progress = kit.NewSolid("Progress", area, Theme.Accent).rectTransform;
            UiLayout.Place(progress, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0f, ProgressHeight));
        }

        public void SetProgress(float value)
        {
            progress.gameObject.SetActive(value < 1f);
            progress.sizeDelta = new Vector2(area.rect.width * Mathf.Clamp01(value), ProgressHeight);
            SetBars(0, highBars);
            SetBars(0, lowBars);
        }

        public void Set(ChartSet set, float lowLevel, float highLevel, Color color)
        {
            progress.gameObject.SetActive(false);
            ChartLayer low = set.LayerFor(lowLevel);
            ChartLayer high = set.LayerFor(highLevel);
            int measures = set.Analysis.MeasureCount;
            int peak = 1;
            for (int measure = 0; measure < measures; measure++)
                peak = Mathf.Max(peak, high.NotesInMeasure(measure));

            SetBars(measures, highBars);
            SetBars(measures, lowBars);
            float width = area.rect.width / Mathf.Max(1, measures);
            float height = area.rect.height;
            for (int measure = 0; measure < measures; measure++)
            {
                Size(highBars[measure], measure, width, height * high.NotesInMeasure(measure) / peak, Theme.Fade(color, 0.22f));
                Size(lowBars[measure], measure, width, height * low.NotesInMeasure(measure) / peak, Theme.Fade(color, 0.85f));
            }
        }

        private static void Size(Image bar, int index, float width, float height, Color color)
        {
            RectTransform rect = bar.rectTransform;
            rect.anchoredPosition = new Vector2(index * width, 0f);
            rect.sizeDelta = new Vector2(Mathf.Max(1f, width - BarGap), Mathf.Max(1f, height));
            bar.color = color;
        }

        private void SetBars(int count, List<Image> bars)
        {
            while (bars.Count < count)
            {
                Image bar = kit.NewSolid("Bar", area, Color.clear);
                UiLayout.Place(bar.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                bars.Add(bar);
            }

            for (int i = 0; i < bars.Count; i++)
                bars[i].enabled = i < count;
        }
    }
}
