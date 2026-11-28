using System;
using System.Collections.Generic;
using UFeel;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class SettingsPanel
    {
        private const float ScrimOpacity = 0.86f;
        private const int BackAction = 1;

        private static readonly KeyHint[] Hints =
        {
            new("Select", 0, KeyHint.Up, KeyHint.Down),
            new("Adjust", 0, KeyHint.Left, KeyHint.Right),
            new("Fine", 0, "Shift"),
            new("Back", BackAction, "Esc"),
        };

        private readonly Scrim scrim;
        private readonly ListScreen screen;
        private readonly KeyBar keys;
        private readonly List<MenuEntry> entries = new();
        private readonly ControlsPanel controls;

        private bool eyeTrackingCalibrated;
        private bool emotionCalibrated;
        private bool resetArmed;

        public SettingsPanel(UiKit kit, Transform parent)
        {
            scrim = new Scrim(kit, parent, ScrimOpacity);
            screen = new ListScreen(kit, parent, "Settings", RowCount);
            keys = new KeyBar(kit, parent);
            keys.Set(Hints);
            keys.SetVisible(false);
            controls = new ControlsPanel(kit, parent);
        }

        private enum Row
        {
            MasterVolume,
            MusicVolume,
            EffectsVolume,
            LookSensitivity,
            FlashlightIntensity,
            Fullscreen,
            VSync,
            Controls,
            EyeTrackingCalibration,
            EmotionCalibration,
            ResetEmotionCalibration,
            Back,
        }

        private static int RowCount => Enum.GetValues(typeof(Row)).Length;

        public bool IsVisible { get; private set; }

        public void Show()
        {
            if (IsVisible)
                return;

            IsVisible = true;
            resetArmed = false;
            UiFocus.Capture(this);
            scrim.Show(true, Fader.DefaultSeconds);
            keys.SetVisible(true);
            Rebuild();
            screen.Show("Settings", entries, 0);
        }

        public void Hide()
        {
            if (!IsVisible)
                return;

            controls.Hide();
            IsVisible = false;
            GameSettings.Save();
            UiFocus.Release(this);
            scrim.Show(false, Fader.DefaultSeconds);
            screen.Hide();
            keys.SetVisible(false);
        }

        public void Tick(float deltaTime, PointerInput mouse)
        {
            scrim.Tick(deltaTime);
            screen.Tick(deltaTime);
            controls.Tick(deltaTime, mouse);
            bool onTop = IsVisible && UiFocus.IsTop(this);
            screen.SetVisible(onTop);
            keys.SetVisible(onTop);
            if (!onTop || !UiFocus.Has(this))
                return;

            if (keys.Clicked(mouse) == BackAction)
            {
                GameSounds.Play(SoundCue.MenuBack);
                Hide();
                return;
            }

            switch (screen.List.HandleInput(deltaTime, mouse))
            {
                case MenuAction.Back:
                    Hide();
                    break;
                case MenuAction.Decrease:
                    Adjust(-1);
                    break;
                case MenuAction.Increase:
                    Adjust(1);
                    break;
                case MenuAction.Confirmed:
                    Confirm((Row)screen.List.Selected);
                    break;
            }

            if (!IsVisible)
                return;

            bool disarm = resetArmed && (Row)screen.List.Selected != Row.ResetEmotionCalibration;
            if (disarm)
                resetArmed = false;
            if (disarm || eyeTrackingCalibrated != UFeelAPI.EyeTracking.IsCalibrated || emotionCalibrated != UFeelAPI.Emotion.IsCalibrated)
                Refresh();
        }

        private void Confirm(Row row)
        {
            switch (row)
            {
                case Row.Back:
                    Hide();
                    break;
                case Row.Controls:
                    controls.Show();
                    break;
                case Row.EyeTrackingCalibration:
                    UFeelScreens.BeginEyeTrackingCalibration();
                    break;
                case Row.EmotionCalibration:
                    UFeelScreens.BeginEmotionCalibration();
                    break;
                case Row.ResetEmotionCalibration:
                    ConfirmEmotionReset();
                    break;
                default:
                    Adjust(1);
                    break;
            }
        }

        private void ConfirmEmotionReset()
        {
            if (!emotionCalibrated)
                return;

            if (resetArmed)
                UFeelScreens.ResetEmotionCalibration();

            resetArmed = !resetArmed;
            Refresh();
        }

        private void Adjust(int direction)
        {
            bool fine = MenuKeys.Fine();
            switch ((Row)screen.List.Selected)
            {
                case Row.MasterVolume:
                    GameSettings.AdjustMasterVolume(direction, fine);
                    break;
                case Row.MusicVolume:
                    GameSettings.AdjustMusicVolume(direction, fine);
                    break;
                case Row.EffectsVolume:
                    GameSettings.AdjustEffectsVolume(direction, fine);
                    break;
                case Row.LookSensitivity:
                    GameSettings.AdjustLookSensitivity(direction, fine);
                    break;
                case Row.FlashlightIntensity:
                    GameSettings.AdjustFlashlightIntensity(direction, fine);
                    break;
                case Row.Fullscreen:
                    GameSettings.ToggleFullscreen();
                    break;
                case Row.VSync:
                    GameSettings.ToggleVSync();
                    break;
                default:
                    return;
            }

            Refresh();
        }

        private void Refresh()
        {
            Rebuild();
            for (int i = 0; i < entries.Count; i++)
                screen.List.SetItem(i, entries[i]);
        }

        private void Rebuild()
        {
            eyeTrackingCalibrated = UFeelAPI.EyeTracking.IsCalibrated;
            emotionCalibrated = UFeelAPI.Emotion.IsCalibrated;

            entries.Clear();
            entries.Add(new MenuEntry("Master volume", Percent(GameSettings.MasterVolume)));
            entries.Add(new MenuEntry("Music volume", Percent(GameSettings.MusicVolume)));
            entries.Add(new MenuEntry("Effects volume", Percent(GameSettings.EffectsVolume)));
            entries.Add(new MenuEntry("Mouse sensitivity", Percent(GameSettings.LookSensitivity)));
            entries.Add(new MenuEntry("Flashlight intensity", Percent(GameSettings.FlashlightIntensity)));
            entries.Add(new MenuEntry("Fullscreen", OnOff(GameSettings.Fullscreen)));
            entries.Add(new MenuEntry("VSync", OnOff(GameSettings.VSync)));
            entries.Add(new MenuEntry("Controls"));
            entries.Add(new MenuEntry("Eye-tracking calibration", eyeTrackingCalibrated ? "Calibrated" : "Not calibrated"));
            entries.Add(new MenuEntry("Emotion calibration", emotionCalibrated ? "Personal" : "Default"));
            entries.Add(new MenuEntry("Reset emotion calibration", resetArmed ? "Press again" : string.Empty, isEnabled: emotionCalibrated));
            entries.Add(new MenuEntry("Back"));
        }

        private static string Percent(int value)
        {
            return FormattableString.Invariant($"{value}%");
        }

        private static string OnOff(bool value)
        {
            return value ? "On" : "Off";
        }
    }
}
