using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class UiKit
    {
        private const int UiLayer = 5;

        private readonly TMP_FontAsset? regular;
        private readonly TMP_FontAsset? bold;

        public UiKit(SpriteLibrary sprites, TMP_FontAsset? regular, TMP_FontAsset? bold)
        {
            Sprites = sprites;
            this.regular = regular;
            this.bold = bold;
        }

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

        public TextMeshProUGUI NewText(string name, Transform parent, float size, bool isBold, TextAlignmentOptions alignment, Color color)
        {
            TextMeshProUGUI text = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset? font = isBold ? bold : regular;
            if (font != null)
                text.font = font;
            else if (isBold)
                text.fontStyle = FontStyles.Bold;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.richText = true;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            return text;
        }
    }
}
