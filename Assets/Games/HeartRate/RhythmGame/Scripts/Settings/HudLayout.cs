using UnityEngine;

namespace GameHub.RhythmGame
{
    public static class HudLayout
    {
        public const int ElementCount = 3;

        private const float MinY = 240f;
        private const float MaxY = 1000f;

        private const string Prefix = "GameHub.RhythmGame.Hud.";

        private static readonly float[] Defaults = { 640f, 470f, 330f };
        private static readonly string[] Labels = { "Combo", "Judgement", "Timing bar" };

        public static float Y(HudElement element)
        {
            return Mathf.Clamp(PlayerPrefs.GetFloat(Prefix + element, Defaults[(int)element]), MinY, MaxY);
        }

        public static void SetY(HudElement element, float y)
        {
            PlayerPrefs.SetFloat(Prefix + element, Mathf.Clamp(y, MinY, MaxY));
        }

        public static void Reset()
        {
            for (int i = 0; i < ElementCount; i++)
                PlayerPrefs.DeleteKey(Prefix + (HudElement)i);
        }

        public static string Label(HudElement element)
        {
            return Labels[(int)element];
        }
    }
}
