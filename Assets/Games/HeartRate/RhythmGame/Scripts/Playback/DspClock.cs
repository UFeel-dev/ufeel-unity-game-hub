using System;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class DspClock
    {
        private const double MaxExtrapolation = 0.05;

        private double lastDsp = -1d;
        private double lastRealtime;
        private double smoothed;

        public double Now => smoothed;

        public void Reset()
        {
            lastDsp = AudioSettings.dspTime;
            lastRealtime = Time.realtimeSinceStartupAsDouble;
            smoothed = lastDsp;
        }

        public void Tick()
        {
            double dsp = AudioSettings.dspTime;
            double now = Time.realtimeSinceStartupAsDouble;
            if (dsp != lastDsp)
            {
                lastDsp = dsp;
                lastRealtime = now;
            }
            smoothed = Math.Max(smoothed, lastDsp + Math.Min(now - lastRealtime, MaxExtrapolation));
        }
    }
}
