using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class SettingsPanel
    {
        private const float ScrimOpacity = 0.86f;
        private const int BackAction = 1;

        private static readonly KeyHint[] Hints =
        {
            new("Select", 0, KeyHint.Up, KeyHint.Down),
            new("Adjust", 0, KeyHint.Left, KeyHint.Right),
            new("Fine", 0, "Shift"),
            new("Back", BackAction, "Esc"),
        };

        private readonly Scrim scrim;
        private readonly ListScreen screen;
        private readonly KeyBar keys;
        private readonly List<MenuEntry> entries = new();

        public SettingsPanel(UiKit kit, Transform parent)
        {
            scrim = new Scrim(kit, parent, ScrimOpacity);
            screen = new ListScreen(kit, parent, "Settings", RowCount);
            keys = new KeyBar(kit, parent);
            keys.Set(Hints);
            keys.SetVisible(false);
        }

        private enum Row
        {
            MasterVolume,
            MusicVolume,
            EffectsVolume,
            LookSensitivity,
            Fullscreen,
            VSync,
            Back,
        }

        private static int RowCount => Enum.GetValues(typeof(Row)).Length;

        public bool IsVisible { get; private set; }

        public void Show()
        {
            if (IsVisible)
                return;

            IsVisible = true;
            UiFocus.Capture(this);
            scrim.Show(true, Fader.DefaultSeconds);
            keys.SetVisible(true);
            Rebuild();
            screen.Show("Settings", entries, 0);
        }

        public void Hide()
        {
            if (!IsVisible)
                return;

            IsVisible = false;
            GameSettings.Save();
            UiFocus.Release(this);
            scrim.Show(false, Fader.DefaultSeconds);
            screen.Hide();
            keys.SetVisible(false);
        }

        public void Tick(float deltaTime, PointerInput mouse)
        {
            scrim.Tick(deltaTime);
            screen.Tick(deltaTime);
            if (!IsVisible || !UiFocus.Has(this))
                return;

            if (keys.Clicked(mouse) == BackAction)
            {
                Hide();
                return;
            }

            switch (screen.List.HandleInput(deltaTime, mouse))
            {
                case MenuAction.Back:
                    Hide();
                    break;
                case MenuAction.Decrease:
                    Adjust(-1);
                    break;
                case MenuAction.Increase:
                    Adjust(1);
                    break;
                case MenuAction.Confirmed when (Row)screen.List.Selected == Row.Back:
                    Hide();
                    break;
                case MenuAction.Confirmed:
                    Adjust(1);
                    break;
            }
        }

        private void Adjust(int direction)
        {
            bool fine = MenuKeys.Fine();
            switch ((Row)screen.List.Selected)
            {
                case Row.MasterVolume:
                    GameSettings.AdjustMasterVolume(direction, fine);
                    break;
                case Row.MusicVolume:
                    GameSettings.AdjustMusicVolume(direction, fine);
                    break;
                case Row.EffectsVolume:
                    GameSettings.AdjustEffectsVolume(direction, fine);
                    break;
                case Row.LookSensitivity:
                    GameSettings.AdjustLookSensitivity(direction, fine);
                    break;
                case Row.Fullscreen:
                    GameSettings.ToggleFullscreen();
                    break;
                case Row.VSync:
                    GameSettings.ToggleVSync();
                    break;
                default:
                    return;
            }

            Rebuild();
            for (int i = 0; i < entries.Count; i++)
                screen.List.SetItem(i, entries[i]);
        }

        private void Rebuild()
        {
            entries.Clear();
            entries.Add(new MenuEntry("Master volume", Percent(GameSettings.MasterVolume)));
            entries.Add(new MenuEntry("Music volume", Percent(GameSettings.MusicVolume)));
            entries.Add(new MenuEntry("Effects volume", Percent(GameSettings.EffectsVolume)));
            entries.Add(new MenuEntry("Mouse sensitivity", Percent(GameSettings.LookSensitivity)));
            entries.Add(new MenuEntry("Fullscreen", OnOff(GameSettings.Fullscreen)));
            entries.Add(new MenuEntry("VSync", OnOff(GameSettings.VSync)));
            entries.Add(new MenuEntry("Back"));
        }

        private static string Percent(int value)
        {
            return FormattableString.Invariant($"{value}%");
        }

        private static string OnOff(bool value)
        {
            return value ? "On" : "Off";
        }
    }
}
