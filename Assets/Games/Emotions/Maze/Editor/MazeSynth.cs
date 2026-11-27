using System;
using System.IO;
using System.Text;

namespace GameHub.Maze.Editor
{
    public static class MazeSynth
    {
        public const int Rate = 44100;
        public const int StepVariants = 4;

        private const float AmbientSeconds = 32f;
        private const float PadLevel = 0.2f;
        private const float PadHarmonic = 0.12f;
        private const float BellLevel = 0.18f;
        private const float BellDecay = 1.6f;
        private const float AirLevel = 0.06f;

        private static readonly float[][] PadChords =
        {
            new[] { 110f, 164.81f, 220f, 261.63f, 329.63f },
            new[] { 87.31f, 130.81f, 220f, 261.63f, 329.63f },
            new[] { 130.81f, 196f, 261.63f, 329.63f, 392f },
            new[] { 98f, 146.83f, 196f, 246.94f, 293.66f },
        };

        private static readonly float[] PadDetune = { 0.998f, 1.002f };
        private static readonly float[] BellNotes = { 440f, 523.25f, 587.33f, 659.25f, 783.99f, 880f };
        private static readonly (float Delay, float Gain)[] BellEchoes = { (0f, 1f), (0.36f, 0.4f), (0.72f, 0.16f) };

        public static float[] Ambient()
        {
            float[] samples = new float[Count(AmbientSeconds)];
            AddPad(samples);
            AddBells(samples);
            AddAir(samples);
            return Normalize(samples, 0.5f);
        }

        public static float[] Charge()
        {
            return Wave(2f, 0.6f, t => (Sine(220f, t) * 0.4f) + (Sine(330f, t) * 0.25f) + (Sine(440f, t) * 0.15f) + (Sine(441f, t) * 0.15f));
        }

        public static float[] DoorOpen()
        {
            Random random = new(11);
            SweepFilter filter = new();
            return Wave(1.4f, 0.7f, t =>
            {
                float envelope = Clamp01(t / 0.25f) * MathF.Exp(-MathF.Max(0f, t - 0.25f) * 3f);
                float noise = filter.Next(Noise(random), Lerp(300f, 2500f, t / 1.4f));
                float chime = t > 0.15f ? (Bell(880f, t - 0.15f, 3f) + (Bell(1320f, t - 0.15f, 3.5f) * 0.6f)) * 0.3f : 0f;
                return (noise * envelope * 0.8f) + chime;
            });
        }

        public static float[] DoorClose()
        {
            Random random = new(13);
            SweepFilter filter = new();
            return Wave(0.8f, 0.7f, t =>
            {
                float thud = Sine(70f * (1f - (t * 0.3f)), t) * MathF.Exp(-t * 9f) * Clamp01(t * 300f);
                float noise = filter.Next(Noise(random), Lerp(2000f, 300f, t / 0.8f)) * MathF.Exp(-t * 6f);
                return thud + (noise * 0.4f);
            });
        }

        public static float[] Hint() => Wave(1.4f, 0f, t => Arpeggio(t, 0.12f, 659.26f, 880f, 1108.73f));

        public static float[] Bonus() => Wave(1.2f, 0f, t => Arpeggio(t, 0.07f, 1318.5f, 1568f, 1760f, 2093f, 2637f) * 0.7f);

        public static float[] Win() => Wave(3f, 0f, t => Arpeggio(t, 0.15f, 440f, 554.37f, 659.26f, 880f, 1108.73f));

        public static float[] Trap()
        {
            Random random = new(17);
            float phase = 0f;
            return Wave(1.3f, 0.6f, t =>
            {
                phase += Lerp(420f, 110f, Clamp01(t / 1.1f)) / Rate;
                float tone = 0f;
                for (int h = 1; h <= 5; h++)
                    tone += MathF.Sin(2f * MathF.PI * phase * h) / h;
                float envelope = Clamp01(t * 50f) * MathF.Exp(-t * 2f);
                return ((tone * 0.5f) + (Noise(random) * 0.1f)) * envelope;
            });
        }

        public static float[] Step(int variant)
        {
            Random random = new(variant + 1);
            return Wave(0.18f, 0f, t => ((Noise(random) * 0.5f) + Sine(90f, t)) * MathF.Exp(-t * 35f) * Clamp01(t * 2000f) * 0.5f);
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

        private static int Count(float seconds) => (int)MathF.Round(seconds * Rate);

        private static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);

        private static float Lerp(float from, float to, float t) => from + ((to - from) * Clamp01(t));

        private static float Noise(Random random) => ((float)random.NextDouble() * 2f) - 1f;

        private static float Sine(float frequency, float t) => MathF.Sin(2f * MathF.PI * frequency * t);

        private static int Wrap(int index, int length) => ((index % length) + length) % length;

        private static float Bell(float frequency, float t, float decay)
        {
            float envelope = MathF.Exp(-t * decay) * Clamp01(t * 400f);
            return envelope * ((Sine(frequency, t) * 0.7f) + (Sine(frequency * 2.76f, t) * 0.2f * MathF.Exp(-t * 6f)) + (Sine(frequency * 5.4f, t) * 0.1f * MathF.Exp(-t * 12f)));
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

        private static float[] Wave(float seconds, float peak, Func<float, float> wave)
        {
            float[] samples = new float[Count(seconds)];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = wave((float)i / Rate);
            return peak > 0f ? Normalize(samples, peak) : samples;
        }

        private static float[] Normalize(float[] samples, float peak)
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

        private static void AddPad(float[] samples)
        {
            int slot = samples.Length / PadChords.Length;
            int ramp = slot / 2;
            int length = slot + ramp;
            for (int c = 0; c < PadChords.Length; c++)
            {
                int start = (c * slot) - (ramp / 2);
                foreach (float note in PadChords[c])
                {
                    foreach (float detune in PadDetune)
                        AddTone(samples, start, length, ramp, note * detune, PadLevel);
                    AddTone(samples, start, length, ramp, note * 2f, PadLevel * PadHarmonic);
                }
            }
        }

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

        private static void AddBells(float[] samples)
        {
            Random random = new(3);
            int length = Count(3f);
            float time = 1.5f;
            while (time < AmbientSeconds)
            {
                float note = BellNotes[random.Next(BellNotes.Length)];
                foreach ((float delay, float gain) in BellEchoes)
                {
                    int start = Count(time + delay);
                    for (int i = 0; i < length; i++)
                        samples[Wrap(start + i, samples.Length)] += Bell(note, (float)i / Rate, BellDecay) * BellLevel * gain;
                }

                time += 2.5f + ((float)random.NextDouble() * 3.5f);
            }
        }

        private static void AddAir(float[] samples)
        {
            const float Smoothing = 0.02f;
            Random random = new(7);
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

            Normalize(air, AirLevel);
            for (int i = 0; i < samples.Length; i++)
                samples[i] += air[i];
        }

        private sealed class SweepFilter
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
