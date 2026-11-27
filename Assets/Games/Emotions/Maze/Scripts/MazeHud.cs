using System;
using System.Collections.Generic;
using GameHub.Common;
using UFeel;
using UnityEngine;

namespace GameHub.Maze
{
    public sealed class MazeHud
    {
        private const float MeterWidth = 520f;
        private const float LabelWidth = 170f;
        private const float ValueWidth = 90f;
        private const float Bottom = 96f;
        private const float ShadeDepth = 460f;
        private const float ShadeOpacity = 0.6f;
        private const float CardOpacity = 0.6f;
        private const float FadingOpacity = 0.55f;
        private const float NoticeOffset = 72f;
        private const float NoticeSeconds = 3f;
        private const float IntroSeconds = 3f;
        private const string Open = "Open";

        private static readonly KeyHint[] Hints = { new("Level select", 0, "L") };

        private readonly RectTransform canvas;
        private readonly RectTransform meters;
        private readonly Dictionary<EmotionType, MeterView> views = new();
        private readonly Dictionary<EmotionType, Color> colors = new();
        private readonly TitleCard card;
        private readonly Toast notice;
        private readonly KeyBar keys;
        private float introRemaining;

        public MazeHud(UiKit kit, Transform owner, IReadOnlyList<EmotionInfo> emotions, MazeGrid grid, MazeIcons icons)
        {
            canvas = UiCanvas.Create("MazeHud", owner, UiCanvas.GameOrder);
            meters = UiKit.NewRect("Meters", canvas);
            UiLayout.Stretch(meters);
            kit.NewShade("Shade", meters, RectTransform.Edge.Bottom, ShadeDepth, ShadeOpacity);

            for (int i = 0; i < emotions.Count; i++)
            {
                MeterView view = new(kit, meters, MeterWidth, LabelWidth, ValueWidth);
                view.SetLabel(emotions[i].Emotion.ToString());
                float y = Bottom + ((emotions.Count - 1 - i) * MeterView.Height);
                UiLayout.Place(view.Root, Vector2.zero, Vector2.zero, new Vector2(Theme.Margin, y), view.Root.sizeDelta);
                views[emotions[i].Emotion] = view;
                colors[emotions[i].Emotion] = emotions[i].Color;
            }

            Map = new MazeMinimap(kit, canvas, grid, icons);
            notice = new Toast(kit, canvas, new Vector2(0.5f, 1f), NoticeOffset, Theme.Body);
            keys = new KeyBar(kit, canvas);
            keys.Set(Hints);
            card = new TitleCard(kit, canvas, CardOpacity);
        }

        public MazeMinimap Map { get; }

        public void SetLevel(EmotionType emotion, float progress, bool dominant)
        {
            bool open = progress >= 1f;
            Color color = dominant ? colors[emotion] : Theme.Fade(colors[emotion], FadingOpacity);
            MeterView view = views[emotion];
            view.Set(progress, open ? Open : FormattableString.Invariant($"{progress * 100f:0}%"), color);
            view.SetEmphasis(dominant);
        }

        public void Notify(string message)
        {
            notice.Show(message, NoticeSeconds);
        }

        public void Destroy()
        {
            if (canvas != null)
                UnityEngine.Object.Destroy(canvas.gameObject);
        }

        public void ShowIntro(string heading, string detail)
        {
            card.Show(heading, detail, Theme.Accent);
            introRemaining = IntroSeconds;
        }

        public void ShowResult(string heading, string detail)
        {
            introRemaining = 0f;
            meters.gameObject.SetActive(false);
            keys.SetVisible(false);
            Map.SetVisible(false);
            notice.Hide();
            card.Show(heading, detail, Theme.Accent);
        }

        public void Tick(float deltaTime)
        {
            foreach (MeterView view in views.Values)
                view.Tick(deltaTime);
            notice.Tick(deltaTime);
            if (introRemaining > 0f)
            {
                introRemaining -= deltaTime;
                if (introRemaining <= 0f)
                    card.Hide();
            }

            card.Tick(deltaTime);
        }
    }
}
