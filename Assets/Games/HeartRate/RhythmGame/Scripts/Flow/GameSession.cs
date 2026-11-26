using System.Collections.Generic;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class GameSession
    {
        private const double LeadIn = 2.0;
        private const double GenerationMargin = 1.5;
        private const double EndPadding = 1.0;
        private const double ResumeDelay = 0.05;
        private const float MusicVolume = 0.85f;

        private readonly SessionContext context;
        private readonly LiveChart live;
        private readonly NoteJudge judge = new();
        private readonly ScoreKeeper score = new();
        private readonly LaneInput input = new();
        private readonly HeartRateStats stats = new();
        private readonly List<NoteData> generated = new(32);
        private readonly string[] keyLabels;

        private double scrollTime;
        private bool showSpeed;

        public GameSession(RhythmSong song, ChartSet set, RunSettings settings, SessionContext context)
        {
            Song = song;
            Settings = settings;
            Set = set;
            this.context = context;
            live = new LiveChart(set);
            keyLabels = KeyBindings.PrimaryLabels();
        }

        public RhythmSong Song { get; }

        public ChartSet Set { get; }

        public RunSettings Settings { get; }

        public float Level { get; private set; }

        public void Begin()
        {
            ApplySettings();
            float exertion = context.Monitor.Exertion(Settings.RestingBpm);
            context.Music.volume = MusicVolume;
            context.Conductor.Begin(Song.Clip, LeadIn, TempoSettings.Rate(exertion));
            Level = Settings.Level(exertion);
            StageView stage = context.Stage;
            stage.ShowGameplay(true);
            stage.Hud.ResetRun();
            stage.Hud.ApplyLayout();
            stage.Playfield.SetStyle(RhythmSettings.NoteStyle);
            stage.Playfield.SetKeyLabels(keyLabels, RhythmSettings.ShowKeys);
            PointerInput.ShowCursor(false);
        }

        public SessionStatus Tick(float deltaTime)
        {
            SongConductor conductor = context.Conductor;
            conductor.Tick();
            double songTime = conductor.SongTime;
            if (SettingsHotkeys.Handle(context.Stage.Overlay))
                ApplySettings();

            HeartRateMonitor monitor = context.Monitor;
            float exertion = monitor.Exertion(Settings.RestingBpm);
            Level = Settings.Level(exertion);
            if (songTime >= 0d)
                stats.Record(deltaTime, monitor.SmoothedBpm, monitor.HasReading, Level);

            float targetRate = TempoSettings.Rate(exertion);
            if (songTime >= 0d)
                conductor.SetRate(Mathf.MoveTowards(conductor.Rate, targetRate, TempoSettings.ChangePerSecond * deltaTime));
            float playRate = conductor.Rate;
            double lookAhead = scrollTime + GenerationMargin;
            float plannedRate = Mathf.Max(playRate, Mathf.MoveTowards(playRate, targetRate, TempoSettings.ChangePerSecond * (float)lookAhead));

            generated.Clear();
            live.ScheduleUntil(songTime + (lookAhead * playRate), Level, plannedRate, generated);
            foreach (NoteData note in generated)
                judge.Add(note);

            input.Tick();
            judge.Tick(songTime, input, playRate);
            foreach (JudgementEvent judgementEvent in judge.FrameEvents)
                Apply(judgementEvent);

            Present(deltaTime, songTime, playRate);
            if (score.IsDead)
                return SessionStatus.Failed;

            bool chartDone = live.IsComplete && judge.Notes.Count == 0 && songTime > live.LastNoteEnd + EndPadding;
            return chartDone || songTime > Set.Analysis.Length + EndPadding ? SessionStatus.Completed : SessionStatus.Running;
        }

        public void Pause()
        {
            context.Conductor.Pause();
            context.Stage.Playfield.ShowKeyLabels(true);
        }

        public void Resume()
        {
            context.Conductor.Resume(ResumeDelay);
            context.Stage.Playfield.ShowKeyLabels(RhythmSettings.ShowKeys);
            PointerInput.ShowCursor(false);
        }

        public void End()
        {
            context.Conductor.Stop();
            context.Stage.ShowGameplay(false);
            judge.Clear();
        }

        public RunResult BuildResult(bool failed)
        {
            return new RunResult(Song, score, stats, Settings, failed);
        }

        private void ApplySettings()
        {
            scrollTime = RhythmSettings.ScrollTimeMs / 1000d;
            context.Conductor.OffsetSeconds = RhythmSettings.AudioOffsetMs / 1000d;
            showSpeed = TempoSettings.IsActive;
        }

        private void Apply(in JudgementEvent judgementEvent)
        {
            score.Apply(judgementEvent);
            context.Stage.Hud.ShowJudgement(judgementEvent);
            if (judgementEvent.Judgement != Judgement.Miss)
                context.Stage.Playfield.Burst(judgementEvent.Lane, Theme.Judgement(judgementEvent.Judgement));
        }

        private void Present(float deltaTime, double songTime, float playRate)
        {
            StageView stage = context.Stage;
            HeartRateMonitor monitor = context.Monitor;
            stage.Playfield.Render(songTime, judge.Notes, scrollTime * playRate, Set.Analysis);
            stage.Playfield.Tick(deltaTime, input, monitor.Pulse, Theme.Level(Level));
            stage.Playfield.SetHealth(score.Health, monitor.Pulse);
            stage.Playfield.SetProgress((float)(songTime / Set.Analysis.Length));
            stage.Hud.Tick(deltaTime, score);
            stage.Hud.TickHeart(deltaTime, monitor.SmoothedBpm, monitor.HasReading, monitor.Pulse, Level, showSpeed ? playRate : 0f, false);
        }
    }
}
