using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace GameHub.Maze
{
    public sealed class MazeSounds
    {
        private const int Rate = 44100;
        private const float AmbientSeconds = 16f;
        private const float ChargeSeconds = 2f;
        private const int StepVariants = 4;

        private readonly List<AudioClip> clips = new();

        public MazeSounds()
        {
            Ambient = Create("MazeAmbient", AmbientSeconds, Ambience);
            Charge = Create("MazeCharge", ChargeSeconds, ChargeHum);
            DoorOpen = Create("MazeDoorOpen", 1.4f, OpenWhoosh);
            DoorClose = Create("MazeDoorClose", 0.8f, CloseThud);
            Hint = CreateWave("MazeHint", 1.4f, t => Arpeggio(t, 0.12f, 659.26f, 880f, 1108.73f));
            Bonus = CreateWave("MazeBonus", 1.2f, t => Arpeggio(t, 0.07f, 1318.5f, 1568f, 1760f, 2093f, 2637f) * 0.7f);
            Trap = Create("MazeTrap", 1.3f, TrapDrop);
            Win = CreateWave("MazeWin", 3f, t => Arpeggio(t, 0.15f, 440f, 554.37f, 659.26f, 880f, 1108.73f));
            AudioClip[] steps = new AudioClip[StepVariants];
            for (int i = 0; i < StepVariants; i++)
            {
                Random random = new(i + 1);
                steps[i] = CreateWave($"MazeStep{i}", 0.18f, t => Step(t, random));
            }

            Steps = steps;
        }

        public AudioClip Ambient { get; }

        public AudioClip Charge { get; }

        public AudioClip DoorOpen { get; }

        public AudioClip DoorClose { get; }

        public AudioClip Hint { get; }

        public AudioClip Bonus { get; }

        public AudioClip Trap { get; }

        public AudioClip Win { get; }

        public IReadOnlyList<AudioClip> Steps { get; }

        public void Release()
        {
            foreach (AudioClip clip in clips)
                UnityEngine.Object.Destroy(clip);
            clips.Clear();
        }

        private static float Sine(float frequency, float t) => Mathf.Sin(2f * Mathf.PI * frequency * t);

        private static float Bell(float frequency, float t, float decay)
        {
            float envelope = Mathf.Exp(-t * decay) * Mathf.Clamp01(t * 400f);
            return envelope * ((Sine(frequency, t) * 0.7f) + (Sine(frequency * 2.76f, t) * 0.2f * Mathf.Exp(-t * 6f)) + (Sine(frequency * 5.4f, t) * 0.1f * Mathf.Exp(-t * 12f)));
        }

        private static float Arpeggio(float t, float spacing, params float[] notes)
        {
            float sum = 0f;
            for (int i = 0; i < notes.Length; i++)
            {
                float local = t - (i * spacing);
                if (local > 0f)
                    sum += Bell(notes[i], local, 3.5f);
            }

            return sum * 0.35f;
        }

        private static float[] Ambience(int count)
        {
            float[] partials = { 55f, 82.5f, 110f, 130.8125f, 165f };
            float[] levels = { 0.32f, 0.2f, 0.16f, 0.08f, 0.07f };
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Rate;
                float sum = 0f;
                for (int p = 0; p < partials.Length; p++)
                {
                    float tremolo = 0.65f + (0.35f * Sine((p + 1) / AmbientSeconds, t + p));
                    sum += levels[p] * tremolo * Sine(partials[p], t);
                }

                samples[i] = sum;
            }

            AddLoopingAir(samples, 0.12f);
            return Normalize(samples, 0.5f);
        }

        private static void AddLoopingAir(float[] samples, float level)
        {
            const float Smoothing = 0.02f;
            int fade = Rate;
            Random random = new(7);
            float[] air = new float[samples.Length + fade];
            float state = 0f;
            for (int i = 0; i < air.Length; i++)
            {
                state += Smoothing * (((float)random.NextDouble() * 2f) - 1f - state);
                air[i] = state;
            }

            Normalize(air, level);
            for (int i = 0; i < samples.Length; i++)
            {
                float mix = Mathf.Clamp01((float)i / fade);
                samples[i] += i < fade ? (air[i] * mix) + (air[samples.Length + i] * (1f - mix)) : air[i];
            }
        }

        private static float[] ChargeHum(int count)
        {
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Rate;
                samples[i] = (Sine(220f, t) * 0.4f) + (Sine(330f, t) * 0.25f) + (Sine(440f, t) * 0.15f) + (Sine(441f, t) * 0.15f);
            }

            return Normalize(samples, 0.6f);
        }

        private static float[] OpenWhoosh(int count)
        {
            Random random = new(11);
            float[] samples = new float[count];
            SweepFilter filter = new();
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Rate;
                float progress = t / 1.4f;
                float envelope = Mathf.Clamp01(t / 0.25f) * Mathf.Exp(-Mathf.Max(0f, t - 0.25f) * 3f);
                float noise = filter.Next(((float)random.NextDouble() * 2f) - 1f, Mathf.Lerp(300f, 2500f, progress));
                float chime = t > 0.15f ? (Bell(880f, t - 0.15f, 3f) + (Bell(1320f, t - 0.15f, 3.5f) * 0.6f)) * 0.3f : 0f;
                samples[i] = (noise * envelope * 0.8f) + chime;
            }

            return Normalize(samples, 0.7f);
        }

        private static float[] CloseThud(int count)
        {
            Random random = new(13);
            float[] samples = new float[count];
            SweepFilter filter = new();
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Rate;
                float thud = Sine(70f * (1f - (t * 0.3f)), t) * Mathf.Exp(-t * 9f) * Mathf.Clamp01(t * 300f);
                float noise = filter.Next(((float)random.NextDouble() * 2f) - 1f, Mathf.Lerp(2000f, 300f, t / 0.8f)) * Mathf.Exp(-t * 6f);
                samples[i] = thud + (noise * 0.4f);
            }

            return Normalize(samples, 0.7f);
        }

        private static float[] TrapDrop(int count)
        {
            Random random = new(17);
            float[] samples = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Rate;
                float frequency = Mathf.Lerp(420f, 110f, Mathf.Clamp01(t / 1.1f));
                phase += frequency / Rate;
                float tone = 0f;
                for (int h = 1; h <= 5; h++)
                    tone += Mathf.Sin(2f * Mathf.PI * phase * h) / h;
                float envelope = Mathf.Clamp01(t * 50f) * Mathf.Exp(-t * 2f);
                samples[i] = ((tone * 0.5f) + ((((float)random.NextDouble() * 2f) - 1f) * 0.1f)) * envelope;
            }

            return Normalize(samples, 0.6f);
        }

        private static float Step(float t, Random random)
        {
            float noise = ((float)random.NextDouble() * 2f) - 1f;
            return ((noise * 0.5f) + Sine(90f, t)) * Mathf.Exp(-t * 35f) * Mathf.Clamp01(t * 2000f) * 0.5f;
        }

        private static float[] Normalize(float[] samples, float peak)
        {
            float max = 0f;
            foreach (float sample in samples)
                max = Mathf.Max(max, Mathf.Abs(sample));
            if (max > 0f)
            {
                for (int i = 0; i < samples.Length; i++)
                    samples[i] *= peak / max;
            }

            return samples;
        }

        private AudioClip CreateWave(string name, float seconds, Func<float, float> wave)
        {
            return Create(name, seconds, count =>
            {
                float[] samples = new float[count];
                for (int i = 0; i < count; i++)
                    samples[i] = wave((float)i / Rate);
                return samples;
            });
        }

        private AudioClip Create(string name, float seconds, Func<int, float[]> synthesize)
        {
            int count = Mathf.RoundToInt(seconds * Rate);
            AudioClip clip = AudioClip.Create(name, count, 1, Rate, false);
            clip.SetData(synthesize(count), 0);
            clips.Add(clip);
            return clip;
        }

        private sealed class SweepFilter
        {
            private const float Resonance = 0.35f;

            private float low;
            private float band;

            public float Next(float input, float cutoff)
            {
                float f = 2f * Mathf.Sin(Mathf.PI * cutoff / Rate);
                low += f * band;
                float high = input - low - (Resonance * band);
                band += f * high;
                return band;
            }
        }
    }
}
