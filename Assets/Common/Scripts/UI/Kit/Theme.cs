using UnityEngine;

namespace GameHub.Common
{
    public static class Theme
    {
        public const float Display = 72f;
        public const float Title = 44f;
        public const float Heading = 32f;
        public const float Body = 26f;
        public const float Small = 21f;
        public const float Tiny = 17f;
        public const float Margin = 72f;

        public static readonly Color Backdrop = new(0.02f, 0.03f, 0.035f, 1f);
        public static readonly Color Text = new(0.93f, 0.95f, 0.94f, 1f);
        public static readonly Color TextDim = new(0.6f, 0.66f, 0.66f, 1f);
        public static readonly Color TextFaint = new(0.36f, 0.42f, 0.42f, 1f);
        public static readonly Color Line = new(1f, 1f, 1f, 0.09f);
        public static readonly Color Accent = new(0.33f, 0.93f, 0.58f, 1f);
        public static readonly Color Warning = new(0.98f, 0.86f, 0.35f, 1f);
        public static readonly Color Danger = new(1f, 0.33f, 0.38f, 1f);

        public static Color Fade(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static string Hex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
    }
}
