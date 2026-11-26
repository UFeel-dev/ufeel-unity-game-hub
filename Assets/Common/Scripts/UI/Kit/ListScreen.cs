using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class ListScreen
    {
        public const float DefaultWidth = 640f;

        public const float DefaultRowHeight = 60f;

        private const float TitleHeight = 80f;
        private const float FooterGap = 12f;
        private const float FooterHeight = 32f;
        private const float TextInset = 22f;

        private readonly RectTransform root;
        private readonly TextMeshProUGUI title;
        private readonly TextMeshProUGUI footer;

        public ListScreen(UiKit kit, Transform parent, string name, int rows)
            : this(kit, parent, name, rows, new Vector2(0.5f, 0.5f), Vector2.zero, DefaultWidth, DefaultRowHeight)
        {
        }

        public ListScreen(UiKit kit, Transform parent, string name, int rows, Vector2 anchor, Vector2 position, float width, float rowHeight)
        {
            root = UiKit.NewRect(name, parent);
            UiLayout.Stretch(root);

            RectTransform column = UiKit.NewRect("Column", root);
            float listHeight = rows * rowHeight;
            UiLayout.Place(column, anchor, anchor, position, new Vector2(width, TitleHeight + listHeight));

            title = kit.NewText("Title", column, Theme.Title, true, TextAlignmentOptions.BottomLeft, Theme.Text);
            UiLayout.Row(title.rectTransform, 0f, TitleHeight - 20f);
            title.margin = new Vector4(TextInset, 0f, 0f, 0f);

            RectTransform list = UiKit.NewRect("List", column);
            UiLayout.Row(list, TitleHeight, listHeight);
            List = new MenuList(kit, list, rows, rowHeight, Theme.Heading);

            footer = kit.NewText("Footer", column, Theme.Small, false, TextAlignmentOptions.TopLeft, Theme.TextDim);
            UiLayout.Row(footer.rectTransform, TitleHeight + listHeight + FooterGap, FooterHeight);
            footer.margin = new Vector4(TextInset, 0f, 0f, 0f);

            root.gameObject.SetActive(false);
        }

        public MenuList List { get; }

        public void Show(string heading, IReadOnlyList<MenuEntry> entries, int selected)
        {
            title.text = heading;
            List.SetItems(entries, selected);
            root.gameObject.SetActive(true);
        }

        public void SetVisible(bool visible)
        {
            if (root.gameObject.activeSelf != visible)
                root.gameObject.SetActive(visible);
        }

        public void SetFooter(string text)
        {
            footer.text = text;
        }

        public void Hide()
        {
            root.gameObject.SetActive(false);
        }

        public void Tick(float deltaTime)
        {
            if (root.gameObject.activeSelf)
                List.Tick(deltaTime);
        }
    }
}
