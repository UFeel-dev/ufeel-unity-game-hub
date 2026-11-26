using GameHub.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class OverlayView
    {
        private const float ToastOffset = 40f;

        private readonly Image flash;
        private readonly Toast toast;
        private readonly TextMeshProUGUI headline;

        private float flashDuration;
        private float flashAge = float.MaxValue;
        private Color flashColor;

        public OverlayView(UiKit kit, Transform parent)
        {
            headline = kit.NewText("Headline", parent, 160f, true, TextAlignmentOptions.Center, Color.clear);
            UiLayout.Place(headline.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1400f, 220f));

            toast = new Toast(kit, parent, new Vector2(0.5f, 1f), ToastOffset);

            flash = kit.NewSolid("Flash", parent, Color.clear);
            UiLayout.Stretch(flash.rectTransform);
        }

        public void Toast(string text)
        {
            toast.Show(text);
        }

        public void Flash(Color color, float duration)
        {
            flashColor = color;
            flashDuration = duration;
            flashAge = 0f;
        }

        public void SetHeadline(string text, Color color)
        {
            headline.text = text;
            headline.color = color;
        }

        public void Tick(float deltaTime)
        {
            toast.Tick(deltaTime);

            flashAge += deltaTime;
            float flashAlpha = flashDuration > 0f ? 1f - Mathf.Clamp01(flashAge / flashDuration) : 0f;
            flash.color = Theme.Fade(flashColor, flashColor.a * flashAlpha);
        }
    }
}
