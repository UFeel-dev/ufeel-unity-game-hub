using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class Toast
    {
        public const float DefaultSeconds = 1.6f;

        private const float FadeSeconds = 0.3f;
        private const float PaddingX = 24f;
        private const float PaddingY = 10f;
        private const float MaxTextWidth = 1100f;
        private const float ChipOpacity = 0.8f;
        private const float ChipRadius = 12f;

        private readonly RectTransform root;
        private readonly CanvasGroup group;
        private readonly TextMeshProUGUI text;
        private readonly Vector2 anchor;
        private readonly Vector2 position;

        private float remaining;

        public Toast(UiKit kit, Transform parent, Vector2 anchor, float edgeOffset, float size = Theme.Small)
        {
            this.anchor = anchor;
            position = new Vector2(0f, anchor.y > 0.5f ? -edgeOffset : edgeOffset);

            root = UiKit.NewRect("Toast", parent);
            group = root.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            Image chip = kit.NewRounded("Chip", root, Theme.Fade(Theme.Backdrop, ChipOpacity), ChipRadius);
            UiLayout.Stretch(chip.rectTransform);

            text = kit.NewText("Text", root, size, false, TextAlignmentOptions.Center, Theme.Text);
            text.textWrappingMode = TextWrappingModes.Normal;
            UiLayout.Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(PaddingX, PaddingY);
            text.rectTransform.offsetMax = new Vector2(-PaddingX, -PaddingY);
        }

        public void Show(string message, float seconds = DefaultSeconds)
        {
            text.text = message;
            Vector2 preferred = text.GetPreferredValues(message, MaxTextWidth, 0f);
            Vector2 size = new(Mathf.Min(preferred.x, MaxTextWidth) + (2f * PaddingX), preferred.y + (2f * PaddingY));
            UiLayout.Place(root, anchor, anchor, position, size);
            remaining = seconds + FadeSeconds;
            group.alpha = 1f;
        }

        public void Hide()
        {
            remaining = Mathf.Min(remaining, FadeSeconds);
        }

        public void Tick(float deltaTime)
        {
            remaining -= deltaTime;
            group.alpha = Mathf.Clamp01(remaining / FadeSeconds);
        }
    }
}
