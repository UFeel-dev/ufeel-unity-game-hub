using System;
using System.Collections.Generic;
using GameHub.Common;

namespace GameHub.RhythmGame
{
    public sealed class OptionsController
    {
        public const int RowCount = 12;

        private const int KeysRow = 0;
        private const int ScrollRow = 1;
        private const int OffsetRow = 2;
        private const int CalibrateRow = 3;
        private const int StyleRow = 4;
        private const int ShowKeysRow = 5;
        private const int LayoutRow = 6;
        private const int FlashRow = 7;
        private const int RestSpeedRow = 8;
        private const int PeakSpeedRow = 9;
        private const int RestingRow = 10;
        private const int BackRow = 11;
        private const int BackAction = 1;

        private static readonly KeyHint[] Hints =
        {
            new("Select", 0, KeyHint.Up, KeyHint.Down),
            new("Adjust", 0, KeyHint.Left, KeyHint.Right),
            new("Fine", 0, "Shift"),
            new("Open", 0, "Enter"),
            new("Back", BackAction, "Esc"),
        };

        private readonly SessionContext context;
        private readonly List<MenuEntry> entries = new();
        private int selection;

        public OptionsController(SessionContext context)
        {
            this.context = context;
        }

        private ListScreen Screen => context.Stage.Options;

        public void Enter()
        {
            Rebuild();
            Screen.Show("Options", entries, selection);
            context.Stage.ShowScrim(true);
            context.Stage.KeyBar.Set(Hints);
            PointerInput.ShowCursor(true);
        }

        public void Leave()
        {
            context.Resting.Save();
            selection = Screen.List.Selected;
            Screen.Hide();
            context.Stage.ShowScrim(false);
        }

        public Navigation Tick(float deltaTime)
        {
            if (context.Stage.KeyBar.Clicked(context.Mouse) == BackAction)
                return Back();
            if (SettingsHotkeys.Handle(context.Stage.Overlay))
                Refresh();

            switch (Screen.List.HandleInput(deltaTime, context.Mouse))
            {
                case MenuAction.Moved:
                    context.Sounds.Move();
                    return Navigation.None;
                case MenuAction.Back:
                    return Back();
                case MenuAction.Decrease:
                    Adjust(-1);
                    return Navigation.None;
                case MenuAction.Increase:
                    Adjust(1);
                    return Navigation.None;
                case MenuAction.Confirmed:
                    return Open(Screen.List.Selected);
                default:
                    return Navigation.None;
            }
        }

        private Navigation Back()
        {
            context.Sounds.Back();
            return Navigation.Back;
        }

        private Navigation Open(int row)
        {
            switch (row)
            {
                case KeysRow:
                    context.Sounds.Confirm();
                    return Navigation.Keys;
                case CalibrateRow:
                    context.Sounds.Confirm();
                    return Navigation.Calibration;
                case LayoutRow:
                    context.Sounds.Confirm();
                    return Navigation.HudLayout;
                case BackRow:
                    return Back();
                default:
                    Adjust(1);
                    return Navigation.None;
            }
        }

        private void Adjust(int direction)
        {
            int row = Screen.List.Selected;
            bool fine = MenuKeys.Fine();
            switch (row)
            {
                case ScrollRow:
                    RhythmSettings.AdjustScroll(direction, fine);
                    break;
                case OffsetRow:
                    RhythmSettings.AdjustOffset(direction, fine);
                    break;
                case StyleRow:
                    RhythmSettings.ToggleNoteStyle();
                    break;
                case ShowKeysRow:
                    RhythmSettings.ToggleShowKeys();
                    break;
                case FlashRow:
                    RhythmSettings.AdjustFlash(direction, fine);
                    break;
                case RestSpeedRow:
                    TempoSettings.AdjustRest(direction, fine);
                    break;
                case PeakSpeedRow:
                    TempoSettings.AdjustPeak(direction, fine);
                    break;
                case RestingRow:
                    context.Resting.Adjust(direction, fine);
                    break;
                default:
                    return;
            }

            context.Sounds.Move();
            Refresh();
        }

        private void Refresh()
        {
            Rebuild();
            for (int i = 0; i < entries.Count; i++)
                Screen.List.SetItem(i, entries[i]);
        }

        private void Rebuild()
        {
            entries.Clear();
            entries.Add(new MenuEntry("Keys", string.Join("  ", KeyBindings.PrimaryLabels())));
            entries.Add(new MenuEntry("Scroll speed", FormattableString.Invariant($"{RhythmSettings.ScrollTimeMs} ms")));
            entries.Add(new MenuEntry("Audio offset", FormattableString.Invariant($"{RhythmSettings.AudioOffsetMs:+0;-0;0} ms")));
            entries.Add(new MenuEntry("Calibrate offset"));
            entries.Add(new MenuEntry("Note style", RhythmSettings.NoteStyle == NoteStyle.Arrows ? "Arrows" : "Circles"));
            entries.Add(new MenuEntry("Show keys", RhythmSettings.ShowKeys ? "On" : "Off"));
            entries.Add(new MenuEntry("HUD layout"));
            entries.Add(new MenuEntry("Heart flash", FormattableString.Invariant($"{RhythmSettings.FlashPercent}%")));
            entries.Add(new MenuEntry("Speed at rest", FormattableString.Invariant($"{TempoSettings.RestPercent}%")));
            entries.Add(new MenuEntry("Speed at peak", FormattableString.Invariant($"{TempoSettings.PeakPercent}%")));
            entries.Add(new MenuEntry("Resting heart rate", FormattableString.Invariant($"{context.Resting.Bpm:0} BPM")));
            entries.Add(new MenuEntry("Back"));
        }
    }
}
