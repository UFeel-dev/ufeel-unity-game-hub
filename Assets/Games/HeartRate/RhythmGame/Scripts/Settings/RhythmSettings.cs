using UnityEngine;

namespace GameHub.RhythmGame
{
    public static class RhythmSettings
    {
        private const int DefaultScrollTimeMs = 900;
        private const int MinScrollTimeMs = 400;
        private const int MaxScrollTimeMs = 2000;
        private const int ScrollStepMs = 50;
        private const int MaxAudioOffsetMs = 300;
        private const int OffsetStepMs = 5;
        private const int DefaultFlashPercent = 100;
        private const int FlashStepPercent = 10;

        private const string ScrollKey = "GameHub.RhythmGame.ScrollTimeMs";
        private const string OffsetKey = "GameHub.RhythmGame.AudioOffsetMs";
        private const string FlashKey = "GameHub.RhythmGame.FlashPercent";
        private const string StyleKey = "GameHub.RhythmGame.NoteStyle";
        private const string ShowKeysKey = "GameHub.RhythmGame.ShowKeys";

        public static int ScrollTimeMs => Mathf.Clamp(PlayerPrefs.GetInt(ScrollKey, DefaultScrollTimeMs), MinScrollTimeMs, MaxScrollTimeMs);

        public static int AudioOffsetMs => Mathf.Clamp(PlayerPrefs.GetInt(OffsetKey, 0), -MaxAudioOffsetMs, MaxAudioOffsetMs);

        public static int FlashPercent => Mathf.Clamp(PlayerPrefs.GetInt(FlashKey, DefaultFlashPercent), 0, 100);

        public static NoteStyle NoteStyle => PlayerPrefs.GetInt(StyleKey, (int)NoteStyle.Arrows) == (int)NoteStyle.Circles ? NoteStyle.Circles : NoteStyle.Arrows;

        public static bool ShowKeys => PlayerPrefs.GetInt(ShowKeysKey, 1) != 0;

        public static int AdjustScroll(int steps, bool fine)
        {
            int value = Mathf.Clamp(ScrollTimeMs + (steps * (fine ? 1 : ScrollStepMs)), MinScrollTimeMs, MaxScrollTimeMs);
            PlayerPrefs.SetInt(ScrollKey, value);
            return value;
        }

        public static int AdjustOffset(int steps, bool fine)
        {
            int value = Mathf.Clamp(AudioOffsetMs + (steps * (fine ? 1 : OffsetStepMs)), -MaxAudioOffsetMs, MaxAudioOffsetMs);
            PlayerPrefs.SetInt(OffsetKey, value);
            return value;
        }

        public static void SetAudioOffset(int milliseconds)
        {
            PlayerPrefs.SetInt(OffsetKey, Mathf.Clamp(milliseconds, -MaxAudioOffsetMs, MaxAudioOffsetMs));
            PlayerPrefs.Save();
        }

        public static int AdjustFlash(int steps, bool fine)
        {
            int value = Mathf.Clamp(FlashPercent + (steps * (fine ? 1 : FlashStepPercent)), 0, 100);
            PlayerPrefs.SetInt(FlashKey, value);
            return value;
        }

        public static void ToggleNoteStyle()
        {
            PlayerPrefs.SetInt(StyleKey, (int)(NoteStyle == NoteStyle.Arrows ? NoteStyle.Circles : NoteStyle.Arrows));
        }

        public static void ToggleShowKeys()
        {
            PlayerPrefs.SetInt(ShowKeysKey, ShowKeys ? 0 : 1);
        }
    }
}
