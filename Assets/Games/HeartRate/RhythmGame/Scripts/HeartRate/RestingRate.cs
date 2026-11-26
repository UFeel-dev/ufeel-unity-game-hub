using System.Collections.Generic;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class RestingRate
    {
        private const float MinBpm = 40f;
        private const float MaxBpm = 120f;
        private const float Step = 5f;
        private const string Key = "GameHub.RhythmGame.RestingBpm";
        private const int WindowSamples = 30;
        private const int MinSamples = 15;
        private const float SampleInterval = 1f;
        private const float Percentile = 0.25f;

        private readonly List<float> samples = new(WindowSamples);
        private readonly List<float> sorted = new(WindowSamples);
        private bool isSet;
        private float timer;

        public RestingRate()
        {
            isSet = PlayerPrefs.HasKey(Key);
            Bpm = PlayerPrefs.GetFloat(Key, DifficultySettings.DefaultRestingBpm);
        }

        public float Bpm { get; private set; }

        public void Observe(float deltaTime, HeartRateMonitor monitor)
        {
            timer += deltaTime;
            if (timer < SampleInterval)
                return;

            timer = 0f;
            if (!monitor.HasReading)
                return;

            samples.Add(monitor.SmoothedBpm);
            if (samples.Count > WindowSamples)
                samples.RemoveAt(0);
            if (samples.Count < MinSamples)
                return;

            sorted.Clear();
            sorted.AddRange(samples);
            sorted.Sort();
            float measured = sorted[(int)((sorted.Count - 1) * Percentile)];
            if (!isSet || measured < Bpm)
                Set(measured);
        }

        public void Adjust(int steps, bool fine)
        {
            Set(Mathf.Round(Bpm + (steps * (fine ? 1f : Step))));
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(Key, Bpm);
            PlayerPrefs.Save();
        }

        private void Set(float bpm)
        {
            Bpm = Mathf.Clamp(bpm, MinBpm, MaxBpm);
            isSet = true;
        }
    }
}
