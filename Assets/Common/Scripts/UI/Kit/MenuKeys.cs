using UnityEngine;

namespace GameHub.Common
{
    public static class MenuKeys
    {
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
            return LayoutInput.GetKeyDown(KeyCode.Escape);
        }

        public static bool Up(bool held)
        {
            return held ? LayoutInput.GetKey(KeyCode.UpArrow) || LayoutInput.GetKey(KeyCode.W) : LayoutInput.GetKeyDown(KeyCode.UpArrow) || LayoutInput.GetKeyDown(KeyCode.W);
        }

        public static bool Down(bool held)
        {
            return held ? LayoutInput.GetKey(KeyCode.DownArrow) || LayoutInput.GetKey(KeyCode.S) : LayoutInput.GetKeyDown(KeyCode.DownArrow) || LayoutInput.GetKeyDown(KeyCode.S);
        }
    }
}
