using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class KeyBar
    {
        private const float CapHeight = 32f;
        private const float CapPadding = 10f;
        private const float CapGap = 5f;
        private const float LabelGap = 10f;
        private const float ItemGap = 34f;
        private const float ArrowSize = 12f;
        private const float BottomOffset = 36f;

        private readonly UiKit kit;
        private readonly RectTransform root;
        private readonly List<RectTransform> items = new();
        private readonly List<int> actions = new();

        public KeyBar(UiKit kit, Transform parent)
        {
            this.kit = kit;
            root = UiKit.NewRect("KeyBar", parent);
            UiLayout.Place(root, Vector2.zero, Vector2.zero, new Vector2(Theme.Margin, BottomOffset), new Vector2(1600f, CapHeight));
        }

        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        public void Set(IReadOnlyList<KeyHint> hints)
        {
            root.gameObject.SetActive(true);
            foreach (RectTransform item in items)
                Object.Destroy(item.gameObject);
            items.Clear();
            actions.Clear();

            float x = 0f;
            foreach (KeyHint hint in hints)
            {
                RectTransform item = UiKit.NewRect("Hint", root);
                float width = BuildItem(item, hint);
                UiLayout.Place(item, Vector2.zero, Vector2.zero, new Vector2(x, 0f), new Vector2(width, CapHeight));
                items.Add(item);
                actions.Add(hint.Action);
                x += width + ItemGap;
            }
        }

        public int Clicked(PointerInput mouse)
        {
            if (!mouse.Pressed || !root.gameObject.activeInHierarchy)
                return -1;

            for (int i = 0; i < items.Count; i++)
            {
                if (mouse.IsOver(items[i]))
                    return actions[i];
            }
            return -1;
        }

        private static float ArrowAngle(string key)
        {
            return key switch
            {
                KeyHint.Down => 180f,
                KeyHint.Left => 90f,
                KeyHint.Right => -90f,
                _ => 0f,
            };
        }

        private static bool IsArrow(string key)
        {
            return key is KeyHint.Up or KeyHint.Down or KeyHint.Left or KeyHint.Right;
        }

        private float BuildItem(RectTransform item, KeyHint hint)
        {
            float x = 0f;
            foreach (string key in hint.Keys)
            {
                x += BuildCap(item, key, x) + CapGap;
            }

            TextMeshProUGUI label = kit.NewText("Label", item, Theme.Small, false, TextAlignmentOptions.MidlineLeft, Theme.TextDim);
            label.text = hint.Label;
            float labelWidth = label.GetPreferredValues(hint.Label).x;
            float labelX = x - CapGap + LabelGap;
            UiLayout.Place(label.rectTransform, Vector2.zero, Vector2.zero, new Vector2(labelX, 0f), new Vector2(labelWidth + 4f, CapHeight));
            return labelX + labelWidth;
        }

        private float BuildCap(RectTransform item, string key, float x)
        {
            Image cap = kit.NewRounded("Key", item, Theme.Fade(Theme.Text, 0.12f), 6f);
            float width = CapHeight;
            if (IsArrow(key))
            {
                Image arrow = UiKit.NewImage("Arrow", cap.rectTransform, kit.Sprites.Triangle, Theme.Text);
                UiLayout.Place(arrow.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ArrowSize, ArrowSize));
                arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, ArrowAngle(key));
            }
            else
            {
                TextMeshProUGUI text = kit.NewText("Text", cap.rectTransform, Theme.Tiny, true, TextAlignmentOptions.Center, Theme.Text);
                text.text = key;
                width = Mathf.Max(CapHeight, text.GetPreferredValues(key).x + (2f * CapPadding));
                UiLayout.Stretch(text.rectTransform);
            }

            UiLayout.Place(cap.rectTransform, Vector2.zero, Vector2.zero, new Vector2(x, 0f), new Vector2(width, CapHeight));
            return width;
        }
    }
}
