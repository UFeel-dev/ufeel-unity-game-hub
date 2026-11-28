using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GameHub.Common
{
    public sealed class ControlsPanel
    {
        private const float ScrimOpacity = 0.9f;
        private const int VisibleRows = 10;
        private const int BackAction = 1;
        private const int CancelAction = 2;
        private const string Reserved = "Esc, Enter, arrows, digits and Backspace are reserved";

        private static readonly KeyHint[] Hints =
        {
            new("Select", 0, KeyHint.Up, KeyHint.Down),
            new("Change", 0, "Enter"),
            new("Back", BackAction, "Esc"),
        };

        private static readonly KeyHint[] ListeningHints =
        {
            new("Press a key", 0),
            new("Cancel", CancelAction, "Esc"),
        };

        private readonly Scrim scrim;
        private readonly ListScreen screen;
        private readonly KeyBar keys;
        private readonly List<MenuEntry> entries = new();

        private KeyAction? listening;

        public ControlsPanel(UiKit kit, Transform parent)
        {
            scrim = new Scrim(kit, parent, ScrimOpacity);
            screen = new ListScreen(kit, parent, "Controls", VisibleRows);
            keys = new KeyBar(kit, parent);
            keys.SetVisible(false);
        }

        public bool IsVisible { get; private set; }

        private static int ResetRow => KeyBinds.Count;

        private static int BackRow => KeyBinds.Count + 1;

        public void Show()
        {
            if (IsVisible)
                return;

            IsVisible = true;
            listening = null;
            UiFocus.Capture(this);
            scrim.Show(true, Fader.DefaultSeconds);
            keys.Set(Hints);
            keys.SetVisible(true);
            Rebuild();
            screen.Show("Controls", entries, 0);
            screen.SetFooter(Reserved);
        }

        public void Hide()
        {
            if (!IsVisible)
                return;

            IsVisible = false;
            listening = null;
            KeyBinds.IsCapturing = false;
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

            if (listening.HasValue)
            {
                Listen(listening.Value, mouse);
                return;
            }

            if (keys.Clicked(mouse) == BackAction)
            {
                GameSounds.Play(SoundCue.MenuBack);
                Hide();
                return;
            }

            switch (screen.List.HandleInput(deltaTime, mouse))
            {
                case MenuAction.Back:
                    Hide();
                    break;
                case MenuAction.Confirmed:
                    Confirm(screen.List.Selected);
                    break;
            }
        }

        private static Key PressedKey()
        {
            Keyboard? keyboard = Keyboard.current;
            if (keyboard == null)
                return Key.None;

            foreach (KeyControl control in keyboard.allKeys)
            {
                if (control != null && control.wasPressedThisFrame)
                    return control.keyCode;
            }

            return Key.None;
        }

        private void Confirm(int row)
        {
            if (row == BackRow)
            {
                Hide();
                return;
            }

            if (row == ResetRow)
            {
                KeyBinds.Reset();
                Refresh("Default keys restored");
                return;
            }

            listening = (KeyAction)row;
            KeyBinds.IsCapturing = true;
            keys.Set(ListeningHints);
            Refresh(Reserved);
        }

        private void Listen(KeyAction action, PointerInput mouse)
        {
            if (keys.Clicked(mouse) == CancelAction || MenuKeys.Back())
            {
                GameSounds.Play(SoundCue.MenuBack);
                StopListening(Reserved);
                return;
            }

            Key key = PressedKey();
            if (key == Key.None)
                return;

            if (KeyBinds.IsReserved(key))
            {
                GameSounds.Play(SoundCue.MenuBack);
                screen.SetFooter($"{KeyBinds.KeyLabel(key)} is reserved");
                return;
            }

            KeyAction? swapped = KeyBinds.Set(action, key);
            GameSounds.Play(SoundCue.MenuConfirm);
            StopListening(swapped.HasValue ? $"{KeyBinds.Name(swapped.Value)} moved to {KeyBinds.Label(swapped.Value)}" : Reserved);
        }

        private void StopListening(string footer)
        {
            listening = null;
            KeyBinds.IsCapturing = false;
            keys.Set(Hints);
            Refresh(footer);
        }

        private void Refresh(string footer)
        {
            Rebuild();
            for (int i = 0; i < entries.Count; i++)
                screen.List.SetItem(i, entries[i]);
            screen.SetFooter(footer);
        }

        private void Rebuild()
        {
            entries.Clear();
            for (int i = 0; i < KeyBinds.Count; i++)
            {
                KeyAction action = (KeyAction)i;
                string key = listening == action ? "Press a key" : KeyBinds.Label(action);
                entries.Add(new MenuEntry(KeyBinds.Name(action), key));
            }

            entries.Add(new MenuEntry("Reset to defaults"));
            entries.Add(new MenuEntry("Back"));
        }
    }
}
