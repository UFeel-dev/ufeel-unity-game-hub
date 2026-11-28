#nullable enable

using System;
using static GameHub.Common.Editor.ProceduralAudio;

namespace GameHub.Common.Editor
{
    public static class GameSynth
    {
        public const int StepVariants = 4;

        private const float MusicSeconds = 32f;
        private const int SongBars = 16;
        private const int BarSteps = 16;
        private const float CarTempo = 126f;
        private const float AnimalsTempo = 98f;
        private const float EscapeRoomTempo = 84f;
        private const float TvSeconds = 8f;
        private const float TvLoopFade = 0.5f;

        private static readonly float[] PadDetune = { 0.998f, 1.002f };
        private static readonly float[] HorrorDetune = { 0.995f, 1.005f };

        private static readonly float[][] HubChords =
        {
            new[] { 130.81f, 196f, 246.94f, 329.63f, 392f },
            new[] { 110f, 164.81f, 196f, 261.63f, 329.63f },
            new[] { 87.31f, 130.81f, 220f, 329.63f, 440f },
            new[] { 98f, 146.83f, 246.94f, 329.63f, 392f },
        };

        private static readonly float[] HubBells = { 523.25f, 587.33f, 659.25f, 783.99f, 880f, 1046.5f };

        private static readonly float[][] MazeChords =
        {
            new[] { 110f, 130.81f, 164.81f, 220f },
            new[] { 116.54f, 146.83f, 174.61f, 233.08f },
            new[] { 92.5f, 110f, 130.81f, 185f },
            new[] { 82.41f, 123.47f, 164.81f, 207.65f },
        };

        private static readonly float[][] MazeTension =
        {
            new[] { 880f, 932.33f },
        };

        private static readonly float[] MazeBass = { 55f, 58.27f, 46.25f, 41.2f };

        private static readonly float[] MazeBells = { 440f, 466.16f, 622.25f };

        private static readonly float[][] AnimalsChords =
        {
            new[] { 174.61f, 261.63f, 329.63f, 440f },
            new[] { 146.83f, 220f, 293.66f, 349.23f },
            new[] { 116.54f, 233.08f, 293.66f, 349.23f },
            new[] { 130.81f, 196f, 329.63f, 392f },
        };

        private static readonly float[] AnimalsRoots = { 87.31f, 73.42f, 58.27f, 65.41f };

        private static readonly (int Step, int Tone)[] AnimalsMotifA = { (0, 3), (3, 2), (6, 1), (8, 2), (10, 3), (12, 1), (14, 2) };

        private static readonly (int Step, int Tone)[] AnimalsMotifB = { (0, 2), (2, 3), (4, 2), (6, 1), (10, 0), (12, 1), (13, 2) };

        private static readonly float[][] CarChords =
        {
            new[] { 123.47f, 185f, 246.94f, 293.66f },
            new[] { 98f, 146.83f, 246.94f, 293.66f },
            new[] { 146.83f, 220f, 293.66f, 369.99f },
            new[] { 110f, 164.81f, 277.18f, 329.63f },
        };

        private static readonly float[] CarBass = { 61.74f, 49f, 73.42f, 55f };

        private static readonly float[][] CarArp =
        {
            new[] { 246.94f, 293.66f, 369.99f, 493.88f },
            new[] { 196f, 246.94f, 293.66f, 392f },
            new[] { 220f, 293.66f, 369.99f, 440f },
            new[] { 220f, 277.18f, 329.63f, 440f },
        };

        private static readonly int[] ArpPattern = { 0, 1, 2, 3, 2, 1, 2, 1 };

        private static readonly float[][] SurvivorChords =
        {
            new[] { 110f, 164.81f, 220f, 261.63f },
            new[] { 87.31f, 174.61f, 220f, 261.63f },
            new[] { 98f, 146.83f, 196f, 246.94f },
            new[] { 82.41f, 164.81f, 207.65f, 246.94f },
        };

        private static readonly float[] SurvivorBass = { 55f, 43.65f, 49f, 41.2f };

        private static readonly float[][] HorrorChords =
        {
            new[] { 55f, 82.41f, 110f, 130.81f },
            new[] { 51.91f, 77.78f, 103.83f, 123.47f },
        };

