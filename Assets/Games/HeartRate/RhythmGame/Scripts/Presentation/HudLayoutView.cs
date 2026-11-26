using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class HudLayoutView
    {
        private const float Padding = 10f;
        private const float Thickness = 2f;

        private readonly RectTransform frame;
        private readonly Image[] edges = new Image[4];
        private readonly TextMeshProUGUI label;

        public HudLayoutView(UiKit kit, Transform parent)
        {
            frame = UiKit.NewRect("Selection", parent);
            for (int i = 0; i < edges.Length; i++)
                edges[i] = kit.NewSolid("Edge", frame, Theme.Accent);

            UiLayout.Place(edges[0].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiLayout.Place(edges[1].rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiLayout.Place(edges[2].rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiLayout.Place(edges[3].rectTransform, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            label = kit.NewText("Label", frame, Theme.Small, true, TextAlignmentOptions.BottomLeft, Theme.Accent);
            UiLayout.Place(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 6f), new Vector2(300f, 28f));

            frame.gameObject.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            frame.gameObject.SetActive(visible);
        }

        public void Frame(RectTransform target, string text)
        {
            Vector2 size = target.sizeDelta + (Vector2.one * (Padding * 2f));
            frame.SetParent(target.parent, false);
            frame.SetAsLastSibling();
            UiLayout.Place(frame, target.anchorMin, new Vector2(0.5f, 0.5f), target.anchoredPosition, size);
            edges[0].rectTransform.sizeDelta = new Vector2(size.x, Thickness);
            edges[1].rectTransform.sizeDelta = new Vector2(size.x, Thickness);
            edges[2].rectTransform.sizeDelta = new Vector2(Thickness, size.y);
            edges[3].rectTransform.sizeDelta = new Vector2(Thickness, size.y);
            label.text = text;
        }
    }
}
