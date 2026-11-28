using System;
using System.Collections.Generic;
using System.Globalization;
using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class RhythmGameManager : MonoBehaviour
    {
        private const float ResumeCountdown = 1.5f;
        private const int ContinueAction = 1;
        private const int RetryAction = 2;
        private const int RetryRow = 1;
        private const int SongListRow = 2;

        private static readonly KeyHint[] PauseHints =
        {
            new("Select", 0, KeyHint.Up, KeyHint.Down),
            new("Confirm", 0, "Enter"),
            new("Resume", 0, "Esc"),
        };

        private static readonly KeyHint[] ResultHints =
        {
            new("Continue", ContinueAction, "Enter"),
            new("Retry", RetryAction, "R"),
        };

        private static readonly MenuEntry[] PauseEntries =
        {
            new("Resume"),
            new("Retry"),
            new("Song list"),
        };

        [Header("Songs")]
        [SerializeField] private RhythmSongLibrary library;

        [Header("Audio")]
        [SerializeField] private AudioClip ecgBeepClip;

        private readonly Dictionary<RhythmSong, ChartSet> sets = new();

        private AudioClip flatlineClip;
        private AudioClip clickClip;
        private PreviewPlayer preview;
        private SessionContext context;
        private SongSelectController songSelect;
        private OptionsController options;
        private KeyBindingController keys;
        private OffsetCalibrationController offsetCalibration;
        private HudLayoutController hudLayout;
        private FlatlineSequence flatline;
        private GameSession? session;
        private RhythmState state;
        private float countdown;
        private int shownCountdown;
        private float flash;
        private bool focusLost;

        private StageView Stage => context.Stage;

        private void Awake()
        {
            flatlineClip = ToneFactory.CreateFlatline();
            clickClip = ToneFactory.CreateClick();
            StageView stage = new(transform, UiKit.Shared);
            AudioSource music = CreateSource("Music", AudioChannel.Music);
            preview = new PreviewPlayer(CreateSource("Preview", AudioChannel.Music));
            UiSounds sounds = new(CreateSource("Effects", AudioChannel.Effects), ecgBeepClip, flatlineClip);
            context = new SessionContext(stage, music, new HeartRateMonitor(), sounds);
            songSelect = new SongSelectController(library != null ? library.Songs : Array.Empty<RhythmSong>(), context, preview, sets);
            options = new OptionsController(context);
            keys = new KeyBindingController(context);
            offsetCalibration = new OffsetCalibrationController(context, new[] { CreateSource("Click A", AudioChannel.Effects), CreateSource("Click B", AudioChannel.Effects) }, clickClip);
            hudLayout = new HudLayoutController(context);
            flatline = new FlatlineSequence(context);
        }

        private void Start()
        {
            EnterSongSelect();
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            context.Monitor.Tick(deltaTime);
            context.Mouse.Tick();
            preview.Tick(deltaTime);
            Stage.Tick(deltaTime, Tint(), context.Monitor.Pulse, FlashIntensity());

            if (!UiFocus.IsFree)
            {
                LoseFocus();
                return;
            }

            if (focusLost)
            {
                focusLost = false;
                PointerInput.ShowCursor(state is not (RhythmState.Playing or RhythmState.Resuming));
            }

            switch (state)
            {
                case RhythmState.SongSelect:
                    Route(songSelect.Tick(deltaTime));
                    break;
                case RhythmState.Options:
                    Route(options.Tick(deltaTime));
                    break;
                case RhythmState.Keys:
                    Route(keys.Tick());
                    break;
                case RhythmState.OffsetCalibration:
                    Route(offsetCalibration.Tick());
                    break;
                case RhythmState.HudLayout:
                    Route(hudLayout.Tick(deltaTime));
                    break;
                default:
                    UpdateRun(deltaTime);
                    break;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && state is RhythmState.Playing or RhythmState.Resuming)
                Pause();
        }

        private void OnDestroy()
        {
            Destroy(flatlineClip);
            Destroy(clickClip);
        }

        private void LoseFocus()
        {
            focusLost = true;
            if (state is RhythmState.Playing or RhythmState.Resuming)
                Pause();
        }

        private Color Tint()
        {
            if (state == RhythmState.HudLayout)
                return RhythmTheme.Level(HudLayoutController.DemoLevel);
            if (session != null && state is RhythmState.Playing or RhythmState.Paused or RhythmState.Resuming or RhythmState.Flatline)
                return RhythmTheme.Level(session.Level);
            return Theme.Accent;
        }

        private float FlashIntensity()
        {
            bool flashing = state is RhythmState.Playing or RhythmState.Paused or RhythmState.Resuming or RhythmState.Flatline or RhythmState.HudLayout;
            return flashing ? flash : 0f;
        }

        private AudioSource CreateSource(string sourceName, AudioChannel channel)
        {
            GameObject holder = new(sourceName);
            holder.transform.SetParent(transform, false);
            AudioSource source = holder.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            GameAudio.Route(source, channel);
            return source;
        }

        private void Route(Navigation navigation)
        {
            switch (navigation)
            {
                case Navigation.Play when songSelect.ChosenSong != null:
                    songSelect.Leave();
                    StartSession(songSelect.ChosenSong, sets[songSelect.ChosenSong], songSelect.ChosenSettings);
                    break;
                case Navigation.Options:
                    LeaveCurrent();
                    options.Enter();
                    state = RhythmState.Options;
                    break;
                case Navigation.Keys:
                    LeaveCurrent();
                    keys.Enter();
                    state = RhythmState.Keys;
                    break;
                case Navigation.OffsetCalibration:
                    LeaveCurrent();
                    preview.Stop();
                    offsetCalibration.Enter();
                    state = RhythmState.OffsetCalibration;
                    break;
                case Navigation.HudLayout:
                    LeaveCurrent();
                    flash = RhythmSettings.FlashPercent / 100f;
                    hudLayout.Enter();
                    state = RhythmState.HudLayout;
                    break;
                case Navigation.Back:
                    GoBack();
                    break;
            }
        }

        private void GoBack()
        {
            LeaveCurrent();
            if (state == RhythmState.Options)
            {
                EnterSongSelect();
                return;
            }

            options.Enter();
            state = RhythmState.Options;
        }

        private void LeaveCurrent()
        {
            switch (state)
            {
                case RhythmState.SongSelect:
                    songSelect.Leave();
                    break;
                case RhythmState.Options:
                    options.Leave();
                    break;
                case RhythmState.Keys:
                    keys.Leave();
                    break;
                case RhythmState.OffsetCalibration:
                    offsetCalibration.Leave();
                    break;
                case RhythmState.HudLayout:
                    hudLayout.Leave();
                    break;
            }
        }

        private void EnterSongSelect()
        {
            session?.End();
            session = null;
            Stage.HideScreens();
            Stage.Overlay.SetHeadline(string.Empty, Color.clear);
            Stage.ShowGameplay(false);
            songSelect.Enter();
            state = RhythmState.SongSelect;
        }

        private void StartSession(RhythmSong song, ChartSet set, RunSettings settings)
        {
            session?.End();
            preview.Stop();
            Stage.HideScreens();
            Stage.Overlay.SetHeadline(string.Empty, Color.clear);
            Stage.KeyBar.SetVisible(false);
            flash = RhythmSettings.FlashPercent / 100f;
            session = new GameSession(song, set, settings, context);
            session.Begin();
            state = RhythmState.Playing;
        }

        private void UpdateRun(float deltaTime)
        {
            switch (state)
            {
                case RhythmState.Playing:
                    UpdatePlaying(deltaTime);
                    break;
                case RhythmState.Paused:
                    UpdatePaused(deltaTime);
                    break;
                case RhythmState.Resuming:
                    UpdateResuming(deltaTime);
                    break;
                case RhythmState.Flatline:
                    if (flatline.Tick(deltaTime))
                        Finish(true);
                    break;
                case RhythmState.Results:
                    UpdateResults();
                    break;
            }
        }

        private void UpdatePlaying(float deltaTime)
        {
            if (session == null)
                return;
            if (MenuKeys.Back())
            {
                Pause();
                return;
            }

            switch (session.Tick(deltaTime))
            {
                case SessionStatus.Failed:
                    flatline.Begin(session.Level);
                    state = RhythmState.Flatline;
                    break;
                case SessionStatus.Completed:
                    Finish(false);
                    break;
            }
        }

        private void Pause()
        {
            if (session == null)
                return;

            session.Pause();
            Stage.Overlay.SetHeadline(string.Empty, Color.clear);
            Stage.ShowScrim(true);
            Stage.Pause.Show("Paused", PauseEntries, 0);
            Stage.KeyBar.Set(PauseHints);
            Stage.KeyBar.SetVisible(true);
            PointerInput.ShowCursor(true);
            context.Sounds.Back();
            state = RhythmState.Paused;
        }

        private void UpdatePaused(float deltaTime)
        {
            switch (Stage.Pause.List.HandleInput(deltaTime, context.Mouse))
            {
                case MenuAction.Moved:
                    context.Sounds.Move();
                    break;
                case MenuAction.Back:
                    BeginResume();
                    break;
                case MenuAction.Confirmed when Stage.Pause.List.Selected == RetryRow:
                    Retry();
                    break;
                case MenuAction.Confirmed when Stage.Pause.List.Selected == SongListRow:
                    context.Sounds.Back();
                    EnterSongSelect();
                    break;
                case MenuAction.Confirmed:
                    BeginResume();
                    break;
            }
        }

        private void BeginResume()
        {
            context.Sounds.Confirm();
            Stage.Pause.Hide();
            Stage.ShowScrim(false);
            Stage.KeyBar.SetVisible(false);
            PointerInput.ShowCursor(false);
            countdown = ResumeCountdown;
            shownCountdown = -1;
            state = RhythmState.Resuming;
        }

        private void UpdateResuming(float deltaTime)
        {
            countdown -= deltaTime;
            int remaining = Mathf.CeilToInt(countdown);
            if (remaining != shownCountdown)
            {
                shownCountdown = remaining;
                Stage.Overlay.SetHeadline(remaining > 0 ? remaining.ToString(CultureInfo.InvariantCulture) : string.Empty, Theme.Text);
            }

            if (countdown > 0f || session == null)
                return;

            session.Resume();
            state = RhythmState.Playing;
        }

        private void Finish(bool failed)
        {
            if (session == null)
                return;

            RunResult result = session.BuildResult(failed);
            session.End();
            if (failed)
                flatline.End();

            bool isNewBest = SongRecords.TrySaveBest(result);
            Stage.Results.Show(result, isNewBest);
            Stage.KeyBar.Set(ResultHints);
            Stage.KeyBar.SetVisible(true);
            PointerInput.ShowCursor(true);
            state = RhythmState.Results;
        }

        private void UpdateResults()
        {
            int clicked = Stage.KeyBar.Clicked(context.Mouse);
            if (clicked == RetryAction || LayoutInput.GetKeyDown(KeyCode.R))
            {
                Retry();
                return;
            }

            if (clicked != ContinueAction && !MenuKeys.Confirm() && !MenuKeys.Back())
                return;

            context.Sounds.Confirm();
            EnterSongSelect();
        }

        private void Retry()
        {
            if (session == null)
                return;

            context.Sounds.Confirm();
            StartSession(session.Song, session.Set, session.Settings);
        }
    }
}
