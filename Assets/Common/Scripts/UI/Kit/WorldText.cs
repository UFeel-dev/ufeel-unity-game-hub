using TMPro;
using UnityEngine;

namespace GameHub.Common
{
    public static class WorldText
    {
        public static void Style(TMP_Text text, bool isBold, Color color)
        {
            UiKit.Shared.ApplyFont(text, isBold);
            text.color = color;
        }
    }
}
