using System.Collections.Generic;
using GameHub.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class MenuList
    {
        private const float RepeatDelay = 0.35f;
        private const float RepeatInterval = 0.07f;
        private const float AnimationSpeed = 18f;
        private const float BarWidth = 3f;
        private const float TextInset = 22f;
        private const float SelectedShift = 6f;

        private readonly RectTransform area;
        private readonly List<Row> rows = new();
        private readonly List<MenuEntry> items = new();

        private int windowStart;
        private int repeatDirection;
        private float repeatTimer;

        public MenuList(UiKit kit, RectTransform area, int visibleRows, float rowHeight, float labelSize)
        {
            this.area = area;
            for (int i = 0; i < visibleRows; i++)
                rows.Add(new Row(kit, area, i, rowHeight, labelSize));
        }

        public int Selected { get; private set; }

        public bool UseVerticalKeys { get; set; } = true;

        public void SetItems(IReadOnlyList<MenuEntry> source, int selection)
        {
            items.Clear();
            for (int i = 0; i < source.Count; i++)
                items.Add(source[i]);
            Selected = items.Count == 0 ? 0 : Mathf.Clamp(selection, 0, items.Count - 1);
            windowStart = 0;
            Refresh(true);
        }

        public void SetItem(int index, MenuEntry item)
        {
            items[index] = item;
            Refresh(false);
        }

        public MenuAction HandleInput(float deltaTime, PointerInput mouse)
        {
            if (MenuKeys.Back())
                return MenuAction.Back;
            if (MenuKeys.Confirm() || LayoutInput.GetKeyDown(KeyCode.Space))
                return items.Count > 0 ? MenuAction.Confirmed : MenuAction.None;

            MenuAction pointerAction = HandlePointer(mouse);
            if (pointerAction != MenuAction.None)
                return pointerAction;

            int direction = Direction();
            if (direction == 0)
            {
                repeatDirection = 0;
                return MenuAction.None;
            }

            if (direction != repeatDirection)
            {
                repeatDirection = direction;
                repeatTimer = RepeatDelay;
                return Apply(direction);
            }

            repeatTimer -= deltaTime;
            if (repeatTimer > 0f)
                return MenuAction.None;

            repeatTimer = RepeatInterval;
            return Apply(direction);
        }

        public MenuAction Step(int step)
        {
            if (items.Count < 2)
                return MenuAction.None;

            Selected = (Selected + step + items.Count) % items.Count;
            Refresh(false);
            return MenuAction.Moved;
        }

        public void Tick(float deltaTime)
        {
            float blend = 1f - Mathf.Exp(-AnimationSpeed * deltaTime);
            for (int i = 0; i < rows.Count; i++)
                rows[i].Animate(windowStart + i == Selected, blend);
        }

        private int Direction()
        {
            if (UseVerticalKeys && MenuKeys.Up(true))
                return 1;
            if (UseVerticalKeys && MenuKeys.Down(true))
                return 2;
            if (LayoutInput.GetKey(KeyCode.LeftArrow) || LayoutInput.GetKey(KeyCode.A))
                return 3;
            return LayoutInput.GetKey(KeyCode.RightArrow) || LayoutInput.GetKey(KeyCode.D) ? 4 : 0;
        }

        private MenuAction Apply(int direction)
        {
            return direction switch
            {
                1 => Step(-1),
                2 => Step(1),
                3 => MenuAction.Decrease,
                _ => MenuAction.Increase,
            };
        }

        private MenuAction HandlePointer(PointerInput mouse)
        {
            if (mouse.Scroll != 0f && mouse.IsOver(area))
                return Step(mouse.Scroll > 0f ? -1 : 1);

            for (int i = 0; i < rows.Count && windowStart + i < items.Count; i++)
            {
                if (!mouse.IsOver(rows[i].Rect))
                    continue;

                if (mouse.Pressed)
                {
                    Selected = windowStart + i;
                    return MenuAction.Confirmed;
                }

                if (mouse.Moved && Selected != windowStart + i)
                {
                    Selected = windowStart + i;
                    return MenuAction.Moved;
                }
            }
            return MenuAction.None;
        }

        private void Refresh(bool snap)
        {
            if (Selected < windowStart)
                windowStart = Selected;
            else if (Selected >= windowStart + rows.Count)
                windowStart = Selected - rows.Count + 1;

            for (int i = 0; i < rows.Count; i++)
            {
                int index = windowStart + i;
                rows[i].Bind(index < items.Count ? items[index] : null);
                if (snap)
                    rows[i].Animate(index == Selected, 1f);
            }
        }

        private sealed class Row
        {
            private readonly RectTransform content;
            private readonly Image highlight;
            private readonly Image bar;
            private readonly TextMeshProUGUI label;
            private readonly TextMeshProUGUI caption;
            private readonly TextMeshProUGUI detail;

            private float selection;

            public Row(UiKit kit, RectTransform parent, int index, float height, float labelSize)
            {
                Rect = UiKit.NewRect("Row", parent);
                UiLayout.Row(Rect, index * height, height);

                highlight = kit.NewSolid("Highlight", Rect, Color.clear);
                UiLayout.Stretch(highlight.rectTransform);
                bar = kit.NewSolid("Bar", Rect, Theme.Accent);
                UiLayout.Place(bar.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(BarWidth, height - 16f));

                content = UiKit.NewRect("Content", Rect);
                UiLayout.Stretch(content);
                content.offsetMin = new Vector2(TextInset, 0f);
                content.offsetMax = new Vector2(-TextInset, 0f);

                label = kit.NewText("Label", content, labelSize, true, TextAlignmentOptions.MidlineLeft, Theme.Text);
                label.overflowMode = TextOverflowModes.Ellipsis;
                caption = kit.NewText("Caption", content, Theme.Small, false, TextAlignmentOptions.TopLeft, Theme.TextFaint);
                detail = kit.NewText("Detail", content, Theme.Body, true, TextAlignmentOptions.MidlineRight, Theme.TextDim);
                UiLayout.Stretch(detail.rectTransform);
            }

            public RectTransform Rect { get; }

            public void Bind(MenuEntry? item)
            {
                Rect.gameObject.SetActive(item.HasValue);
                if (!item.HasValue)
                    return;

                bool hasCaption = !string.IsNullOrEmpty(item.Value.Caption);
                label.text = item.Value.Label;
                caption.text = item.Value.Caption;
                detail.text = item.Value.Detail;
                label.alignment = hasCaption ? TextAlignmentOptions.BottomLeft : TextAlignmentOptions.MidlineLeft;
                UiLayout.Stretch(label.rectTransform);
                UiLayout.Stretch(caption.rectTransform);
                if (hasCaption)
                {
                    label.rectTransform.offsetMin = new Vector2(0f, Rect.rect.height * 0.48f);
                    caption.rectTransform.offsetMax = new Vector2(0f, -Rect.rect.height * 0.56f);
                }
                label.margin = new Vector4(0f, 0f, string.IsNullOrEmpty(item.Value.Detail) ? 0f : 140f, 0f);
            }

            public void Animate(bool isSelected, float blend)
            {
                selection = Mathf.Lerp(selection, isSelected ? 1f : 0f, blend);
                highlight.color = Theme.Fade(Theme.Accent, 0.06f * selection);
                bar.color = Theme.Fade(Theme.Accent, selection);
                label.color = Color.Lerp(Theme.TextDim, Theme.Text, selection);
                caption.color = Color.Lerp(Theme.TextFaint, Theme.TextDim, selection);
                content.anchoredPosition = new Vector2(SelectedShift * selection, 0f);
            }
        }
    }
}
