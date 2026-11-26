using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class ListScreen
    {
        private const float Width = 640f;
        private const float RowHeight = 60f;
        private const float TitleHeight = 80f;

        private readonly RectTransform root;
        private readonly TextMeshProUGUI title;

        public ListScreen(UiKit kit, Transform parent, string name, int rows)
        {
            root = UiKit.NewRect(name, parent);
            UiLayout.Stretch(root);

            RectTransform column = UiKit.NewRect("Column", root);
            float height = TitleHeight + (rows * RowHeight);
            UiLayout.Place(column, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Width, height));

            title = kit.NewText("Title", column, Theme.Title, true, TextAlignmentOptions.BottomLeft, Theme.Text);
            UiLayout.Row(title.rectTransform, 0f, TitleHeight - 20f);
            title.margin = new Vector4(22f, 0f, 0f, 0f);

            RectTransform list = UiKit.NewRect("List", column);
            UiLayout.Row(list, TitleHeight, rows * RowHeight);
            List = new MenuList(kit, list, rows, RowHeight, Theme.Heading);

            root.gameObject.SetActive(false);
        }

        public MenuList List { get; }

        public void Show(string heading, IReadOnlyList<MenuEntry> entries, int selected)
        {
            title.text = heading;
            List.SetItems(entries, selected);
            root.gameObject.SetActive(true);
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
