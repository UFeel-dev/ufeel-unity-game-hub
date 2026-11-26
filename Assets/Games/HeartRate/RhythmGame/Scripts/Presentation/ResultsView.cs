using System;
using System.Collections.Generic;
using System.Globalization;
using GameHub.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class ResultsView
    {
        private const float LeftWidth = 620f;
        private const float GraphWidth = 720f;
        private const float GraphHeight = 300f;
        private const float ColumnWidth = 118f;
        private const float MinBpmRange = 10f;
        private const int GridLines = 4;
        private const float PeakSize = 12f;

        private readonly RectTransform root;
        private readonly TextMeshProUGUI songTitle;
        private readonly TextMeshProUGUI songInfo;
        private readonly TextMeshProUGUI grade;
        private readonly TextMeshProUGUI tag;
        private readonly TextMeshProUGUI accuracy;
        private readonly TextMeshProUGUI score;
        private readonly TextMeshProUGUI combo;
        private readonly TextMeshProUGUI[] counts = new TextMeshProUGUI[5];
        private readonly RectTransform graph;
        private readonly LineGraph bpmLine;
        private readonly LineGraph levelLine;
        private readonly TextMeshProUGUI bpmLegend;
        private readonly TextMeshProUGUI levelLegend;
        private readonly TextMeshProUGUI summary;
        private readonly List<Vector2> points = new(256);
        private readonly TextMeshProUGUI[] scale = new TextMeshProUGUI[GridLines + 1];
        private readonly RectTransform peakDot;
        private readonly TextMeshProUGUI peakLabel;

        public ResultsView(UiKit kit, Transform parent)
        {
            root = UiKit.NewRect("Results", parent);
            UiLayout.Stretch(root);

            RectTransform left = UiKit.NewRect("Summary", root);
            UiLayout.Place(left, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(Theme.Margin + 22f, 20f), new Vector2(LeftWidth, 700f));
            songTitle = Line(kit, left, Theme.Heading, true, Theme.Text, 0f, 44f);
            songInfo = Line(kit, left, Theme.Small, false, Theme.TextDim, 46f, 30f);
            grade = Line(kit, left, 220f, true, Theme.Text, 90f, 230f);
            tag = Line(kit, left, Theme.Small, true, Theme.Accent, 318f, 30f);
            accuracy = Line(kit, left, Theme.Display, true, Theme.Text, 350f, 84f);
            score = Line(kit, left, Theme.Heading, false, Theme.TextDim, 436f, 42f);
            combo = Line(kit, left, Theme.Small, false, Theme.TextDim, 482f, 30f);

            for (int i = 0; i < counts.Length; i++)
            {
                RectTransform column = UiKit.NewRect("Count", left);
                UiLayout.Place(column, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(i * ColumnWidth, -548f), new Vector2(ColumnWidth, 76f));
                counts[i] = kit.NewText("Value", column, Theme.Heading, true, TextAlignmentOptions.TopLeft, RhythmTheme.Judgement((Judgement)i));
                UiLayout.Row(counts[i].rectTransform, 0f, 40f);
                TextMeshProUGUI name = kit.NewText("Name", column, Theme.Tiny, false, TextAlignmentOptions.TopLeft, Theme.TextDim);
                UiLayout.Row(name.rectTransform, 42f, 26f);
                name.text = RhythmTheme.JudgementLabel((Judgement)i);
            }

            graph = UiKit.NewRect("Graph", root);
            UiLayout.Place(graph, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-Theme.Margin, 40f), new Vector2(GraphWidth, GraphHeight));
            for (int i = 0; i <= GridLines; i++)
            {
                Image line = kit.NewSolid("Grid", graph, i == 0 ? Theme.Line : Theme.Fade(Theme.Text, 0.04f));
                UiLayout.Place(line.rectTransform, new Vector2(0f, i / (float)GridLines), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(GraphWidth, 1f));
                scale[i] = kit.NewText("Scale", graph, Theme.Tiny, false, TextAlignmentOptions.MidlineRight, Theme.TextDim);
                UiLayout.Place(scale[i].rectTransform, new Vector2(0f, i / (float)GridLines), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(60f, 24f));
            }
            levelLine = new LineGraph(graph, kit.Sprites.Solid, 3f);
            bpmLine = new LineGraph(graph, kit.Sprites.Solid, 2f);
            peakDot = UiKit.NewImage("Peak", graph, kit.Sprites.Circle, Theme.Text).rectTransform;
            peakLabel = kit.NewText("PeakLabel", graph, Theme.Small, true, TextAlignmentOptions.Bottom, Theme.Text);
            peakLabel.rectTransform.sizeDelta = new Vector2(120f, 28f);

            bpmLegend = kit.NewText("BpmLegend", graph, Theme.Small, true, TextAlignmentOptions.BottomLeft, Theme.Text);
            UiLayout.Place(bpmLegend.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 10f), new Vector2(360f, 30f));
            levelLegend = kit.NewText("LevelLegend", graph, Theme.Small, true, TextAlignmentOptions.BottomRight, Theme.Text);
            UiLayout.Place(levelLegend.rectTransform, Vector2.one, new Vector2(1f, 0f), new Vector2(0f, 10f), new Vector2(360f, 30f));
            summary = kit.NewText("Summary", graph, Theme.Small, false, TextAlignmentOptions.TopLeft, Theme.TextDim);
            UiLayout.Place(summary.rectTransform, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, -14f), new Vector2(GraphWidth, 30f));

            root.gameObject.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        public void Show(RunResult result, bool isNewBest)
        {
            songTitle.text = result.Song.Title;
            songInfo.text = FormattableString.Invariant($"Base {result.BaseLevel:0.0}      Scaling {result.Scaling:0.0}");
            grade.text = GradeCalculator.Label(result.Grade);
            grade.color = RhythmTheme.Grade(result.Grade);
            tag.text = TagText(result.Failed, isNewBest);
            tag.color = result.Failed ? Theme.Danger : Theme.Accent;
            accuracy.text = (result.Accuracy * 100d).ToString("0.00", CultureInfo.InvariantCulture) + "%";
            score.text = result.Score.ToString("N0", CultureInfo.InvariantCulture);
            combo.text = FormattableString.Invariant($"Max combo {result.MaxCombo}");
            for (int i = 0; i < counts.Length; i++)
                counts[i].text = result.Count((Judgement)i).ToString(CultureInfo.InvariantCulture);

            DrawGraph(result);
            root.gameObject.SetActive(true);
        }

        private static string TagText(bool failed, bool isNewBest)
        {
            if (failed)
                return "Flatline";
            return isNewBest ? "New best" : string.Empty;
        }

        private static TextMeshProUGUI Line(UiKit kit, RectTransform parent, float size, bool bold, Color color, float top, float height)
        {
            TextMeshProUGUI text = kit.NewText("Line", parent, size, bold, TextAlignmentOptions.BottomLeft, color);
            UiLayout.Row(text.rectTransform, top, height);
            return text;
        }

        private void DrawScale(bool visible, float low, float high)
        {
            for (int i = 0; i <= GridLines; i++)
            {
                scale[i].gameObject.SetActive(visible);
                scale[i].text = FormattableString.Invariant($"{low + ((high - low) * i / GridLines):0}");
            }
        }

        private void DrawPeak(Vector2? point, float bpm)
        {
            peakDot.gameObject.SetActive(point.HasValue);
            peakLabel.gameObject.SetActive(point.HasValue);
            if (!point.HasValue)
                return;

            Vector2 position = Vector2.Scale(point.Value, new Vector2(GraphWidth, GraphHeight));
            UiLayout.Place(peakDot, Vector2.zero, new Vector2(0.5f, 0.5f), position, Vector2.one * PeakSize);
            float labelX = Mathf.Clamp(position.x, peakLabel.rectTransform.sizeDelta.x / 2f, GraphWidth - (peakLabel.rectTransform.sizeDelta.x / 2f));
            UiLayout.Place(peakLabel.rectTransform, Vector2.zero, new Vector2(0.5f, 0f), new Vector2(labelX, position.y + 10f), peakLabel.rectTransform.sizeDelta);
            peakLabel.text = FormattableString.Invariant($"Peak {bpm:0}");
        }

        private void DrawGraph(RunResult result)
        {
            IReadOnlyList<float> levels = result.LevelSeries;
            IReadOnlyList<float> bpms = result.BpmSeries;
            int count = Math.Max(2, levels.Count);
            float peakLevel = DifficultySettings.MinLevel;
            foreach (float level in levels)
                peakLevel = Math.Max(peakLevel, level);

            points.Clear();
            for (int i = 0; i < levels.Count; i++)
                points.Add(new Vector2(i / (float)(count - 1), (levels[i] - DifficultySettings.MinLevel) / (DifficultySettings.MaxLevel - DifficultySettings.MinLevel)));
            levelLine.SetPoints(points, Theme.Fade(RhythmTheme.Level(peakLevel), 0.9f));
            levelLegend.text = $"<color=#{Theme.Hex(RhythmTheme.Level(peakLevel))}>Level</color>";

            float low = result.RestingBpm;
            int peakIndex = -1;
            for (int i = 0; i < bpms.Count; i++)
            {
                if (float.IsNaN(bpms[i]))
                    continue;
                low = Math.Min(low, bpms[i]);
                if (peakIndex < 0 || bpms[i] > bpms[peakIndex])
                    peakIndex = i;
            }

            float high = Math.Max(result.PeakBpm, low + MinBpmRange);
            points.Clear();
            for (int i = 0; i < bpms.Count; i++)
                points.Add(new Vector2(i / (float)(count - 1), float.IsNaN(bpms[i]) ? float.NaN : Mathf.Clamp01((bpms[i] - low) / (high - low))));
            bpmLine.SetPoints(points, Theme.Text);
            DrawScale(result.HasHeartRate, low, high);
            DrawPeak(peakIndex >= 0 && result.HasHeartRate ? points[peakIndex] : null, peakIndex >= 0 ? bpms[peakIndex] : 0f);

            bpmLegend.text = result.HasHeartRate ? "Heart rate" : "Heart rate  <color=#5C6B6B>no reading</color>";
            summary.text = result.HasHeartRate
                ? FormattableString.Invariant($"Resting <b>{result.RestingBpm:0}</b>      Average <b>{result.AverageBpm:0}</b>      Peak <b>{result.PeakBpm:0}</b>")
                : FormattableString.Invariant($"Resting <b>{result.RestingBpm:0}</b>");
        }
    }
}
