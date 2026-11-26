using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class HudView
    {
        private const float PopSeconds = 0.08f;
        private const float JudgementPop = 1.2f;
        private const float ComboPop = 1.08f;
        private const float JudgementHold = 0.45f;
        private const float JudgementFade = 0.25f;
        private const float HeartGap = 56f;
        private const float HeartY = 150f;

        private readonly RectTransform root;
        private readonly TextMeshProUGUI combo;
        private readonly TextMeshProUGUI judgement;
        private readonly HitErrorBar timingBar;
        private readonly TextMeshProUGUI score;
        private readonly TextMeshProUGUI accuracy;
        private readonly HeartView heart;

        private float judgementAge = float.MaxValue;
        private float comboAge = float.MaxValue;
        private long shownScore = -1;
        private int shownAccuracy = -1;
        private int shownCombo = -1;

        public HudView(UiKit kit, Transform parent)
        {
            root = UiKit.NewRect("Hud", parent);
            UiLayout.Stretch(root);

            combo = kit.NewText("Combo", root, 72f, true, TextAlignmentOptions.Center, Theme.Fade(Theme.Text, 0.85f));
            combo.rectTransform.sizeDelta = new Vector2(PlayfieldView.Width, 90f);
            judgement = kit.NewText("Judgement", root, 40f, true, TextAlignmentOptions.Center, Color.clear);
            judgement.rectTransform.sizeDelta = new Vector2(PlayfieldView.Width, 56f);
            timingBar = new HitErrorBar(kit, root);

            score = kit.NewText("Score", root, Theme.Title, true, TextAlignmentOptions.TopRight, Theme.Text);
            UiLayout.Place(score.rectTransform, Vector2.one, Vector2.one, new Vector2(-Theme.Margin, -44f), new Vector2(520f, 60f));
            accuracy = kit.NewText("Accuracy", root, Theme.Body, false, TextAlignmentOptions.TopRight, Theme.TextDim);
            UiLayout.Place(accuracy.rectTransform, Vector2.one, Vector2.one, new Vector2(-Theme.Margin, -104f), new Vector2(520f, 36f));

            heart = new HeartView(kit, root, true);
            UiLayout.Place(heart.Root, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(-(PlayfieldView.Width / 2f) - HeartGap, HeartY), heart.Root.sizeDelta);

            ApplyLayout();
            root.gameObject.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        public RectTransform ElementRect(HudElement element)
        {
            return element switch
            {
                HudElement.Combo => combo.rectTransform,
                HudElement.Judgement => judgement.rectTransform,
                _ => timingBar.Root,
            };
        }

        public void ApplyLayout()
        {
            for (int i = 0; i < HudLayout.ElementCount; i++)
                SetElementY((HudElement)i, HudLayout.Y((HudElement)i));
        }

        public void SetElementY(HudElement element, float y)
        {
            RectTransform rect = ElementRect(element);
            UiLayout.Place(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), rect.sizeDelta);
        }

        public void ResetRun()
        {
            judgementAge = float.MaxValue;
            comboAge = float.MaxValue;
            shownScore = -1;
            shownAccuracy = -1;
            shownCombo = -1;
            timingBar.Clear();
        }

        public void ShowJudgement(in JudgementEvent judgementEvent)
        {
            judgement.text = Theme.JudgementLabel(judgementEvent.Judgement);
            judgement.color = Theme.Judgement(judgementEvent.Judgement);
            judgementAge = 0f;
            if (!judgementEvent.IsTail)
                timingBar.Add(judgementEvent.Offset, judgementEvent.Judgement);
        }

        public void Tick(float deltaTime, ScoreKeeper keeper)
        {
            UpdateScore(keeper);

            comboAge += deltaTime;
            combo.rectTransform.localScale = Vector3.one * Mathf.Lerp(ComboPop, 1f, comboAge / PopSeconds);

            judgementAge += deltaTime;
            judgement.alpha = 1f - Mathf.Clamp01((judgementAge - JudgementHold) / JudgementFade);
            judgement.rectTransform.localScale = Vector3.one * Mathf.Lerp(JudgementPop, 1f, judgementAge / PopSeconds);
            timingBar.Tick(deltaTime);
        }

        public void TickHeart(float deltaTime, float bpm, bool hasReading, float pulse, float level, float rate, bool flat)
        {
            Color color = Theme.Level(level);
            string caption = FormattableString.Invariant($"Level <color=#{Theme.Hex(color)}>{level:0.0}</color>");
            if (rate > 0f)
                caption += FormattableString.Invariant($"   Speed x{rate:0.00}");
            heart.Tick(deltaTime, bpm, hasReading, pulse, color, caption, flat);
        }

        private void UpdateScore(ScoreKeeper keeper)
        {
            if (keeper.Score != shownScore)
            {
                shownScore = keeper.Score;
                score.text = keeper.Score.ToString("N0", CultureInfo.InvariantCulture);
            }

            int hundredths = (int)Math.Round(keeper.Accuracy * 10000d);
            if (hundredths != shownAccuracy)
            {
                shownAccuracy = hundredths;
                accuracy.text = (hundredths / 100d).ToString("0.00", CultureInfo.InvariantCulture) + "%";
            }

            if (keeper.Combo == shownCombo)
                return;

            if (keeper.Combo > shownCombo)
                comboAge = 0f;
            shownCombo = keeper.Combo;
            if (keeper.Combo >= 2)
                combo.SetText("{0}", keeper.Combo);
            else
                combo.SetText(string.Empty);
        }
    }
}