        private static readonly float[][] EscapeRoomChords =
        {
            new[] { 110f, 164.81f, 220f, 261.63f },
            new[] { 87.31f, 130.81f, 220f, 261.63f },
            new[] { 146.83f, 220f, 293.66f, 349.23f },
            new[] { 82.41f, 123.47f, 207.65f, 329.63f },
        };

        private static readonly float[] EscapeRoomRoots = { 110f, 87.31f, 73.42f, 82.41f };

        private static readonly (int Step, float Note)[][] EscapeRoomPhraseA =
        {
            new[] { (0, 659.25f), (6, 587.33f), (8, 523.25f), (12, 493.88f) },
            new[] { (0, 440f), (8, 523.25f) },
            new[] { (0, 587.33f), (6, 698.46f), (8, 659.25f), (12, 587.33f) },
            new[] { (0, 493.88f), (8, 415.3f), (12, 329.63f) },
        };

        private static readonly (int Step, float Note)[][] EscapeRoomPhraseB =
        {
            new[] { (0, 880f), (4, 659.25f), (8, 523.25f), (12, 659.25f) },
            new[] { (0, 698.46f), (4, 659.25f), (8, 523.25f), (12, 440f) },
            new[] { (0, 587.33f), (4, 698.46f), (8, 880f), (12, 698.46f) },
            new[] { (0, 659.25f), (4, 587.33f), (8, 493.88f), (12, 415.3f) },
        };

        public static float[] MenuMove() => Wave(0.06f, 0.18f, t => Soft(1046.5f, t, 70f));

        public static float[] MenuConfirm() => Wave(0.3f, 0.24f, t => Chime(t, 0.05f, 20f, 783.99f, 1174.66f));

        public static float[] MenuBack() => Wave(0.3f, 0.22f, t => Chime(t, 0.05f, 20f, 783.99f, 587.33f));

        public static float[] Start() => Wave(0.6f, 0.3f, t => Chime(t, 0.08f, 10f, 659.25f, 987.77f));

        public static float[] Success() => Wave(0.7f, 0.3f, t => Chime(t, 0.06f, 10f, 880f, 1108.73f, 1318.51f));

        public static float[] Fail() => Wave(1.2f, 0.32f, t => Chime(t, 0.14f, 5f, 440f, 349.23f, 261.63f));

        public static float[] Win() => Wave(1.6f, 0.35f, t => Chime(t, 0.1f, 5f, 523.25f, 659.25f, 783.99f, 1046.5f));

        public static float[] Shot()
        {
            Random random = new(3);
            SweepFilter filter = new();
            return Wave(0.12f, 0.4f, t =>
            {
                float tone = Sine(Lerp(900f, 420f, t / 0.12f), t) * MathF.Exp(-t * 35f);
                float air = filter.Next(Noise(random), 3000f) * MathF.Exp(-t * 80f);
                return ((tone * 0.7f) + (air * 0.3f)) * Clamp01(t * 2000f);
            });
        }

        public static float[] Hit()
        {
            return Wave(0.35f, 0.5f, t => ((Sine(160f, t) * MathF.Exp(-t * 30f)) + (Bell(1174.66f, t, 14f) * 0.5f)) * Clamp01(t * 2000f));
        }

        public static float[] Hurt()
        {
            Random random = new(11);
            SweepFilter filter = new();
            return Wave(0.4f, 0.55f, t =>
            {
                float thump = Sine(Lerp(110f, 60f, t / 0.4f), t) * MathF.Exp(-t * 9f);
                float grit = filter.Next(Noise(random), 600f) * MathF.Exp(-t * 18f);
                return (thump + (grit * 0.6f)) * Clamp01(t * 1500f);
            });
        }

        public static float[] Charge()
        {
            return Wave(2f, 0.6f, t => (Sine(220f, t) * 0.4f) + (Sine(330f, t) * 0.25f) + (Sine(440f, t) * 0.15f) + (Sine(441f, t) * 0.15f));
        }

