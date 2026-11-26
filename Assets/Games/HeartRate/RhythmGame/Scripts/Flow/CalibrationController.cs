using System;
using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class CalibrationController
    {
        private const double Interval = 0.5;
        private const double LeadIn = 1.0;
        private const double ScheduleAhead = 0.3;
        private const double MaxError = 0.2;
        private const int WarmupBeats = 2;
        private const float PulseDecay = 8f;
        private const int ApplyAction = 1;
        private const int CancelAction = 2;
        private const int RestartAction = 3;

        private static readonly KeyHint[] TapHints =
        {
            new("Tap", 0, "Lane keys"),
            new("Cancel", CancelAction, "Esc"),
        };

        private static readonly KeyHint[] DoneHints =
        {
            new("Apply", ApplyAction, "Enter"),
            new("Again", RestartAction, "R"),
            new("Cancel", CancelAction, "Esc"),
        };

        private readonly SessionContext context;
        private readonly AudioSource[] sources;
        private readonly AudioClip click;
        private readonly DspClock clock = new();
        private readonly LaneInput input = new();
        private readonly List<double> offsets = new(StageView.CalibrationTaps);

        private double start;
        private double nextClick;
        private int nextSource;
        private int result;
        private bool done;

        public CalibrationController(SessionContext context, AudioSource[] sources, AudioClip click)
        {
            this.context = context;
            this.sources = sources;
            this.click = click;
        }

        private CalibrationView View => context.Stage.Calibration;

        public void Enter()
        {
            View.SetVisible(true);
            context.Stage.ShowScrim(true);
            PointerInput.ShowCursor(true);
            input.Reload();
            Restart();
        }

        public void Leave()
        {
            foreach (AudioSource source in sources)
                source.Stop();
            View.SetVisible(false);
            context.Stage.ShowScrim(false);
        }

        public Navigation Tick()
        {
            clock.Tick();
            ScheduleClicks();
            double beat = (clock.Now - start) / Interval;
            View.SetBeat(beat < 0d ? 0f : Mathf.Exp(-(float)(beat - Math.Floor(beat)) * PulseDecay));

            int clicked = context.Stage.KeyBar.Clicked(context.Mouse);
            if (clicked == CancelAction || MenuKeys.Back())
            {
                context.Sounds.Back();
                return Navigation.Back;
            }

            if (done)
                return TickDone(clicked);

            input.Tick();
            if (input.AnyPressed())
                RecordTap(beat);
            return Navigation.None;
        }

        private Navigation TickDone(int clicked)
        {
            if (clicked == RestartAction || LayoutInput.GetKeyDown(KeyCode.R))
            {
                Restart();
                return Navigation.None;
            }

            if (clicked != ApplyAction && !MenuKeys.Confirm())
                return Navigation.None;

            RhythmSettings.SetAudioOffset(result);
            context.Sounds.Confirm();
            context.Stage.Overlay.Toast(FormattableString.Invariant($"Audio offset {result:+0;-0;0} ms"));
            return Navigation.Back;
        }

        private void Restart()
        {
            clock.Reset();
            start = clock.Now + LeadIn;
            nextClick = start;
            offsets.Clear();
            done = false;
            View.SetProgress(0);
            View.SetResult(string.Empty);
            context.Stage.KeyBar.Set(TapHints);
        }

        private void ScheduleClicks()
        {
            if (done)
                return;

            while (nextClick < AudioSettings.dspTime + ScheduleAhead)
            {
                AudioSource source = sources[nextSource];
                source.clip = click;
                source.PlayScheduled(nextClick);
                nextSource = (nextSource + 1) % sources.Length;
                nextClick += Interval;
            }
        }

        private void RecordTap(double beat)
        {
            int nearest = (int)Math.Round(beat);
            double error = (beat - nearest) * Interval;
            if (nearest < WarmupBeats || Math.Abs(error) > MaxError)
                return;

            offsets.Add(error);
            View.SetProgress(offsets.Count);
            if (offsets.Count < StageView.CalibrationTaps)
                return;

            offsets.Sort();
            result = (int)Math.Round(offsets[offsets.Count / 2] * 1000d);
            done = true;
            foreach (AudioSource source in sources)
                source.Stop();
            View.SetResult(FormattableString.Invariant($"{result:+0;-0;0} ms"));
            context.Stage.KeyBar.Set(DoneHints);
        }
    }
}
