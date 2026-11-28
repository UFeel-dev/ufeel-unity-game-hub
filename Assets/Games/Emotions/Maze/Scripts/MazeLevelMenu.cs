using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;

namespace GameHub.Maze
{
    public sealed class MazeLevelMenu
    {
        private const float Width = 760f;
        private const float RowHeight = 62f;
        private const float PanelPadding = 32f;
        private const float PanelExtra = 140f;
        private const float PanelOpacity = 0.78f;
        private const float PanelRadius = 20f;
        private const float NoticeOffset = 96f;
        private const int PlayAction = 1;

        private static readonly KeyHint[] Hints =
        {
            new("Select", 0, KeyHint.Up, KeyHint.Down),
            new("Play", PlayAction, "Enter"),
        };

        private readonly int levelCount;
        private readonly MazeIcons icons;
        private readonly RectTransform backdropCanvas;
        private readonly RectTransform canvas;
        private readonly MazeMenuBackdrop backdrop;
        private readonly ListScreen screen;
        private readonly KeyBar keys;
        private readonly Toast notice;
        private readonly PointerInput mouse = new();
        private readonly List<MenuEntry> entries = new();
        private bool resetArmed;

        public MazeLevelMenu(UiKit kit, Transform owner, MazeCatalog catalog, MazeIcons icons)
        {
            levelCount = catalog.Levels.Count;
            this.icons = icons;

            backdropCanvas = UiCanvas.Create("MazeLevelsBackdrop", owner, UiCanvas.FullScreenGameOrder, false);
            backdrop = new MazeMenuBackdrop(kit, backdropCanvas, catalog.Emotions);
            canvas = UiCanvas.Create("MazeLevels", owner, UiCanvas.FullScreenGameOrder + 1);

            int rows = levelCount + 3;
            Vector2 center = new(0.5f, 0.5f);
            float panelHeight = (rows * RowHeight) + PanelExtra + (2f * PanelPadding);
            UiLayout.Place(
                kit.NewRounded("Panel", canvas, Theme.Fade(Theme.Backdrop, PanelOpacity), PanelRadius).rectTransform,
                center,
                Vector2.zero,
                new Vector2(Width + (2f * PanelPadding), panelHeight));

            screen = new ListScreen(kit, canvas, "MazeLevelList", rows, center, Vector2.zero, Width, RowHeight);
            screen.List.AllowBack = false;
            BuildEntries();
            screen.Show("Maze", entries, Mathf.Min(MazeSave.UnlockedLevels, levelCount) - 1);
            screen.SetFooter("Clear a level to unlock the next one.");

            keys = new KeyBar(kit, canvas);
            keys.Set(Hints);
            notice = new Toast(kit, canvas, new Vector2(0.5f, 1f), NoticeOffset, Theme.Body);
            PointerInput.ShowCursor(true);
        }

        public int UnlimitedRow => levelCount;

        public int HubRow => levelCount + 2;

        private int ResetRow => levelCount + 1;

        public int? Tick(float deltaTime)
        {
            mouse.Tick();
            backdrop.Tick(deltaTime);
            screen.Tick(deltaTime);
            notice.Tick(deltaTime);

            if (UiFocus.IsFree && Cursor.lockState != CursorLockMode.None)
                PointerInput.ShowCursor(true);

            if (!UiFocus.IsFree)
                return null;

            bool clicked = keys.Clicked(mouse) == PlayAction;
            if (clicked)
                GameSounds.Play(SoundCue.MenuConfirm);
            bool confirmed = clicked || screen.List.HandleInput(deltaTime, mouse) == MenuAction.Confirmed;
            int row = screen.List.Selected;
            if (row != ResetRow)
                resetArmed = false;
            if (!confirmed)
                return null;

            if (row == ResetRow)
            {
                ConfirmReset();
                return null;
            }

            if (row < levelCount && !MazeSave.IsUnlocked(row))
            {
                notice.Show("Clear the previous level to unlock this one");
                return null;
            }

            if (row == UnlimitedRow && !MazeSave.IsUnlimitedUnlocked(levelCount))
            {
                notice.Show("Clear every level to unlock Unlimited");
                return null;
            }

            return row;
        }

        public void Close()
        {
            backdrop.Release();
            if (backdropCanvas != null)
                Object.Destroy(backdropCanvas.gameObject);
            if (canvas != null)
                Object.Destroy(canvas.gameObject);
        }

        private void ConfirmReset()
        {
            if (!resetArmed)
            {
                resetArmed = true;
                notice.Show("Press again to erase all progress");
                return;
            }

            resetArmed = false;
            MazeSave.Reset();
            BuildEntries();
            screen.Show("Maze", entries, ResetRow);
            notice.Show("Progress erased");
        }

        private void BuildEntries()
        {
            entries.Clear();
            for (int i = 0; i < levelCount; i++)
            {
                if (!MazeSave.IsUnlocked(i))
                    entries.Add(new MenuEntry($"Level {i + 1}", icon: icons.Padlock, isEnabled: false));
                else
                    entries.Add(new MenuEntry($"Level {i + 1}", icon: MazeSave.IsCleared(i) ? icons.Check : null));
            }

            if (MazeSave.IsUnlimitedUnlocked(levelCount))
            {
                string record = MazeSave.UnlimitedBest > 0 ? $"Best {MazeSave.UnlimitedBest}" : string.Empty;
                entries.Add(new MenuEntry("Unlimited", record));
            }
            else
            {
                entries.Add(new MenuEntry("Unlimited", icon: icons.Padlock, isEnabled: false));
            }

            entries.Add(new MenuEntry("Reset progress"));
            entries.Add(new MenuEntry("Back to the hub"));
        }
    }
}