        public static float[] Tv()
        {
            Random random = new(53);
            SweepFilter voice = new();
            SweepFilter hiss = new();
            int syllable = Count(0.16f);
            float level = 0f;
            float targetLevel = 0f;
            float cutoff = 600f;
            float targetCutoff = 600f;
            float[] raw = new float[Count(TvSeconds + TvLoopFade)];
            for (int i = 0; i < raw.Length; i++)
            {
                if (i % syllable == 0)
                {
                    bool pause = random.NextDouble() < 0.25;
                    targetLevel = pause ? 0.05f : 0.4f + (0.6f * (float)random.NextDouble());
                    targetCutoff = 400f + (500f * (float)random.NextDouble());
                }

                level += (targetLevel - level) * 0.0015f;
                cutoff += (targetCutoff - cutoff) * 0.001f;
                raw[i] = (voice.Next(Noise(random), cutoff) * level) + (hiss.Next(Noise(random), 4000f) * 0.03f);
            }

            return Normalize(Loop(raw, Count(TvLoopFade)), 0.3f);
        }

        public static float[] DefaultStep(int variant)
        {
            Random random = new(variant + 31);
            SweepFilter filter = new();
            return Wave(0.15f, 0f, t =>
            {
                float click = filter.Next(Noise(random), 2500f) * MathF.Exp(-t * 60f);
                float thump = Sine(140f, t) * MathF.Exp(-t * 40f);
                return ((click * 0.6f) + thump) * Clamp01(t * 2000f) * 0.45f;
            });
        }

        public static float[] MazeStep(int variant)
        {
            Random random = new(variant + 1);
            return Wave(0.18f, 0f, t => ((Noise(random) * 0.5f) + Sine(90f, t)) * MathF.Exp(-t * 35f) * Clamp01(t * 2000f) * 0.5f);
        }

        public static float[] Engine()
        {
            return Wave(2f, 0.5f, t =>
            {
                float throb = 0.85f + (0.15f * Sine(8f, t));
                return ((Sine(55f, t) * 0.5f) + (Sine(110f, t) * 0.3f) + (Sine(165f, t) * 0.15f) + (Sine(220f, t) * 0.05f)) * throb;
            });
        }

        public static float[] HubMusic()
        {
            float[] samples = new float[Count(MusicSeconds)];
            AddPad(samples, HubChords, PadDetune, 0.18f, 0.1f);
            AddBells(samples, HubBells, 0.14f, 1.4f, 5);
            AddAir(samples, 0.05f, 9);
            return Normalize(samples, 0.5f);
        }

        public static float[] MazeMusic()
        {
            float[] samples = new float[Count(MusicSeconds)];
            AddPad(samples, MazeChords, HorrorDetune, 0.16f, 0.04f);
            AddPad(samples, MazeTension, PadDetune, 0.025f, 0f);
            AddPulse(samples, MazeBass, 105f, 0.3f);
            AddBells(samples, MazeBells, 0.06f, 2f, 43);
            AddAir(samples, 0.05f, 47);
            return Normalize(samples, 0.5f);
        }

        public static float[] AnimalsMusic()
        {
            float[] samples = Bars(AnimalsTempo, SongBars, out int step);
            AddPad(samples, Repeat(AnimalsChords, SongBars / AnimalsChords.Length), PadDetune, 0.08f, 0.1f);
            float[] shaker = Hat(73);
            for (int bar = 0; bar < SongBars; bar++)
            {
                int chord = bar % AnimalsChords.Length;
                int start = bar * BarSteps * step;
                float root = AnimalsRoots[chord];
                AddNote(samples, start, 0.6f, 0.25f, t => Bass(root, t));
                AddNote(samples, start + (8 * step), 0.6f, 0.18f, t => Bass(root * 1.5f, t));

                float[] tones = AnimalsChords[chord];
                (int Step, int Tone)[] motif = bar % 2 == 0 ? AnimalsMotifA : AnimalsMotifB;
                foreach ((int s, int tone) in motif)
                {
                    float note = tones[tone] * 2f;
                    AddNote(samples, start + (s * step), 0.5f, 0.12f, t => Marimba(note, t));
                }

                if (bar >= SongBars / 4)
                {
                    for (int s = 2; s < BarSteps; s += 4)
                        AddBuffer(samples, start + (s * step), shaker, 0.05f);
                }
            }

            return Normalize(samples, 0.5f);
        }

