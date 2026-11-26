using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class KeyBindingView
    {
        private const float ColumnWidth = 150f;
        private const float CapWidth = 120f;
        private const float CapHeight = 64f;
        private const float DotSize = 40f;
        private const float BlinkSpeed = 6f;

        private readonly RectTransform root;
        private readonly Image[] caps = new Image[LaneMask.LaneCount * KeyBindings.SlotCount];
        private readonly TextMeshProUGUI[] labels = new TextMeshProUGUI[LaneMask.LaneCount * KeyBindings.SlotCount];

        private int selected;
        private bool listening;
        private float time;

        public KeyBindingView(UiKit kit, Transform parent)
        {
            root = UiKit.NewRect("Keys", parent);
            UiLayout.Stretch(root);

            RectTransform column = UiKit.NewRect("Content", root);
            UiLayout.Place(column, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ColumnWidth * LaneMask.LaneCount, 360f));
            TextMeshProUGUI title = kit.NewText("Title", column, Theme.Title, true, TextAlignmentOptions.BottomLeft, Theme.Text);
            UiLayout.Row(title.rectTransform, 0f, 60f);
            title.text = "Keys";

            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                float x = (lane + 0.5f) * ColumnWidth;
                Image dot = UiKit.NewImage("Lane", column, kit.Sprites.Ring, Theme.Fade(Theme.Lane(lane), 0.8f));
                UiLayout.Place(dot.rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(x, -120f), Vector2.one * DotSize);
                for (int slot = 0; slot < KeyBindings.SlotCount; slot++)
                {
                    int index = Index(lane, slot);
                    caps[index] = kit.NewRounded("Key", column, Color.clear, 8f);
                    UiLayout.Place(caps[index].rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(x, -200f - (slot * (CapHeight + 14f))), new Vector2(CapWidth, CapHeight));
                    labels[index] = kit.NewText("Label", caps[index].rectTransform, Theme.Body, true, TextAlignmentOptions.Center, Theme.Text);
                    UiLayout.Stretch(labels[index].rectTransform);
                }
            }

            root.gameObject.SetActive(false);
        }

        public static int Index(int lane, int slot)
        {
            return (slot * LaneMask.LaneCount) + lane;
        }

        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        public RectTransform Cap(int index)
        {
            return caps[index].rectTransform;
        }

        public void SetLabel(int index, string label)
        {
            labels[index].text = label;
        }

        public void SetSelection(int index, bool isListening)
        {
            selected = index;
            listening = isListening;
        }

        public void Tick(float deltaTime)
        {
            time += deltaTime;
            for (int i = 0; i < caps.Length; i++)
            {
                bool isSelected = i == selected;
                float blink = isSelected && listening ? 0.5f + (0.5f * Mathf.Sin(time * BlinkSpeed)) : 1f;
                caps[i].color = isSelected ? Theme.Fade(Theme.Accent, 0.22f * blink) : Theme.Fade(Theme.Text, 0.07f);
                labels[i].color = isSelected ? Theme.Text : Theme.TextDim;
                labels[i].alpha = isSelected && listening ? blink : 1f;
            }
        }
    }
}
