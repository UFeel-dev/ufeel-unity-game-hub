using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class SliderView
    {
        private const float Height = 48f;
        private const float LabelWidth = 150f;
        private const float ValueWidth = 80f;
        private const float TrackThickness = 2f;
        private const float KnobSize = 18f;
        private const float FocusedKnobSize = 24f;
        private const float AnimationSpeed = 16f;

        private readonly TextMeshProUGUI label;
        private readonly TextMeshProUGUI value;
        private readonly RectTransform track;
        private readonly RectTransform root;
        private readonly Image fill;
        private readonly Image knob;

        private float focus;
        private bool focused;
        private bool dragging;

        public SliderView(UiKit kit, Transform parent, string name, float top, float width)
        {
            root = UiKit.NewRect(name, parent);
            UiLayout.Place(root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -top), new Vector2(width, Height));

            label = kit.NewText("Label", root, Theme.Body, false, TextAlignmentOptions.MidlineLeft, Theme.TextDim);
            UiLayout.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(LabelWidth, Height));
            label.text = name;

            track = kit.NewSolid("Track", root, Theme.Line).rectTransform;
            UiLayout.Place(track, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(LabelWidth, 0f), new Vector2(width - LabelWidth - ValueWidth - 16f, TrackThickness));
            fill = kit.NewSolid("Fill", track, Theme.Accent);
            UiLayout.Stretch(fill.rectTransform);
            knob = UiKit.NewImage("Knob", track, kit.Sprites.Circle, Theme.Text);

            value = kit.NewText("Value", root, Theme.Body, true, TextAlignmentOptions.MidlineRight, Theme.Text);
            UiLayout.Place(value.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(ValueWidth, Height));
        }

        public void Set(float normalized, string text, Color color)
        {
            normalized = Mathf.Clamp01(normalized);
            fill.rectTransform.anchorMax = new Vector2(normalized, 1f);
            fill.rectTransform.offsetMax = Vector2.zero;
            fill.color = color;
            UiLayout.Place(knob.rectTransform, new Vector2(normalized, 0.5f), Vector2.zero, Vector2.one * KnobSize);
            value.text = text;
        }

        public void SetFocus(bool isFocused)
        {
            focused = isFocused;
        }

        public float? Drag(PointerInput mouse)
        {
            if (mouse.Pressed && mouse.IsOver(root))
                dragging = true;
            if (!mouse.Held)
                dragging = false;
            if (!dragging)
                return null;

            float local = mouse.LocalIn(track).x + (track.rect.width * track.pivot.x);
            return Mathf.Clamp01(local / track.rect.width);
        }

        public void Tick(float deltaTime)
        {
            focus = Mathf.Lerp(focus, focused ? 1f : 0f, 1f - Mathf.Exp(-AnimationSpeed * deltaTime));
            label.color = Color.Lerp(Theme.TextDim, Theme.Text, focus);
            knob.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(KnobSize, FocusedKnobSize, focus);
            knob.color = Color.Lerp(Theme.TextDim, Theme.Text, focus);
        }
    }
}