        public static float[] CarMusic()
        {
            float[] samples = Bars(CarTempo, SongBars, out int step);
            AddPad(samples, Repeat(CarChords, SongBars / CarChords.Length), PadDetune, 0.08f, 0.06f);
            float[] kick = Kick();
            float[] hat = Hat(71);
            for (int bar = 0; bar < SongBars; bar++)
            {
                int chord = bar % CarChords.Length;
                int start = bar * BarSteps * step;
                float bass = CarBass[chord];
                for (int s = 0; s < BarSteps; s += 2)
                {
                    float accent = s % 4 == 0 ? 1f : 0.7f;
                    AddNote(samples, start + (s * step), 0.22f, 0.2f * accent, t => Bass(bass, t));
                }

                if (bar > 0)
                {
                    for (int beat = 0; beat < 4; beat++)
                        AddBuffer(samples, start + (beat * 4 * step), kick, 0.25f);
                }

                if (bar > 3)
                {
                    for (int beat = 0; beat < 4; beat++)
                        AddBuffer(samples, start + (((beat * 4) + 2) * step), hat, 0.07f);
                }

                if (bar >= SongBars / 2)
                {
                    float[] tones = CarArp[chord];
                    for (int s = 0; s < BarSteps; s += 2)
                    {
                        float note = tones[ArpPattern[s / 2]];
                        AddNote(samples, start + (s * step), 0.5f, 0.06f, t => Pluck(note, t));
                    }
                }
            }

            return Normalize(samples, 0.5f);
        }

        public static float[] SurvivorMusic()
        {
            float[] samples = new float[Count(MusicSeconds)];
            AddPad(samples, SurvivorChords, PadDetune, 0.14f, 0.08f);
            AddPulse(samples, SurvivorBass, 120f, 0.25f);
            AddAir(samples, 0.04f, 29);
            return Normalize(samples, 0.5f);
        }

        public static float[] HorrorMusic()
        {
            float[] samples = new float[Count(MusicSeconds)];
            AddPad(samples, HorrorChords, HorrorDetune, 0.2f, 0.05f);
            AddAir(samples, 0.08f, 31);
            return Normalize(samples, 0.45f);
        }

        public static float[] EscapeRoomMusic()
        {
            float[] samples = Bars(EscapeRoomTempo, SongBars, out int step);
            AddPad(samples, Repeat(EscapeRoomChords, SongBars / EscapeRoomChords.Length), PadDetune, 0.08f, 0.04f);
            float[] tick = Tick(1600f);
            float[] tock = Tick(1200f);
            for (int bar = 0; bar < SongBars; bar++)
            {
                int chord = bar % EscapeRoomChords.Length;
                int start = bar * BarSteps * step;
                float root = EscapeRoomRoots[chord];
                AddNote(samples, start, 2.5f, 0.2f, t => Pluck(root, t));
                AddNote(samples, start + (8 * step), 2f, 0.1f, t => Pluck(root * 1.5f, t));

                for (int beat = 0; beat < 4; beat++)
                    AddBuffer(samples, start + (beat * 4 * step), beat % 2 == 0 ? tick : tock, 0.035f);

                (int Step, float Note)[][]? phrase = (bar / EscapeRoomChords.Length) switch
                {
                    1 or 3 => EscapeRoomPhraseA,
                    2 => EscapeRoomPhraseB,
                    _ => null,
                };
                if (phrase == null)
                    continue;

                foreach ((int s, float note) in phrase[chord])
                    AddNote(samples, start + (s * step), 1.6f, 0.13f, t => MusicBox(note, t));
            }

            return Normalize(samples, 0.45f);
        }

        private static float[] Loop(float[] raw, int fade)
        {
            float[] samples = new float[raw.Length - fade];
            for (int i = 0; i < samples.Length; i++)
            {
                if (i >= fade)
                {
                    samples[i] = raw[i];
                    continue;
                }

                float w = (float)i / fade;
                samples[i] = (raw[i] * MathF.Sqrt(w)) + (raw[samples.Length + i] * MathF.Sqrt(1f - w));
            }

            return samples;
        }

