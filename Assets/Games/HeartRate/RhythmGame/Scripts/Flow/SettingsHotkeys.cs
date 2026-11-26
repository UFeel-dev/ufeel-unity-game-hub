using System;
using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public static class SettingsHotkeys
    {
        public static bool Handle(OverlayView overlay)
        {
            if (LayoutInput.GetKeyDown(KeyCode.F3))
                return Scroll(overlay, 1);
            if (LayoutInput.GetKeyDown(KeyCode.F4))
                return Scroll(overlay, -1);
            if (LayoutInput.GetKeyDown(KeyCode.F5))
                return Offset(overlay, -1);
            return LayoutInput.GetKeyDown(KeyCode.F6) && Offset(overlay, 1);
        }

        private static bool Scroll(OverlayView overlay, int steps)
        {
            int value = RhythmSettings.AdjustScroll(steps, MenuKeys.Fine());
            overlay.Toast(FormattableString.Invariant($"Scroll speed   {value} ms on screen"));
            return true;
        }

        private static bool Offset(OverlayView overlay, int steps)
        {
            int value = RhythmSettings.AdjustOffset(steps, MenuKeys.Fine());
            overlay.Toast(FormattableString.Invariant($"Audio offset   {value:+0;-0;0} ms"));
            return true;
        }
    }
}
