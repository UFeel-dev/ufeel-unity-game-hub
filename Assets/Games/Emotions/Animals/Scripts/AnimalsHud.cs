using System;
using System.Globalization;
using GameHub.Common;
using UnityEngine;

namespace GameHub.Animals
{
    public sealed class AnimalsHud
    {
        private const float StatWidth = 320f;
        private const float StatTop = 44f;
        private const float StatGap = 16f;
        private const float MeterWidth = 720f;
        private const float MeterBottom = 64f;
        private const float TopShadeDepth = 260f;
        private const float BottomShadeDepth = 220f;
        private const float ShadeOpacity = 0.6f;
        private const float CardOpacity = 0.55f;
        private const float WarningSeconds = 10f;

        private readonly int levelCount;
        private readonly StatView score;
        private readonly StatView time;
        private readonly StatView level;
        private readonly MeterView meter;
        private readonly TitleCard card;

        public AnimalsHud(UiKit kit, Transform owner, int levelCount)
        {
            this.levelCount = levelCount;
            RectTransform canvas = UiCanvas.Create("AnimalsHud", owner, UiCanvas.GameOrder);
            kit.NewShade("TopShade", canvas, RectTransform.Edge.Top, TopShadeDepth, ShadeOpacity);
            kit.NewShade("BottomShade", canvas, RectTransform.Edge.Bottom, BottomShadeDepth, ShadeOpacity);

            score = new StatView(kit, canvas, "Score", true, Theme.Title);
            UiLayout.Place(score.Root, Vector2.one, Vector2.one, new Vector2(-Theme.Margin, -StatTop), new Vector2(StatWidth, score.Height));
            time = new StatView(kit, canvas, "Time", true);
            UiLayout.Place(time.Root, Vector2.one, Vector2.one, new Vector2(-Theme.Margin, -(StatTop + score.Height + StatGap)), new Vector2(StatWidth, time.Height));
            level = new StatView(kit, canvas, "Level", false, Theme.Title);
            UiLayout.Place(level.Root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(Theme.Margin, -StatTop), new Vector2(StatWidth, level.Height));

            meter = new MeterView(kit, canvas, MeterWidth, 200f, 90f);
            UiLayout.Place(meter.Root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, MeterBottom), meter.Root.sizeDelta);
            meter.Root.gameObject.SetActive(false);

            card = new TitleCard(kit, canvas, CardOpacity);
            SetScore(0);
        }

        public void SetLevel(int value)
        {
            level.Set(FormattableString.Invariant($"{value}<size=60%><color=#{Theme.Hex(Theme.TextDim)}> / {levelCount}</color></size>"));
        }

        public void SetScore(int value)
        {
            score.Set(value.ToString(CultureInfo.InvariantCulture));
        }

        public void SetTime(float seconds)
        {
            int whole = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            time.Set(FormattableString.Invariant($"{whole / 60}:{whole % 60:00}"), seconds <= WarningSeconds ? Theme.Danger : Theme.Text);
        }

        public void ShowAnimal(string animal)
        {
            meter.SetLabel(animal);
            meter.Set(0f, "0%", Theme.Danger, true);
            meter.SetEmphasis(false);
            meter.Root.gameObject.SetActive(true);
        }

        public void HideAnimal()
        {
            meter.Root.gameObject.SetActive(false);
        }

        public void SetProgress(float progress, bool matching)
        {
            meter.Set(progress, FormattableString.Invariant($"{Mathf.RoundToInt(progress * 100f)}%"), matching ? Theme.Accent : Theme.Danger);
            meter.SetEmphasis(matching);
        }

        public void ShowCard(string title, string subtitle, Color accent)
        {
            card.Show(title, subtitle, accent);
        }

        public void HideCard()
        {
            card.Hide();
        }

        public void Tick(float deltaTime)
        {
            meter.Tick(deltaTime);
            card.Tick(deltaTime);
        }
    }
}