        private static float Soft(float frequency, float t, float decay)
        {
            if (t <= 0f)
                return 0f;

            float envelope = MathF.Exp(-t * decay) * Clamp01(t * 160f);
            return envelope * (Sine(frequency, t) + (Sine(frequency * 2f, t) * 0.15f));
        }

        private static float Chime(float t, float spacing, float decay, params float[] notes)
        {
            float sum = 0f;
            for (int i = 0; i < notes.Length; i++)
                sum += Soft(notes[i], t - (i * spacing), decay);
            return sum;
        }

        private static float[] Bars(float bpm, int bars, out int step)
        {
            step = (int)MathF.Round(Rate * 60f / bpm / 4f);
            return new float[step * BarSteps * bars];
        }

        private static float[][] Repeat(float[][] chords, int times)
        {
            float[][] repeated = new float[chords.Length * times][];
            for (int i = 0; i < repeated.Length; i++)
                repeated[i] = chords[i % chords.Length];
            return repeated;
        }

        private static void AddNote(float[] samples, int start, float seconds, float level, Func<float, float> voice)
        {
            int length = Count(seconds);
            for (int i = 0; i < length; i++)
                samples[(start + i) % samples.Length] += voice((float)i / Rate) * level;
        }

        private static void AddBuffer(float[] samples, int start, float[] buffer, float level)
        {
            for (int i = 0; i < buffer.Length; i++)
                samples[(start + i) % samples.Length] += buffer[i] * level;
        }

        private static float Pluck(float frequency, float t)
        {
            float attack = Clamp01(t * 300f);
            return attack * MathF.Exp(-t * 5f) * (Sine(frequency, t) + (Sine(frequency * 2f, t) * 0.5f * MathF.Exp(-t * 4f)) + (Sine(frequency * 3f, t) * 0.25f * MathF.Exp(-t * 8f)));
        }

        private static float Marimba(float frequency, float t)
        {
            float attack = Clamp01(t * 500f);
            return attack * MathF.Exp(-t * 9f) * (Sine(frequency, t) + (Sine(frequency * 4f, t) * 0.3f * MathF.Exp(-t * 30f)));
        }

        private static float MusicBox(float frequency, float t)
        {
            float attack = Clamp01(t * 600f);
            return attack * MathF.Exp(-t * 3.5f) * (Sine(frequency, t) + (Sine(frequency * 2f, t) * 0.25f * MathF.Exp(-t * 6f)) + (Sine(frequency * 3f, t) * 0.1f * MathF.Exp(-t * 10f)));
        }

        private static float Bass(float frequency, float t)
        {
            float attack = Clamp01(t * 200f);
            return attack * MathF.Exp(-t * 5f) * (Sine(frequency, t) + (Sine(frequency * 2f, t) * 0.35f) + (Sine(frequency * 3f, t) * 0.15f));
        }

        private static float[] Kick()
        {
            return Wave(0.3f, 0f, t => MathF.Sin(2f * MathF.PI * ((45f * t) + (75f / 28f * (1f - MathF.Exp(-28f * t))))) * MathF.Exp(-t * 10f) * Clamp01(t * 400f));
        }

        private static float[] Hat(int seed)
        {
            Random random = new(seed);
            SweepFilter filter = new();
            return Wave(0.05f, 0f, t => filter.Next(Noise(random), 7000f) * MathF.Exp(-t * 90f));
        }

        private static float[] Tick(float frequency)
        {
            return Wave(0.04f, 0f, t => Sine(frequency, t) * MathF.Exp(-t * 120f) * Clamp01(t * 2000f));
        }

        private static void AddPulse(float[] samples, float[] notes, float bpm, float level)
        {
            int beat = Count(60f / bpm);
            int beats = samples.Length / beat;
            int perNote = Math.Max(1, beats / notes.Length);
            int length = Count(0.45f);
            for (int b = 0; b < beats; b++)
            {
                float note = notes[(b / perNote) % notes.Length];
                float accent = b % 2 == 0 ? 1f : 0.6f;
                for (int i = 0; i < length; i++)
                {
                    float t = (float)i / Rate;
                    int index = ((b * beat) + i) % samples.Length;
                    samples[index] += Sine(note, t) * MathF.Exp(-t * 7f) * Clamp01(t * 200f) * level * accent;
                }
            }
        }
    }
}
