using System;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class SongConductor
    {
        private const double ResyncThreshold = 0.1;

        private readonly AudioSource source;
        private readonly DspClock clock = new();

        private double anchorDsp;
        private double anchorSong;
        private double smoothed;
        private int lastSamples = -1;

        public SongConductor(AudioSource source)
        {
            this.source = source;
        }

        public double OffsetSeconds { get; set; }

        public double SongTime => smoothed - (OffsetSeconds * Rate);

        public bool IsPaused { get; private set; }

        public float Rate { get; private set; } = 1f;

        public void Begin(AudioClip clip, double leadIn, float rate)
        {
            source.Stop();
            source.clip = clip;
            IsPaused = false;
            Rate = rate;
            source.pitch = rate;
            smoothed = -leadIn * rate;
            Schedule(smoothed, 0d);
        }

        public void Tick()
        {
            if (IsPaused)
                return;

            clock.Tick();
            smoothed = Math.Max(smoothed, Position());
            Resync();
        }

        public void SetRate(float rate)
        {
            if (IsPaused || smoothed < 0d || rate == Rate)
                return;

            anchorSong = Position();
            anchorDsp = clock.Now;
            Rate = rate;
            source.pitch = rate;
        }

        public void Pause()
        {
            if (IsPaused)
                return;

            IsPaused = true;
            source.Stop();
        }

        public void Resume(double delay)
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            Schedule(smoothed, delay);
        }

        public void Stop()
        {
            source.Stop();
            IsPaused = false;
            Rate = 1f;
            source.pitch = 1f;
        }

        private double Position()
        {
            return anchorSong + ((clock.Now - anchorDsp) * Rate);
        }

        private void Resync()
        {
            AudioClip clip = source.clip;
            if (clip == null || !source.isPlaying || smoothed <= 0d || source.timeSamples == lastSamples)
                return;

            lastSamples = source.timeSamples;
            double audio = (double)lastSamples / clip.frequency;
            if (Math.Abs(audio - Position()) <= ResyncThreshold)
                return;

            anchorSong = audio;
            anchorDsp = clock.Now;
        }

        private void Schedule(double songTime, double delay)
        {
            clock.Reset();
            double startDsp = clock.Now + delay;
            lastSamples = -1;
            if (songTime > 0d && source.clip != null)
            {
                anchorSong = songTime;
                anchorDsp = startDsp;
                source.timeSamples = Math.Min((int)(songTime * source.clip.frequency), source.clip.samples - 1);
                source.PlayScheduled(startDsp);
                return;
            }

            anchorSong = 0d;
            anchorDsp = startDsp - (songTime / Rate);
            source.timeSamples = 0;
            source.PlayScheduled(anchorDsp);
        }
    }
}
