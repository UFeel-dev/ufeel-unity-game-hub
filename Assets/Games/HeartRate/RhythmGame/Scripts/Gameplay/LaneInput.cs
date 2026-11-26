using UnityEngine.InputSystem;

namespace GameHub.RhythmGame
{
    public sealed class LaneInput : ILaneInput
    {
        private readonly Key[] keys = new Key[LaneMask.LaneCount * KeyBindings.SlotCount];
        private readonly bool[] held = new bool[LaneMask.LaneCount];
        private readonly bool[] pressed = new bool[LaneMask.LaneCount];
        private readonly bool[] released = new bool[LaneMask.LaneCount];

        public LaneInput()
        {
            Reload();
        }

        public bool IsHeld(int lane)
        {
            return held[lane];
        }

        public bool WasPressed(int lane)
        {
            return pressed[lane];
        }

        public bool WasReleased(int lane)
        {
            return released[lane];
        }

        public void Reload()
        {
            for (int slot = 0; slot < KeyBindings.SlotCount; slot++)
            {
                for (int lane = 0; lane < LaneMask.LaneCount; lane++)
                    keys[(slot * LaneMask.LaneCount) + lane] = KeyBindings.Get(lane, slot);
            }
        }

        public void Tick()
        {
            Keyboard? keyboard = Keyboard.current;
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                bool wasHeld = held[lane];
                bool down = false;
                bool isHeld = false;
                for (int slot = 0; slot < KeyBindings.SlotCount && keyboard != null; slot++)
                {
                    Key key = keys[(slot * LaneMask.LaneCount) + lane];
                    if (key == Key.None)
                        continue;
                    down |= keyboard[key].wasPressedThisFrame;
                    isHeld |= keyboard[key].isPressed;
                }

                pressed[lane] = down && !wasHeld;
                released[lane] = (wasHeld && !isHeld) || (pressed[lane] && !isHeld);
                held[lane] = isHeld;
            }
        }

        public bool AnyPressed()
        {
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                if (pressed[lane])
                    return true;
            }
            return false;
        }
    }
}
