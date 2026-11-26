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
        private const float Bottom = 48f;
        private const float ShadeDepth = 300f;
        private const float ShadeOpacity = 0.6f;
        private const float CardOpacity = 0.6f;
        private const string Open = "Open";

        private readonly RectTransform meters;
        private readonly Dictionary<EmotionType, MeterView> views = new();
        private readonly TitleCard card;

        public MazeHud(UiKit kit, Transform owner, IReadOnlyList<EmotionInfo> emotions)
        {
            RectTransform canvas = UiCanvas.Create("MazeHud", owner, UiCanvas.GameOrder);
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
            }

            card = new TitleCard(kit, canvas, CardOpacity);
        }

        public void SetLevel(EmotionType emotion, float progress, bool dominant)
        {
            bool open = progress >= 1f;
            Color color = Theme.TextFaint;
            if (dominant)
                color = open ? Theme.Accent : Theme.Warning;
            MeterView view = views[emotion];
            view.Set(progress, open ? Open : FormattableString.Invariant($"{progress * 100f:0}%"), color);
            view.SetEmphasis(dominant);
        }

        public void ShowWin()
        {
            meters.gameObject.SetActive(false);
            card.Show("You won", "Congratulations! Back to the hub", Theme.Accent);
        }

        public void Tick(float deltaTime)
        {
            foreach (MeterView view in views.Values)
                view.Tick(deltaTime);
            card.Tick(deltaTime);
        }
    }
}
