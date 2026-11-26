using TMPro;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class StatView
    {
        private const float CaptionHeight = 26f;
        private const float CaptionGap = 2f;

        private readonly TextMeshProUGUI value;

        public StatView(UiKit kit, Transform parent, string caption, bool alignRight, float valueSize = Theme.Heading)
        {
            Root = UiKit.NewRect(caption, parent);
            float valueHeight = valueSize * 1.3f;
            Root.sizeDelta = new Vector2(Root.sizeDelta.x, valueHeight + CaptionGap + CaptionHeight);

            value = kit.NewText("Value", Root, valueSize, true, alignRight ? TextAlignmentOptions.BottomRight : TextAlignmentOptions.BottomLeft, Theme.Text);
            value.overflowMode = TextOverflowModes.Ellipsis;
            UiLayout.Row(value.rectTransform, 0f, valueHeight);

            TextMeshProUGUI label = kit.NewText("Caption", Root, Theme.Tiny, false, alignRight ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft, Theme.TextDim);
            UiLayout.Row(label.rectTransform, valueHeight + CaptionGap, CaptionHeight);
            label.text = caption;
        }

        public RectTransform Root { get; }

        public float Height => Root.sizeDelta.y;

        public void Set(string text)
        {
            value.text = text;
        }

        public void Set(string text, Color color)
        {
            value.text = text;
            value.color = color;
        }
    }
}
