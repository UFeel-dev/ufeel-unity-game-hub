using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class HitErrorBar
    {
        private const float Width = 260f;
        private const float Height = 22f;
        private const float BandHeight = 4f;
        private const float TickWidth = 2f;
        private const float TickLife = 2.5f;
        private const int TickCount = 24;
        private const int AverageWindow = 12;
        private const float MarkerSize = 10f;
        private const float AverageSpeed = 10f;

        private readonly List<Image> ticks = new(TickCount);
        private readonly float[] ages = new float[TickCount];
        private readonly double[] recent = new double[AverageWindow];
        private readonly RectTransform marker;

        private int nextTick;
        private int recentCount;
        private int recentCursor;
        private float markerX;

        public HitErrorBar(UiKit kit, Transform parent)
        {
            Root = UiKit.NewRect("TimingBar", parent);
            Root.sizeDelta = new Vector2(Width, Height);

            Band(kit, HitWindows.Meh, Judgement.Meh);
            Band(kit, HitWindows.Good, Judgement.Good);
            Band(kit, HitWindows.Great, Judgement.Great);
            Band(kit, HitWindows.Perfect, Judgement.Perfect);

            Image center = kit.NewSolid("Center", Root, Theme.Text);
            UiLayout.Place(center.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2f, Height));

            for (int i = 0; i < TickCount; i++)
            {
                Image tick = kit.NewSolid("Tick", Root, Color.clear);
                UiLayout.Place(tick.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(TickWidth, Height - 6f));
                ticks.Add(tick);
                ages[i] = TickLife;
            }

            marker = UiKit.NewImage("Average", Root, kit.Sprites.Triangle, Theme.Text).rectTransform;
            UiLayout.Place(marker, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 3f), Vector2.one * MarkerSize);
            marker.localRotation = Quaternion.Euler(0f, 0f, 180f);
        }

        public RectTransform Root { get; }

        public static float Position(double offset)
        {
            return (float)(offset / HitWindows.Meh) * (Width / 2f);
        }

        public void Add(double offset, Judgement judgement)
        {
            if (judgement == Judgement.Miss)
                return;

            Image tick = ticks[nextTick];
            tick.rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(Position(offset), -Width / 2f, Width / 2f), 0f);
            tick.color = RhythmTheme.Judgement(judgement);
            ages[nextTick] = 0f;
            nextTick = (nextTick + 1) % TickCount;

            recent[recentCursor] = offset;
            recentCursor = (recentCursor + 1) % AverageWindow;
            recentCount = Mathf.Min(recentCount + 1, AverageWindow);
        }

        public void Clear()
        {
            for (int i = 0; i < TickCount; i++)
            {
                ages[i] = TickLife;
                ticks[i].color = Color.clear;
            }
            recentCount = 0;
            markerX = 0f;
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < TickCount; i++)
            {
                if (ages[i] >= TickLife)
                    continue;

                ages[i] += deltaTime;
                Color color = ticks[i].color;
                ticks[i].color = Theme.Fade(color, Mathf.Clamp01(1f - (ages[i] / TickLife)));
            }

            double sum = 0d;
            for (int i = 0; i < recentCount; i++)
                sum += recent[i];
            float target = recentCount > 0 ? Position(sum / recentCount) : 0f;
            markerX = Mathf.Lerp(markerX, target, 1f - Mathf.Exp(-AverageSpeed * deltaTime));
            marker.anchoredPosition = new Vector2(markerX, 3f);
        }

        private void Band(UiKit kit, double window, Judgement judgement)
        {
            Image band = kit.NewSolid("Band", Root, Theme.Fade(RhythmTheme.Judgement(judgement), 0.35f));
            UiLayout.Place(band.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Position(window) * 2f, BandHeight));
        }
    }
}
