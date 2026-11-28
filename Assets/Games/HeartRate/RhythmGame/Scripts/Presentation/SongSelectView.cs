using System;
using System.Collections.Generic;
using GameHub.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class SongSelectView
    {
        private const int VisibleRows = 8;
        private const float RowHeight = 76f;
        private const float ListWidth = 560f;
        private const float PanelWidth = 600f;
        private const float GraphTop = 300f;
        private const float GraphHeight = 170f;
        private const float DensityGap = 18f;
        private const float DensityHeight = 44f;
        private const float MarkerSize = 14f;

        private readonly RectTransform root;
        private readonly TextMeshProUGUI songTitle;
        private readonly TextMeshProUGUI artist;
        private readonly TextMeshProUGUI meta;
        private readonly TextMeshProUGUI best;
        private readonly RectTransform graph;
        private readonly LineGraph curve;
        private readonly RectTransform marker;
        private readonly RectTransform markerLine;
        private readonly TextMeshProUGUI markerLabel;
        private readonly TextMeshProUGUI restingLabel;
        private readonly TextMeshProUGUI peakLabel;
        private readonly DensityStrip density;
        private readonly HeartView heart;
        private readonly List<Vector2> points = new(3);

        public SongSelectView(UiKit kit, Transform parent)
        {
            root = UiKit.NewRect("SongSelect", parent);
            UiLayout.Stretch(root);

            RectTransform listArea = UiKit.NewRect("Songs", root);
            UiLayout.Place(listArea, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(Theme.Margin, 20f), new Vector2(ListWidth, VisibleRows * RowHeight));
            List = new MenuList(kit, listArea, VisibleRows, RowHeight, Theme.Heading) { PlaysSounds = false };

            RectTransform panel = UiKit.NewRect("Details", root);
            UiLayout.Place(panel, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-Theme.Margin, 30f), new Vector2(PanelWidth, 640f));

            songTitle = kit.NewText("Title", panel, Theme.Title, true, TextAlignmentOptions.BottomLeft, Theme.Text);
            songTitle.overflowMode = TextOverflowModes.Ellipsis;
            UiLayout.Row(songTitle.rectTransform, 0f, 56f);
            artist = kit.NewText("Artist", panel, Theme.Body, false, TextAlignmentOptions.TopLeft, Theme.TextDim);
            UiLayout.Row(artist.rectTransform, 60f, 34f);
            meta = kit.NewText("Meta", panel, Theme.Small, false, TextAlignmentOptions.TopLeft, Theme.TextDim);
            UiLayout.Row(meta.rectTransform, 98f, 28f);

            BaseSlider = new SliderView(kit, panel, "Base", 170f, PanelWidth);
            ScalingSlider = new SliderView(kit, panel, "Scaling", 222f, PanelWidth);

            graph = UiKit.NewRect("Curve", panel);
            UiLayout.Place(graph, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -GraphTop), new Vector2(PanelWidth, GraphHeight));
            for (int i = 0; i <= 3; i++)
            {
                Image line = kit.NewSolid("Grid", graph, i == 0 ? Theme.Line : Theme.Fade(Theme.Text, 0.04f));
                UiLayout.Place(line.rectTransform, new Vector2(0f, i / 3f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(PanelWidth, 1f));
            }
            curve = new LineGraph(graph, kit.Sprites.Solid, 3f);
            markerLine = kit.NewSolid("MarkerLine", graph, Theme.Fade(Theme.Text, 0.12f)).rectTransform;
            marker = UiKit.NewImage("Marker", graph, kit.Sprites.Circle, Theme.Text).rectTransform;
            markerLabel = kit.NewText("MarkerLabel", graph, Theme.Small, true, TextAlignmentOptions.Bottom, Theme.Text);
            markerLabel.rectTransform.sizeDelta = new Vector2(80f, 28f);
            restingLabel = AxisLabel(kit, TextAlignmentOptions.TopLeft, new Vector2(0f, 0f), new Vector2(0f, 1f));
            peakLabel = AxisLabel(kit, TextAlignmentOptions.TopRight, new Vector2(1f, 0f), new Vector2(1f, 1f));

            RectTransform densityArea = UiKit.NewRect("Density", panel);
            UiLayout.Place(densityArea, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -(GraphTop + GraphHeight + 34f + DensityGap)), new Vector2(PanelWidth, DensityHeight));
            density = new DensityStrip(kit, densityArea);

            best = kit.NewText("Best", panel, Theme.Body, false, TextAlignmentOptions.TopLeft, Theme.Text);
            UiLayout.Row(best.rectTransform, GraphTop + GraphHeight + 34f + DensityGap + DensityHeight + 26f, 34f);

            heart = new HeartView(kit, root, false);
            UiLayout.Place(heart.Root, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-Theme.Margin, 40f), heart.Root.sizeDelta);

            root.gameObject.SetActive(false);
        }

        public MenuList List { get; }

        public SliderView BaseSlider { get; }

        public SliderView ScalingSlider { get; }

        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        public void ShowEmpty()
        {
            songTitle.text = "No songs";
            artist.text = string.Empty;
            meta.text = string.Empty;
            best.text = string.Empty;
        }

        public void ShowSong(RhythmSong song, SongRecord record, double bpm)
        {
            songTitle.text = song.Title;
            artist.text = song.Artist;
            AudioClip clip = song.Clip;
            float length = 0f;
            if (clip != null)
                length = clip.length;
            meta.text = FormattableString.Invariant($"{(int)length / 60}:{(int)length % 60:00}      {bpm:0} BPM");

            if (!record.HasRecord)
            {
                best.text = string.Empty;
                return;
            }

            string grade = $"<color=#{Theme.Hex(RhythmTheme.Grade(record.Grade))}><b>{GradeCalculator.Label(record.Grade)}</b></color>";
            best.text = FormattableString.Invariant($"Best   {grade}   {record.Accuracy * 100f:0.00}%   <color=#{Theme.Hex(Theme.TextDim)}>{record.Score:N0}   at {record.Base:0.0} / {record.Scaling:0.0}</color>");
        }

        public void SetDifficulty(float baseLevel, float scaling, float restingBpm, float exertion, bool hasLiveRate)
        {
            const float span = DifficultySettings.MaxLevel - DifficultySettings.MinLevel;
            float top = DifficultySettings.Level(baseLevel, scaling, 1f);
            BaseSlider.Set((baseLevel - DifficultySettings.BaseMin) / (DifficultySettings.BaseMax - DifficultySettings.BaseMin), FormattableString.Invariant($"{baseLevel:0.0}"), RhythmTheme.Level(baseLevel));
            ScalingSlider.Set((scaling - DifficultySettings.ScalingMin) / (DifficultySettings.ScalingMax - DifficultySettings.ScalingMin), FormattableString.Invariant($"{scaling:0.0}"), RhythmTheme.Level(top));

            points.Clear();
            points.Add(new Vector2(0f, (baseLevel - DifficultySettings.MinLevel) / span));
            if (scaling > 0f && baseLevel + scaling > DifficultySettings.MaxLevel)
                points.Add(new Vector2((DifficultySettings.MaxLevel - baseLevel) / scaling, 1f));
            points.Add(new Vector2(1f, (top - DifficultySettings.MinLevel) / span));
            curve.SetPoints(points, RhythmTheme.Level(top));

            restingLabel.text = FormattableString.Invariant($"{restingBpm:0}");
            peakLabel.text = FormattableString.Invariant($"{restingBpm + DifficultySettings.HeartRateSpan:0} BPM");

            float level = DifficultySettings.Level(baseLevel, scaling, exertion);
            Vector2 position = new(exertion * PanelWidth, (level - DifficultySettings.MinLevel) / span * GraphHeight);
            UiLayout.Place(marker, Vector2.zero, new Vector2(0.5f, 0.5f), position, Vector2.one * MarkerSize);
            UiLayout.Place(markerLine, Vector2.zero, new Vector2(0.5f, 0f), new Vector2(position.x, 0f), new Vector2(1f, GraphHeight));
            UiLayout.Place(markerLabel.rectTransform, Vector2.zero, new Vector2(0.5f, 0f), position + new Vector2(0f, 12f), markerLabel.rectTransform.sizeDelta);
            markerLabel.text = FormattableString.Invariant($"{level:0.0}");
            markerLabel.color = hasLiveRate ? RhythmTheme.Level(level) : Theme.TextDim;
            marker.GetComponent<Image>().color = hasLiveRate ? Theme.Text : Theme.TextDim;
        }

        public void SetDensity(ChartSet? set, float baseLevel, float scaling, float progress)
        {
            if (set == null)
            {
                density.SetProgress(progress);
                return;
            }

            float top = DifficultySettings.Level(baseLevel, scaling, 1f);
            density.Set(set, baseLevel, top, RhythmTheme.Level(top));
        }

        public void TickHeart(float deltaTime, HeartRateMonitor monitor, float restingBpm)
        {
            string caption = FormattableString.Invariant($"Resting {restingBpm:0}");
            heart.Tick(deltaTime, monitor.SmoothedBpm, monitor.HasReading, monitor.Pulse, Theme.Accent, caption, false);
        }

        public void Tick(float deltaTime)
        {
            List.Tick(deltaTime);
            BaseSlider.Tick(deltaTime);
            ScalingSlider.Tick(deltaTime);
        }

        private TextMeshProUGUI AxisLabel(UiKit kit, TextAlignmentOptions alignment, Vector2 anchor, Vector2 pivot)
        {
            TextMeshProUGUI label = kit.NewText("Axis", graph, Theme.Tiny, false, alignment, Theme.TextDim);
            UiLayout.Place(label.rectTransform, anchor, pivot, new Vector2(0f, -8f), new Vector2(200f, 24f));
            return label;
        }
    }
}
