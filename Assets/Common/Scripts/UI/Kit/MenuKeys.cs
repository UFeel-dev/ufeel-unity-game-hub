using UnityEngine;

namespace GameHub.Common
{
    public static class MenuKeys
    {
        private static int backFrame = -1;

        public static bool Fine()
        {
            return LayoutInput.GetKey(KeyCode.LeftShift) || LayoutInput.GetKey(KeyCode.RightShift);
        }

        public static bool Confirm()
        {
            return LayoutInput.GetKeyDown(KeyCode.Return) || LayoutInput.GetKeyDown(KeyCode.KeypadEnter);
        }

        public static bool Back()
        {
            if (backFrame == Time.frameCount || !LayoutInput.GetKeyDown(KeyCode.Escape))
                return false;

            backFrame = Time.frameCount;
            return true;
        }

        public static bool Up(bool held)
        {
            return held ? LayoutInput.GetKey(KeyCode.UpArrow) || KeyBinds.Held(KeyAction.MoveForward) : LayoutInput.GetKeyDown(KeyCode.UpArrow) || KeyBinds.Down(KeyAction.MoveForward);
        }

        public static bool Down(bool held)
        {
            return held ? LayoutInput.GetKey(KeyCode.DownArrow) || KeyBinds.Held(KeyAction.MoveBack) : LayoutInput.GetKeyDown(KeyCode.DownArrow) || KeyBinds.Down(KeyAction.MoveBack);
        }

        public static bool Left()
        {
            return LayoutInput.GetKey(KeyCode.LeftArrow) || KeyBinds.Held(KeyAction.MoveLeft);
        }

        public static bool Right()
        {
            return LayoutInput.GetKey(KeyCode.RightArrow) || KeyBinds.Held(KeyAction.MoveRight);
        }
    }
}
