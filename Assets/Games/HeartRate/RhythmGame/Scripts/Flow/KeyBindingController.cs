using GameHub.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GameHub.RhythmGame
{
    public sealed class KeyBindingController
    {
        private const int BackAction = 1;
        private const int ClearAction = 2;
        private const int ResetAction = 3;
        private const int CancelAction = 4;

        private static readonly KeyHint[] Hints =
        {
            new("Select", 0, KeyHint.Left, KeyHint.Right, KeyHint.Up, KeyHint.Down),
            new("Change", 0, "Enter"),
            new("Clear", ClearAction, "Backspace"),
            new("Defaults", ResetAction, "R"),
            new("Back", BackAction, "Esc"),
        };

        private static readonly KeyHint[] ListeningHints =
        {
            new("Press a key", 0),
            new("Cancel", CancelAction, "Esc"),
        };

        private readonly SessionContext context;

        private int selected;
        private bool listening;
        private bool skipFrame;

        public KeyBindingController(SessionContext context)
        {
            this.context = context;
        }

        private KeyBindingView View => context.Stage.Keys;

        public void Enter()
        {
            listening = false;
            View.SetVisible(true);
            context.Stage.ShowScrim(true);
            RefreshLabels();
            SetListening(false);
            PointerInput.ShowCursor(true);
        }

        public void Leave()
        {
            View.SetVisible(false);
            context.Stage.ShowScrim(false);
        }

        public Navigation Tick()
        {
            return listening ? Listen() : Browse();
        }

        private static int Lane(int index)
        {
            return index % LaneMask.LaneCount;
        }

        private static int Slot(int index)
        {
            return index / LaneMask.LaneCount;
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

        private Navigation Browse()
        {
            switch (context.Stage.KeyBar.Clicked(context.Mouse))
            {
                case BackAction:
                    context.Sounds.Back();
                    return Navigation.Back;
                case ClearAction:
                    Clear();
                    return Navigation.None;
                case ResetAction:
                    ResetDefaults();
                    return Navigation.None;
            }

            for (int i = 0; i < LaneMask.LaneCount * KeyBindings.SlotCount; i++)
            {
                if (context.Mouse.Pressed && context.Mouse.IsOver(View.Cap(i)))
                {
                    selected = i;
                    SetListening(true);
                    return Navigation.None;
                }
            }

            if (MenuKeys.Back())
            {
                context.Sounds.Back();
                return Navigation.Back;
            }

            if (MenuKeys.Confirm())
                SetListening(true);
            else if (LayoutInput.GetKeyDown(KeyCode.Backspace) || LayoutInput.GetKeyDown(KeyCode.Delete))
                Clear();
            else if (LayoutInput.GetKeyDown(KeyCode.R))
                ResetDefaults();
            else
                Move();
            return Navigation.None;
        }

        private Navigation Listen()
        {
            if (skipFrame)
            {
                skipFrame = false;
                return Navigation.None;
            }

            if (context.Stage.KeyBar.Clicked(context.Mouse) == CancelAction)
            {
                SetListening(false);
                return Navigation.None;
            }

            Key key = PressedKey();
            if (key == Key.None)
                return Navigation.None;

            if (key != Key.Escape)
            {
                KeyBindings.Set(Lane(selected), Slot(selected), key);
                context.Sounds.Confirm();
                RefreshLabels();
            }
            SetListening(false);
            return Navigation.None;
        }

        private void Move()
        {
            int lane = Lane(selected);
            int slot = Slot(selected);
            if (LayoutInput.GetKeyDown(KeyCode.LeftArrow))
                lane = (lane + LaneMask.LaneCount - 1) % LaneMask.LaneCount;
            else if (LayoutInput.GetKeyDown(KeyCode.RightArrow))
                lane = (lane + 1) % LaneMask.LaneCount;
            else if (LayoutInput.GetKeyDown(KeyCode.UpArrow) || LayoutInput.GetKeyDown(KeyCode.DownArrow))
                slot = (slot + 1) % KeyBindings.SlotCount;
            else
                return;

            selected = KeyBindingView.Index(lane, slot);
            context.Sounds.Move();
            View.SetSelection(selected, false);
        }

        private void Clear()
        {
            KeyBindings.Set(Lane(selected), Slot(selected), Key.None);
            context.Sounds.Back();
            RefreshLabels();
        }

        private void ResetDefaults()
        {
            KeyBindings.Reset();
            context.Sounds.Confirm();
            RefreshLabels();
        }

        private void SetListening(bool value)
        {
            listening = value;
            skipFrame = value;
            View.SetSelection(selected, value);
            context.Stage.KeyBar.Set(value ? ListeningHints : Hints);
            if (value)
                context.Sounds.Confirm();
        }

        private void RefreshLabels()
        {
            for (int i = 0; i < LaneMask.LaneCount * KeyBindings.SlotCount; i++)
            {
                string label = KeyBindings.Label(KeyBindings.Get(Lane(i), Slot(i)));
                View.SetLabel(i, string.IsNullOrEmpty(label) ? "–" : label);
            }
        }
    }
}
