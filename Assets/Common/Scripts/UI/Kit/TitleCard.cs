using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class TitleCard
    {
        private const float Width = 1400f;
        private static readonly Vector2 AccentSize = new(56f, 3f);

        private readonly RectTransform root;
        private readonly CanvasGroup group;
        private readonly Image accent;
        private readonly TextMeshProUGUI title;
        private readonly TextMeshProUGUI subtitle;
        private readonly Fader fader = new();

        public TitleCard(UiKit kit, Transform parent, float scrimOpacity)
        {
            root = UiKit.NewRect("TitleCard", parent);
            UiLayout.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            Image scrim = kit.NewSolid("Scrim", root, Theme.Fade(Theme.Backdrop, scrimOpacity));
            UiLayout.Stretch(scrim.rectTransform);

            Vector2 center = new(0.5f, 0.5f);
            accent = kit.NewSolid("Accent", root, Theme.Accent);
            UiLayout.Place(accent.rectTransform, center, new Vector2(0f, 76f), AccentSize);

            title = kit.NewText("Title", root, Theme.Display, true, TextAlignmentOptions.Center, Theme.Text);
            UiLayout.Place(title.rectTransform, center, new Vector2(0f, 12f), new Vector2(Width, 96f));

            subtitle = kit.NewText("Subtitle", root, Theme.Heading, false, TextAlignmentOptions.Top, Theme.TextDim);
            subtitle.textWrappingMode = TextWrappingModes.Normal;
            UiLayout.Place(subtitle.rectTransform, center, new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(Width, 200f));

            Apply();
        }

        public void Show(string heading, string detail, Color color, float fadeSeconds = Fader.DefaultSeconds)
        {
            title.text = heading;
            subtitle.text = detail;
            accent.color = color;
            fader.To(true, fadeSeconds);
            Apply();
        }

        public void Hide(float fadeSeconds = Fader.DefaultSeconds)
        {
            fader.To(false, fadeSeconds);
            Apply();
        }

        public void Tick(float deltaTime)
        {
            fader.Tick(deltaTime);
            Apply();
        }

        private void Apply()
        {
            group.alpha = fader.Value;
            root.gameObject.SetActive(fader.Value > 0f);
        }
    }
}
