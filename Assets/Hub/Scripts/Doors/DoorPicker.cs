using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;

namespace GameHub.Hub
{
    public sealed class DoorPicker
    {
        private const int MaxRows = 7;
        private const float Width = 480f;
        private const float RowHeight = 76f;
        private const float ShadeDepth = 760f;
        private const float ShadeOpacity = 0.85f;
        private const float KeyShadeDepth = 150f;
        private const float KeyShadeOpacity = 0.6f;
        private const float IdleAlpha = 0.55f;
        private const float FadeSpeed = 6f;
        private const float GazeOffset = 60f;

        private readonly RectTransform canvas;
        private readonly CanvasGroup menu;
        private readonly ListScreen screen;
        private readonly KeyBar keys;
        private readonly GazeIndicator gaze;
        private readonly List<KeyHint> hints = new();

        private (bool UsingMenu, bool InCategory, bool EyeControl, string? EnterLabel)? shownHints;

        public DoorPicker(UiKit kit, Transform owner, int rows)
        {
            canvas = UiCanvas.Create("DoorPicker", owner, UiCanvas.HubOrder);
            kit.NewShade("Shade", canvas, RectTransform.Edge.Left, ShadeDepth, ShadeOpacity);
            kit.NewShade("KeyShade", canvas, RectTransform.Edge.Bottom, KeyShadeDepth, KeyShadeOpacity);

            RectTransform menuRoot = UiKit.NewRect("Menu", canvas);
            UiLayout.Stretch(menuRoot);
            menu = menuRoot.gameObject.AddComponent<CanvasGroup>();
            menu.interactable = false;
            menu.blocksRaycasts = false;

            screen = new ListScreen(kit, menuRoot, "Doors", Mathf.Clamp(rows, 1, MaxRows), new Vector2(0f, 0.5f), new Vector2(Theme.Margin - 22f, 0f), Width, RowHeight);
            keys = new KeyBar(kit, canvas);

            gaze = new GazeIndicator(kit, screen.Column);
            UiLayout.Place(gaze.Root, Vector2.zero, new Vector2(0f, 1f), new Vector2(22f, -GazeOffset), gaze.Root.sizeDelta);
            gaze.Root.gameObject.SetActive(false);
        }

        public MenuList List => screen.List;

        public void Show(string title, IReadOnlyList<MenuEntry> entries, int selected)
        {
            screen.Show(title, entries, selected);
        }

        public void SetVisible(bool visible)
        {
            if (canvas.gameObject.activeSelf != visible)
                canvas.gameObject.SetActive(visible);
        }

        public void SetFooter(string text)
        {
            screen.SetFooter(text);
        }

        public void SetHints(bool usingMenu, bool inCategory, bool eyeControl, string? enterLabel)
        {
            (bool, bool, bool, string?) wanted = (usingMenu, inCategory, eyeControl, enterLabel);
            if (shownHints == wanted)
                return;

            shownHints = wanted;
            hints.Clear();
            if (enterLabel != null)
                hints.Add(new KeyHint(enterLabel, 0, "E"));

            if (usingMenu)
            {
                hints.Add(new KeyHint("Select", 0, KeyHint.Up, KeyHint.Down));
                hints.Add(new KeyHint(inCategory ? "Go" : "Open", 0, "Enter"));
                if (inCategory)
                    hints.Add(new KeyHint("Back", 0, "Esc"));
                hints.Add(new KeyHint("Look around", 0, "Tab"));
            }
            else
            {
                hints.Add(new KeyHint("Use the menu", 0, "Tab"));
            }

            hints.Add(new KeyHint("Door number", 0, "0-9"));
            string state = eyeControl ? $"<color=#{Theme.Hex(Theme.Accent)}>on</color>" : "off";
            hints.Add(new KeyHint($"Eye control {state}", 0, "N"));
            hints.Add(new KeyHint("Menu", 0, "M"));
            keys.Set(hints);
        }

        public void SetGaze(GazeMenuInput? input)
        {
            gaze.Root.gameObject.SetActive(input != null);
            if (input != null)
                gaze.Set(input.Direction, input.StepProgress, input.ConfirmProgress);
        }

        public void Tick(float deltaTime, bool active)
        {
            screen.Tick(deltaTime);
            menu.alpha = Mathf.MoveTowards(menu.alpha, active ? 1f : IdleAlpha, FadeSpeed * deltaTime);
        }
    }
}
