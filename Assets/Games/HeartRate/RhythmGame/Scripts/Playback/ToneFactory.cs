using UnityEngine;

namespace GameHub.RhythmGame
{
    public static class ToneFactory
    {
        private const int SampleRate = 44100;
        private const float Frequency = 1000f;
        private const float Seconds = 3f;
        private const float Amplitude = 0.3f;
        private const float Attack = 0.01f;
        private const float Release = 0.3f;

        public static AudioClip CreateClick()
        {
            const float clickSeconds = 0.03f;
            const float clickFrequency = 2000f;
            const int samples = (int)(SampleRate * clickSeconds);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)SampleRate;
                data[i] = 0.5f * Mathf.Exp(-time * 160f) * Mathf.Sin(2f * Mathf.PI * clickFrequency * time);
            }

            AudioClip clip = AudioClip.Create("Click", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateFlatline()
        {
            const int samples = (int)(SampleRate * Seconds);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)SampleRate;
                float envelope = Mathf.Min(1f, time / Attack) * Mathf.Clamp01((Seconds - time) / Release);
                data[i] = Amplitude * envelope * Mathf.Sin(2f * Mathf.PI * Frequency * time);
            }

            AudioClip clip = AudioClip.Create("Flatline", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
