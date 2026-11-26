using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class MeterView
    {
        public const float Height = 40f;

        private const float TrackThickness = 4f;
        private const float Gap = 16f;
        private const float AnimationSpeed = 12f;

        private readonly TextMeshProUGUI label;
        private readonly TextMeshProUGUI value;
        private readonly Image fill;

        private float shown;
        private float target;
        private bool emphasized;

        public MeterView(UiKit kit, Transform parent, float width, float labelWidth, float valueWidth)
        {
            Root = UiKit.NewRect("Meter", parent);
            Root.sizeDelta = new Vector2(width, Height);

            label = kit.NewText("Label", Root, Theme.Body, false, TextAlignmentOptions.MidlineLeft, Theme.TextDim);
            label.overflowMode = TextOverflowModes.Ellipsis;
            UiLayout.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(labelWidth - Gap, Height));

            Image track = kit.NewSolid("Track", Root, Theme.Line);
            UiLayout.Place(track.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(labelWidth, 0f), new Vector2(width - labelWidth - valueWidth - Gap, TrackThickness));
            fill = kit.NewSolid("Fill", track.rectTransform, Theme.Accent);
            UiLayout.Stretch(fill.rectTransform);

            value = kit.NewText("Value", Root, Theme.Body, true, TextAlignmentOptions.MidlineRight, Theme.TextDim);
            UiLayout.Place(value.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(valueWidth, Height));

            Apply();
        }

        public RectTransform Root { get; }

        public void SetLabel(string text)
        {
            label.text = text;
        }

        public void Set(float normalized, string text, Color color, bool snap = false)
        {
            target = Mathf.Clamp01(normalized);
            if (snap)
                shown = target;
            value.text = text;
            fill.color = color;
            Apply();
        }

        public void SetEmphasis(bool isEmphasized)
        {
            emphasized = isEmphasized;
            Apply();
        }

        public void Tick(float deltaTime)
        {
            shown = Mathf.Lerp(shown, target, 1f - Mathf.Exp(-AnimationSpeed * deltaTime));
            Apply();
        }

        private void Apply()
        {
            fill.rectTransform.anchorMax = new Vector2(shown, 1f);
            fill.rectTransform.offsetMax = Vector2.zero;
            label.color = emphasized ? Theme.Text : Theme.TextDim;
            value.color = emphasized ? Theme.Text : Theme.TextDim;
        }
    }
}
