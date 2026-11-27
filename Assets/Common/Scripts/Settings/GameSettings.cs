using UnityEngine;

namespace GameHub.Common
{
    public static class GameSettings
    {
        private const int MaxVolume = 100;
        private const int VolumeStep = 5;
        private const int DefaultSensitivity = 100;
        private const int MinSensitivity = 25;
        private const int MaxSensitivity = 300;
        private const int SensitivityStep = 5;
        private const int DefaultFlashlight = 100;
        private const int MinFlashlight = 10;
        private const int MaxFlashlight = 100;
        private const int FlashlightStep = 10;

        private const string MasterKey = "GameHub.Settings.MasterVolume";
        private const string MusicKey = "GameHub.Settings.MusicVolume";
        private const string EffectsKey = "GameHub.Settings.EffectsVolume";
        private const string SensitivityKey = "GameHub.Settings.LookSensitivity";
        private const string FullscreenKey = "GameHub.Settings.Fullscreen";
        private const string VSyncKey = "GameHub.Settings.VSync";
        private const string FlashlightKey = "GameHub.Settings.FlashlightIntensity";

        public static int MasterVolume => Read(MasterKey, MaxVolume, 0, MaxVolume);

        public static int MusicVolume => Read(MusicKey, MaxVolume, 0, MaxVolume);

        public static int EffectsVolume => Read(EffectsKey, MaxVolume, 0, MaxVolume);

        public static int LookSensitivity => Read(SensitivityKey, DefaultSensitivity, MinSensitivity, MaxSensitivity);

        public static float LookMultiplier => LookSensitivity / 100f;

        public static int FlashlightIntensity => Read(FlashlightKey, DefaultFlashlight, MinFlashlight, MaxFlashlight);

        public static float FlashlightMultiplier => FlashlightIntensity / 100f;

        public static bool Fullscreen => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) != 0;

        public static bool VSync => PlayerPrefs.GetInt(VSyncKey, 1) != 0;

        public static void AdjustMasterVolume(int steps, bool fine)
        {
            Adjust(MasterKey, MasterVolume, steps * (fine ? 1 : VolumeStep), 0, MaxVolume);
            ApplyAudio();
        }

        public static void AdjustMusicVolume(int steps, bool fine)
        {
            Adjust(MusicKey, MusicVolume, steps * (fine ? 1 : VolumeStep), 0, MaxVolume);
            ApplyAudio();
        }

        public static void AdjustEffectsVolume(int steps, bool fine)
        {
            Adjust(EffectsKey, EffectsVolume, steps * (fine ? 1 : VolumeStep), 0, MaxVolume);
            ApplyAudio();
        }

        public static void AdjustLookSensitivity(int steps, bool fine)
        {
            Adjust(SensitivityKey, LookSensitivity, steps * (fine ? 1 : SensitivityStep), MinSensitivity, MaxSensitivity);
        }

        public static void AdjustFlashlightIntensity(int steps, bool fine)
        {
            Adjust(FlashlightKey, FlashlightIntensity, steps * (fine ? 1 : FlashlightStep), MinFlashlight, MaxFlashlight);
        }

        public static void ToggleFullscreen()
        {
            PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 0 : 1);
            ApplyDisplay();
        }

        public static void ToggleVSync()
        {
            PlayerPrefs.SetInt(VSyncKey, VSync ? 0 : 1);
            ApplyDisplay();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static void Apply()
        {
            ApplyAudio();
            ApplyDisplay();
        }

        private static void ApplyAudio()
        {
            GameAudio.SetVolumes(MasterVolume / (float)MaxVolume, MusicVolume / (float)MaxVolume, EffectsVolume / (float)MaxVolume);
        }

        private static void ApplyDisplay()
        {
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            FullScreenMode mode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            if (Screen.fullScreenMode != mode)
                Screen.fullScreenMode = mode;
        }

        private static int Read(string key, int fallback, int min, int max)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(key, fallback), min, max);
        }

        private static void Adjust(string key, int current, int delta, int min, int max)
        {
            PlayerPrefs.SetInt(key, Mathf.Clamp(current + delta, min, max));
        }
    }
}
