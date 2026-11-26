using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class GazeIndicator
    {
        public const float Height = 96f;

        private const float Width = 420f;
        private const float ArrowSize = 26f;
        private const float ArrowOffset = 64f;
        private const float RingSize = 40f;
        private const float IconY = 24f;
        private const string Instructions = "Look left or right to move, close your eyes to select";

        private readonly Image left;
        private readonly Image right;
        private readonly Image ring;
        private readonly Image disc;

        public GazeIndicator(UiKit kit, Transform parent)
        {
            Root = UiKit.NewRect("Gaze", parent);
            Root.sizeDelta = new Vector2(Width, Height);

            Vector2 top = new(0.5f, 1f);
            left = Arrow(kit, "Left", 90f, new Vector2(-ArrowOffset, -IconY));
            right = Arrow(kit, "Right", -90f, new Vector2(ArrowOffset, -IconY));

            ring = UiKit.NewImage("Ring", Root, kit.Sprites.Ring, Theme.TextFaint);
            UiLayout.Place(ring.rectTransform, top, new Vector2(0.5f, 0.5f), new Vector2(0f, -IconY), Vector2.one * RingSize);
            disc = UiKit.NewImage("Disc", ring.rectTransform, kit.Sprites.Circle, Theme.Accent);
            UiLayout.Place(disc.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * RingSize);

            TextMeshProUGUI caption = kit.NewText("Caption", Root, Theme.Tiny, false, TextAlignmentOptions.Top, Theme.TextDim);
            UiLayout.Row(caption.rectTransform, (2f * IconY) + 8f, 28f);
            caption.text = Instructions;
        }

        public RectTransform Root { get; }

        public void Set(int direction, float stepProgress, float confirmProgress)
        {
            left.color = ArrowColor(direction < 0, stepProgress);
            right.color = ArrowColor(direction > 0, stepProgress);
            ring.color = confirmProgress > 0f ? Theme.Accent : Theme.TextFaint;
            disc.rectTransform.localScale = Vector3.one * confirmProgress;
        }

        private static Color ArrowColor(bool active, float progress)
        {
            return active ? Color.Lerp(Theme.Text, Theme.Accent, progress) : Theme.TextFaint;
        }

        private Image Arrow(UiKit kit, string name, float angle, Vector2 position)
        {
            Image arrow = UiKit.NewImage(name, Root, kit.Sprites.Triangle, Theme.TextFaint);
            UiLayout.Place(arrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), position, Vector2.one * ArrowSize);
            arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
            return arrow;
        }
    }
}
