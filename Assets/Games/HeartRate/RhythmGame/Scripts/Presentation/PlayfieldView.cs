using System.Collections.Generic;
using GameHub.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class PlayfieldView
    {
        public const float Width = LaneWidth * LaneMask.LaneCount;

        private const float LaneWidth = 120f;
        private const float JudgeY = 150f;
        private const float Height = 1080f;

        private const float Border = 2f;
        private const float TopY = Height + (NoteView.HeadSize / 2f);
        private const float TopFadeHeight = 160f;
        private const int BarLineCount = 8;
        private const float HealthWidth = 10f;
        private const float HealthHeight = 560f;
        private const float HealthGap = 16f;
        private const float LowHealth = 0.3f;
        private const float MidHealth = 0.6f;
        private const float KeyLabelFade = 3f;

        private static readonly Color HealthMid = new(0.98f, 0.86f, 0.35f, 1f);

        private readonly RectTransform root;
        private readonly RectTransform notesLayer;
        private readonly SpriteLibrary sprites;
        private readonly List<NoteView> views = new();
        private readonly ReceptorView[] receptors = new ReceptorView[LaneMask.LaneCount];
        private readonly TextMeshProUGUI[] keyLabels = new TextMeshProUGUI[LaneMask.LaneCount];
        private readonly Image[] barLines = new Image[BarLineCount];
        private readonly Image judgeLine;
        private readonly Image leftEdge;
        private readonly Image rightEdge;
        private readonly Image healthFill;
        private readonly RectTransform progress;

        private NoteStyle style;
        private float keyLabelAlpha;
        private float keyLabelTarget;

        public PlayfieldView(UiKit kit, Transform parent)
        {
            sprites = kit.Sprites;
            root = UiKit.NewRect("Playfield", parent);
            UiLayout.Place(root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(Width, Height));

            Image background = kit.NewSolid("Background", root, Theme.Fade(Theme.Backdrop, 0.9f));
            UiLayout.Stretch(background.rectTransform);
            for (int lane = 1; lane < LaneMask.LaneCount; lane++)
                CreateLine(kit, "Divider", lane * LaneWidth, Theme.Fade(Theme.Text, 0.04f), 1f);
            leftEdge = CreateLine(kit, "Edge", -Border / 2f, Theme.Line, Border);
            rightEdge = CreateLine(kit, "Edge", Width + (Border / 2f), Theme.Line, Border);

            RectTransform barLayer = CreateLayer("Bars", false);
            for (int i = 0; i < BarLineCount; i++)
            {
                barLines[i] = kit.NewSolid("BarLine", barLayer, Theme.Fade(Theme.Text, 0.07f));
                UiLayout.Place(barLines[i].rectTransform, Vector2.zero, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(Width, 1f));
            }

            RectTransform beamLayer = CreateLayer("Beams", false);
            judgeLine = kit.NewSolid("JudgeLine", root, Theme.Fade(Theme.Text, 0.5f));
            UiLayout.Place(judgeLine.rectTransform, Vector2.zero, new Vector2(0f, 0.5f), new Vector2(0f, JudgeY), new Vector2(Width, 2f));
            RectTransform receptorLayer = CreateLayer("Receptors", false);
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                receptors[lane] = new ReceptorView(beamLayer, receptorLayer, sprites, lane, LaneCenter(lane), JudgeY);
                keyLabels[lane] = kit.NewText("Key", receptorLayer, Theme.Heading, true, TextAlignmentOptions.Center, Color.clear);
                UiLayout.Place(keyLabels[lane].rectTransform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(LaneCenter(lane), JudgeY - 90f), new Vector2(LaneWidth, 40f));
            }

            notesLayer = CreateLayer("Notes", true);

            Image topFade = UiKit.NewImage("TopFade", root, sprites.Fade, Theme.Backdrop);
            UiLayout.Place(topFade.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -TopFadeHeight / 2f), new Vector2(Width, TopFadeHeight));
            topFade.rectTransform.localScale = new Vector3(1f, -1f, 1f);

            Image healthTrack = kit.NewSolid("Health", root, Theme.Fade(Theme.Text, 0.12f));
            UiLayout.Place(healthTrack.rectTransform, new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(HealthGap, JudgeY), new Vector2(HealthWidth, HealthHeight));
            healthFill = kit.NewSolid("Fill", healthTrack.rectTransform, Theme.Accent);
            UiLayout.Stretch(healthFill.rectTransform);

            progress = kit.NewSolid("Progress", root, Theme.Fade(Theme.Text, 0.35f)).rectTransform;
            UiLayout.Place(progress, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(0f, 2f));

            SetStyle(style);
            root.gameObject.SetActive(false);
        }

        public static float LaneCenter(int lane)
        {
            return (lane + 0.5f) * LaneWidth;
        }

        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        public void SetStyle(NoteStyle value)
        {
            style = value;
            foreach (ReceptorView receptor in receptors)
                receptor.SetStyle(value, sprites);
            foreach (NoteView view in views)
                view.SetStyle(value);
        }

        public void SetKeyLabels(IReadOnlyList<string> labels, bool visible)
        {
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
                keyLabels[lane].text = labels[lane];
            keyLabelTarget = visible ? 1f : 0f;
        }

        public void ShowKeyLabels(bool visible)
        {
            keyLabelTarget = visible ? 1f : 0f;
        }

        public void SetHealth(float health, float pulse)
        {
            healthFill.rectTransform.anchorMax = new Vector2(1f, Mathf.Clamp01(health));
            healthFill.rectTransform.offsetMax = Vector2.zero;
            healthFill.color = health < LowHealth
                ? Theme.Fade(Theme.Danger, 0.6f + (0.4f * pulse))
                : health < MidHealth ? Color.Lerp(Theme.Danger, HealthMid, (health - LowHealth) / (MidHealth - LowHealth)) : Color.Lerp(HealthMid, Theme.Accent, (health - MidHealth) / (1f - MidHealth));
        }

        public void SetProgress(float value)
        {
            progress.sizeDelta = new Vector2(Width * Mathf.Clamp01(value), 2f);
        }

        public void Burst(int lane, Color color)
        {
            receptors[lane].Burst(color);
        }

        public void Render(double songTime, IReadOnlyList<PlayableNote> notes, double scrollTime, SongAnalysis? analysis)
        {
            float pixelsPerSecond = (float)((TopY - JudgeY) / scrollTime);
            int used = 0;
            foreach (PlayableNote note in notes)
            {
                if (note.IsCleared)
                    continue;

                float headY = note.IsHolding ? JudgeY : JudgeY + (float)((note.Time - songTime) * pixelsPerSecond);
                if (headY > TopY + NoteView.HeadSize)
                    break;

                float tailY = JudgeY + (float)((note.EndTime - songTime) * pixelsPerSecond);
                if (Mathf.Max(headY, tailY) < -NoteView.HeadSize)
                    continue;

                if (used == views.Count)
                    views.Add(new NoteView(notesLayer, sprites, style));
                views[used++].Show(headY, tailY, note.Lane, note.IsHold, note.IsHolding, note.IsBroken);
            }

            for (int i = used; i < views.Count; i++)
                views[i].Hide();

            RenderBars(songTime, pixelsPerSecond, analysis);
        }

        public void Tick(float deltaTime, ILaneInput input, float pulse, Color levelColor)
        {
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
                receptors[lane].Tick(deltaTime, input.IsHeld(lane), pulse);

            judgeLine.color = Theme.Fade(Color.Lerp(Theme.Text, levelColor, 0.5f), 0.45f + (0.35f * pulse));
            Color edge = Theme.Fade(levelColor, 0.25f + (0.25f * pulse));
            leftEdge.color = edge;
            rightEdge.color = edge;

            keyLabelAlpha = Mathf.MoveTowards(keyLabelAlpha, keyLabelTarget, deltaTime * KeyLabelFade);
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
                keyLabels[lane].color = Theme.Fade(Theme.Text, 0.8f * keyLabelAlpha);
        }

        public void Clear()
        {
            foreach (NoteView view in views)
                view.Hide();
            for (int i = 0; i < BarLineCount; i++)
                barLines[i].enabled = false;
        }

        private void RenderBars(double songTime, float pixelsPerSecond, SongAnalysis? analysis)
        {
            int first = analysis?.MeasureAt(songTime - (JudgeY / pixelsPerSecond)) ?? 0;
            for (int i = 0; i < BarLineCount; i++)
            {
                float y = analysis == null ? -1f : JudgeY + (float)((analysis.MeasureTime(first + i) - songTime) * pixelsPerSecond);
                bool visible = analysis != null && first + i >= 0 && y >= 0f && y <= Height;
                barLines[i].enabled = visible;
                if (visible)
                    barLines[i].rectTransform.anchoredPosition = new Vector2(0f, y);
            }
        }

        private Image CreateLine(UiKit kit, string name, float x, Color color, float width)
        {
            Image line = kit.NewSolid(name, root, color);
            UiLayout.Place(line.rectTransform, Vector2.zero, new Vector2(0.5f, 0f), new Vector2(x, 0f), new Vector2(width, Height));
            return line;
        }

        private RectTransform CreateLayer(string name, bool ownCanvas)
        {
            RectTransform layer = UiKit.NewRect(name, root);
            UiLayout.Stretch(layer);
            if (ownCanvas)
                layer.gameObject.AddComponent<Canvas>();
            return layer;
        }
    }
}
