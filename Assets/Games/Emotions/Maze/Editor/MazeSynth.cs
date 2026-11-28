using System;
using static GameHub.Common.Editor.ProceduralAudio;

namespace GameHub.Maze.Editor
{
    public static class MazeSynth
    {
        public const int StepVariants = 4;

        private const float AmbientSeconds = 32f;

        private static readonly float[][] PadChords =
        {
            new[] { 110f, 164.81f, 220f, 261.63f, 329.63f },
            new[] { 87.31f, 130.81f, 220f, 261.63f, 329.63f },
            new[] { 130.81f, 196f, 261.63f, 329.63f, 392f },
            new[] { 98f, 146.83f, 196f, 246.94f, 293.66f },
        };

        private static readonly float[] PadDetune = { 0.998f, 1.002f };
        private static readonly float[] BellNotes = { 440f, 523.25f, 587.33f, 659.25f, 783.99f, 880f };

        public static float[] Ambient()
        {
            float[] samples = new float[Count(AmbientSeconds)];
            AddPad(samples, PadChords, PadDetune, 0.2f, 0.12f);
            AddBells(samples, BellNotes, 0.18f, 1.6f, 3);
            AddAir(samples, 0.06f, 7);
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
    }
}
