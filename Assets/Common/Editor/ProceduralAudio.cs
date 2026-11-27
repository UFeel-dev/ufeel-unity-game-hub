using System;
using System.IO;
using System.Text;

namespace GameHub.Common.Editor
{
    public static class ProceduralAudio
    {
        public const int Rate = 44100;

        private static readonly (float Delay, float Gain)[] BellEchoes = { (0f, 1f), (0.36f, 0.4f), (0.72f, 0.16f) };

        public static int Count(float seconds) => (int)MathF.Round(seconds * Rate);

        public static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);

        public static float Lerp(float from, float to, float t) => from + ((to - from) * Clamp01(t));

        public static float Noise(Random random) => ((float)random.NextDouble() * 2f) - 1f;

        public static float Sine(float frequency, float t) => MathF.Sin(2f * MathF.PI * frequency * t);

        public static float Bell(float frequency, float t, float decay)
        {
            float envelope = MathF.Exp(-t * decay) * Clamp01(t * 400f);
            return envelope * ((Sine(frequency, t) * 0.7f) + (Sine(frequency * 2.76f, t) * 0.2f * MathF.Exp(-t * 6f)) + (Sine(frequency * 5.4f, t) * 0.1f * MathF.Exp(-t * 12f)));
        }

        public static float Arpeggio(float t, float spacing, params float[] notes)
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

        public static float[] Wave(float seconds, float peak, Func<float, float> wave)
        {
            float[] samples = new float[Count(seconds)];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = wave((float)i / Rate);
            return peak > 0f ? Normalize(samples, peak) : samples;
        }

        public static float[] Normalize(float[] samples, float peak)
        {
            float max = 0f;
            foreach (float sample in samples)
                max = MathF.Max(max, MathF.Abs(sample));
            if (max > 0f)
            {
                for (int i = 0; i < samples.Length; i++)
                    samples[i] *= peak / max;
            }

            return samples;
        }

        public static void AddPad(float[] samples, float[][] chords, float[] detune, float level, float harmonic)
        {
            int slot = samples.Length / chords.Length;
            int ramp = slot / 2;
            int length = slot + ramp;
            for (int c = 0; c < chords.Length; c++)
            {
                int start = (c * slot) - (ramp / 2);
                foreach (float note in chords[c])
                {
                    foreach (float factor in detune)
                        AddTone(samples, start, length, ramp, note * factor, level);
                    AddTone(samples, start, length, ramp, note * 2f, level * harmonic);
                }
            }
        }

        public static void AddBells(float[] samples, float[] notes, float level, float decay, int seed)
        {
            Random random = new(seed);
            float seconds = (float)samples.Length / Rate;
            int length = Count(3f);
            float time = 1.5f;
            while (time < seconds)
            {
                float note = notes[random.Next(notes.Length)];
                foreach ((float delay, float gain) in BellEchoes)
                {
                    int start = Count(time + delay);
                    for (int i = 0; i < length; i++)
                        samples[Wrap(start + i, samples.Length)] += Bell(note, (float)i / Rate, decay) * level * gain;
                }

                time += 2.5f + ((float)random.NextDouble() * 3.5f);
            }
        }

        public static void AddAir(float[] samples, float level, int seed)
        {
            const float Smoothing = 0.02f;
            Random random = new(seed);
            float[] noise = new float[samples.Length];
            for (int i = 0; i < noise.Length; i++)
                noise[i] = Noise(random);

            float[] air = new float[samples.Length];
            float state = 0f;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < noise.Length; i++)
                {
                    state += Smoothing * (noise[i] - state);
                    air[i] = state;
                }
            }

            Normalize(air, level);
            for (int i = 0; i < samples.Length; i++)
                samples[i] += air[i];
        }

        public static void WriteWav(string path, float[] samples)
        {
            using BinaryWriter writer = new(File.Create(path));
            int dataSize = samples.Length * 2;
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(Rate);
            writer.Write(Rate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);
            foreach (float sample in samples)
                writer.Write((short)MathF.Round(Math.Clamp(sample, -1f, 1f) * short.MaxValue));
        }

        private static int Wrap(int index, int length) => ((index % length) + length) % length;

        private static void AddTone(float[] samples, int start, int length, int ramp, float frequency, float level)
        {
            double step = 2.0 * Math.PI * frequency / Rate;
            double cos = Math.Cos(step);
            double sin = Math.Sin(step);
            double x = 1.0;
            double y = 0.0;
            for (int i = 0; i < length; i++)
            {
                float window = 1f;
                if (i < ramp)
                    window = MathF.Pow(MathF.Sin(0.5f * MathF.PI * i / ramp), 2f);
                else if (i > length - ramp)
                    window = MathF.Pow(MathF.Sin(0.5f * MathF.PI * (length - i) / ramp), 2f);

                samples[Wrap(start + i, samples.Length)] += (float)y * level * window;
                double next = (x * cos) - (y * sin);
                y = (x * sin) + (y * cos);
                x = next;
            }
        }

        public sealed class SweepFilter
        {
            private const float Resonance = 0.35f;

            private float low;
            private float band;

            public float Next(float input, float cutoff)
            {
                float f = 2f * MathF.Sin(MathF.PI * cutoff / Rate);
                low += f * band;
                float high = input - low - (Resonance * band);
                band += f * high;
                return band;
            }
        }
    }
}
