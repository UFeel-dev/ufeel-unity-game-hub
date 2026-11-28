using System;
using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class SongSelectController
    {
        private const double IdleBudgetMs = 3.0;
        private const double PreparingBudgetMs = 10.0;
        private const int PlayAction = 1;
        private const int OptionsAction = 2;
        private const int NextSongAction = 3;

        private static readonly KeyHint[] Hints =
        {
            new("Song", NextSongAction, "Tab"),
            new("Base / Scaling", 0, KeyHint.Up, KeyHint.Down),
            new("Adjust", 0, KeyHint.Left, KeyHint.Right),
            new("Play", PlayAction, "Enter"),
            new("Options", OptionsAction, "O"),
        };

        private readonly IReadOnlyList<RhythmSong> songs;
        private readonly SessionContext context;
        private readonly PreviewPlayer preview;
        private readonly Dictionary<RhythmSong, ChartSet> sets;
        private readonly HashSet<RhythmSong> failed = new();
        private readonly List<MenuEntry> items = new();

        private OnsetAnalyzer? analyzer;
        private RhythmSong? pending;
        private int selection;
        private bool scalingFocused;
        private float baseLevel = DifficultySettings.BaseDefault;
        private float scaling = DifficultySettings.ScalingDefault;
        private int difficultyKey = int.MinValue;
        private int densityKey = int.MinValue;

        public SongSelectController(IReadOnlyList<RhythmSong> songs, SessionContext context, PreviewPlayer preview, Dictionary<RhythmSong, ChartSet> sets)
        {
            this.songs = songs;
            this.context = context;
            this.preview = preview;
            this.sets = sets;
        }

        public RhythmSong? ChosenSong { get; private set; }

        public RunSettings ChosenSettings => new(baseLevel, scaling, context.Resting.Bpm);

        private SongSelectView View => context.Stage.SongSelect;

        private RhythmSong? Current => selection < songs.Count ? songs[selection] : null;

        private RhythmSong? Target => pending != null ? pending : Current;

        public void Enter()
        {
            ChosenSong = null;
            pending = null;
            items.Clear();
            foreach (RhythmSong song in songs)
                items.Add(EntryFor(song));

            View.List.UseVerticalKeys = false;
            View.List.AllowBack = false;
            View.List.SetItems(items, selection);
            View.SetVisible(true);
            context.Stage.KeyBar.Set(Hints);
            PointerInput.ShowCursor(true);
            difficultyKey = int.MinValue;
            densityKey = int.MinValue;
            OnSelectionChanged();
        }

        public void Leave()
        {
            SaveDifficulty();
            context.Resting.Save();
            View.SetVisible(false);
        }

        public Navigation Tick(float deltaTime)
        {
            context.Resting.Observe(deltaTime, context.Monitor);
            View.TickHeart(deltaTime, context.Monitor, context.Resting.Bpm);
            View.Tick(deltaTime);
            StepAnalysis();
            RefreshDifficulty();
            RefreshDensity();

            if (pending != null)
                return TickPending();

            Navigation clicked = HandleKeyBar();
            if (clicked != Navigation.None)
                return clicked;

            HandleSliderPointer();
            HandleFocusKeys();
            if (LayoutInput.GetKeyDown(KeyCode.Tab) && View.List.Step(MenuKeys.Fine() ? -1 : 1) == MenuAction.Moved)
                OnMoved();
            if (LayoutInput.GetKeyDown(KeyCode.O))
            {
                context.Sounds.Confirm();
                return Navigation.Options;
            }

            return HandleList(deltaTime);
        }

        private static MenuEntry EntryFor(RhythmSong song)
        {
            SongRecord record = SongRecords.Load(song);
            string grade = record.HasRecord ? $"<color=#{Theme.Hex(RhythmTheme.Grade(record.Grade))}>{GradeCalculator.Label(record.Grade)}</color>" : string.Empty;
            return new MenuEntry(song.Title, grade, song.Artist);
        }

        private static float SliderStep()
        {
            return MenuKeys.Fine() ? DifficultySettings.SliderFineStep : DifficultySettings.SliderStep;
        }

        private static float Snap(float value, float step)
        {
            return (float)Math.Round(value / step) * step;
        }

        private Navigation HandleList(float deltaTime)
        {
            switch (View.List.HandleInput(deltaTime, context.Mouse))
            {
                case MenuAction.Moved:
                    OnMoved();
                    return Navigation.None;
                case MenuAction.Confirmed:
                    return Confirm();
                case MenuAction.Decrease:
                    Adjust(-1);
                    return Navigation.None;
                case MenuAction.Increase:
                    Adjust(1);
                    return Navigation.None;
                default:
                    return Navigation.None;
            }
        }

        private Navigation HandleKeyBar()
        {
            switch (context.Stage.KeyBar.Clicked(context.Mouse))
            {
                case PlayAction:
                    return Confirm();
                case OptionsAction:
                    context.Sounds.Confirm();
                    return Navigation.Options;
                case NextSongAction:
                    if (View.List.Step(1) == MenuAction.Moved)
                        OnMoved();
                    return Navigation.None;
                default:
                    return Navigation.None;
            }
        }

        private void HandleSliderPointer()
        {
            float? baseDrag = View.BaseSlider.Drag(context.Mouse);
            if (baseDrag.HasValue)
            {
                scalingFocused = false;
                baseLevel = Snap(DifficultySettings.BaseMin + (baseDrag.Value * (DifficultySettings.BaseMax - DifficultySettings.BaseMin)), SliderStep());
            }

            float? scalingDrag = View.ScalingSlider.Drag(context.Mouse);
            if (scalingDrag.HasValue)
            {
                scalingFocused = true;
                scaling = Snap(DifficultySettings.ScalingMin + (scalingDrag.Value * (DifficultySettings.ScalingMax - DifficultySettings.ScalingMin)), SliderStep());
            }
        }

        private void HandleFocusKeys()
        {
            bool up = MenuKeys.Up(false);
            bool down = MenuKeys.Down(false);
            if ((up && scalingFocused) || (down && !scalingFocused))
            {
                scalingFocused = down;
                context.Sounds.Move();
            }
        }

        private void OnMoved()
        {
            context.Sounds.Move();
            SaveDifficulty();
            selection = View.List.Selected;
            OnSelectionChanged();
        }

        private void Adjust(int direction)
        {
            float step = SliderStep() * direction;
            if (scalingFocused)
                scaling = Math.Clamp(Snap(scaling + step, DifficultySettings.SliderFineStep), DifficultySettings.ScalingMin, DifficultySettings.ScalingMax);
            else
                baseLevel = Math.Clamp(Snap(baseLevel + step, DifficultySettings.SliderFineStep), DifficultySettings.BaseMin, DifficultySettings.BaseMax);
            context.Sounds.Move();
        }

        private void RefreshDifficulty()
        {
            View.BaseSlider.SetFocus(!scalingFocused);
            View.ScalingSlider.SetFocus(scalingFocused);
            HeartRateMonitor monitor = context.Monitor;
            float exertion = monitor.Exertion(context.Resting.Bpm);
            bool live = monitor.HasReading;
            int key = HashCode.Combine(Mathf.RoundToInt(baseLevel * 10f), Mathf.RoundToInt(scaling * 10f), Mathf.RoundToInt(context.Resting.Bpm), Mathf.RoundToInt(exertion * 100f), live);
            if (key == difficultyKey)
                return;

            difficultyKey = key;
            View.SetDifficulty(baseLevel, scaling, context.Resting.Bpm, exertion, live);
        }

        private void RefreshDensity()
        {
            RhythmSong? song = Current;
            ChartSet? set = song != null && sets.TryGetValue(song, out ChartSet? ready) ? ready : null;
            float progress = analyzer != null && analyzer.Song == song ? analyzer.Progress : 0f;
            int key = set != null ? HashCode.Combine(set, Mathf.RoundToInt(baseLevel * 10f), Mathf.RoundToInt(scaling * 10f)) : Mathf.RoundToInt(progress * 100f);
            if (key == densityKey)
                return;

            densityKey = key;
            View.SetDensity(set, baseLevel, scaling, progress);
        }

        private Navigation TickPending()
        {
            if (MenuKeys.Back())
            {
                context.Sounds.Back();
                pending = null;
                return Navigation.None;
            }

            if (pending != null && sets.ContainsKey(pending))
            {
                ChosenSong = pending;
                pending = null;
                return Navigation.Play;
            }

            if (pending != null && failed.Contains(pending))
                pending = null;
            return Navigation.None;
        }

        private Navigation Confirm()
        {
            RhythmSong? song = Current;
            if (song == null || song.Clip == null || failed.Contains(song))
            {
                context.Sounds.Back();
                return Navigation.None;
            }

            context.Sounds.Confirm();
            SaveDifficulty();
            if (sets.ContainsKey(song))
            {
                ChosenSong = song;
                return Navigation.Play;
            }

            pending = song;
            return Navigation.None;
        }

        private void SaveDifficulty()
        {
            RhythmSong? song = Current;
            if (song != null)
                SongPreferences.Save(song, baseLevel, scaling);
        }

        private void OnSelectionChanged()
        {
            RhythmSong? song = Current;
            preview.Play(song);
            if (song == null)
            {
                View.ShowEmpty();
                return;
            }

            baseLevel = SongPreferences.Base(song);
            scaling = SongPreferences.Scaling(song);
            double bpm = sets.TryGetValue(song, out ChartSet? set) ? set.Analysis.Bpm : song.Bpm;
            View.ShowSong(song, SongRecords.Load(song), bpm);
            densityKey = int.MinValue;
        }

        private void StepAnalysis()
        {
            RhythmSong? wanted = Target;
            if (wanted == null || wanted.Clip == null || sets.ContainsKey(wanted) || failed.Contains(wanted))
                return;

            if (analyzer == null || analyzer.Song != wanted)
                analyzer = new OnsetAnalyzer(wanted);

            analyzer.Step(pending != null ? PreparingBudgetMs : IdleBudgetMs);
            if (analyzer.Result != null)
            {
                sets[wanted] = analyzer.Result;
                analyzer = null;
                if (wanted == Current)
                    OnSelectionChanged();
            }
            else if (analyzer.Failed)
            {
                failed.Add(wanted);
                analyzer = null;
            }
        }
    }
}
