using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class UiKit
    {
        public const int UiLayer = 5;

        private const float ShadeLength = 4000f;
        private const string RegularFontPath = "Fonts/Barlow Regular SDF";
        private const string BoldFontPath = "Fonts/Barlow SemiBold SDF";

        private static UiKit? shared;

        private readonly TMP_FontAsset? regular;
        private readonly TMP_FontAsset? bold;

        private UiKit(SpriteLibrary sprites, TMP_FontAsset? regular, TMP_FontAsset? bold)
        {
            Sprites = sprites;
            this.regular = regular;
            this.bold = bold;
        }

        public static UiKit Shared => shared ??= new UiKit(new SpriteLibrary(), Resources.Load<TMP_FontAsset>(RegularFontPath), Resources.Load<TMP_FontAsset>(BoldFontPath));

        public SpriteLibrary Sprites { get; }

        public static RectTransform NewRect(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform)) { layer = UiLayer };
            RectTransform rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static Image NewImage(string name, Transform parent, Sprite sprite, Color color)
        {
            Image image = NewRect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public Image NewSolid(string name, Transform parent, Color color)
        {
            return NewImage(name, parent, Sprites.Solid, color);
        }

        public Image NewRounded(string name, Transform parent, Color color, float radius)
        {
            Image image = NewImage(name, parent, Sprites.Pill, color);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = SpriteLibrary.PillBorder / Mathf.Max(1f, radius);
            return image;
        }

        public Image NewShade(string name, Transform parent, RectTransform.Edge edge, float depth, float opacity)
        {
            (Vector2 anchor, float angle) = edge switch
            {
                RectTransform.Edge.Bottom => (new Vector2(0.5f, 0f), 0f),
                RectTransform.Edge.Top => (new Vector2(0.5f, 1f), 180f),
                RectTransform.Edge.Left => (new Vector2(0f, 0.5f), -90f),
                _ => (new Vector2(1f, 0.5f), 90f),
            };

            Image image = NewImage(name, parent, Sprites.Fade, Theme.Fade(Theme.Backdrop, opacity));
            Vector2 inward = (new Vector2(0.5f, 0.5f) - anchor).normalized;
            UiLayout.Place(image.rectTransform, anchor, new Vector2(0.5f, 0.5f), inward * (depth / 2f), new Vector2(ShadeLength, depth));
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
            return image;
        }

        public TextMeshProUGUI NewText(string name, Transform parent, float size, bool isBold, TextAlignmentOptions alignment, Color color)
        {
            TextMeshProUGUI text = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            ApplyFont(text, isBold);
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.richText = true;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            return text;
        }

        public void ApplyFont(TMP_Text text, bool isBold)
        {
            TMP_FontAsset? font = isBold ? bold : regular;
            if (font != null)
                text.font = font;
            else if (isBold)
                text.fontStyle = FontStyles.Bold;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetShared()
        {
            shared?.Sprites.Release();
            shared = null;
        }
    }
}
