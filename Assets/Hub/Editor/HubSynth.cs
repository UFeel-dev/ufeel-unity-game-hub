using System;
using static GameHub.Common.Editor.ProceduralAudio;

namespace GameHub.Hub.Editor
{
    public static class HubSynth
    {
        public const int StepVariants = 4;

        private const float AmbientSeconds = 32f;

        private static readonly float[][] PadChords =
        {
            new[] { 130.81f, 196f, 246.94f, 329.63f, 392f },
            new[] { 110f, 164.81f, 196f, 261.63f, 329.63f },
            new[] { 87.31f, 130.81f, 220f, 329.63f, 440f },
            new[] { 98f, 146.83f, 246.94f, 329.63f, 392f },
        };

        private static readonly float[] PadDetune = { 0.998f, 1.002f };
        private static readonly float[] BellNotes = { 523.25f, 587.33f, 659.25f, 783.99f, 880f, 1046.5f };

        public static float[] Ambient()
        {
            float[] samples = new float[Count(AmbientSeconds)];
            AddPad(samples, PadChords, PadDetune, 0.18f, 0.1f);
            AddBells(samples, BellNotes, 0.14f, 1.4f, 5);
            AddAir(samples, 0.05f, 9);
            return Normalize(samples, 0.5f);
        }

        public static float[] MenuMove()
        {
            return Wave(0.08f, 0.45f, t => Sine(1400f, t) * MathF.Exp(-t * 60f) * Clamp01(t * 3000f));
        }

        public static float[] MenuConfirm() => Wave(0.7f, 0.55f, t => Arpeggio(t, 0.08f, 880f, 1318.5f));

        public static float[] MenuBack() => Wave(0.7f, 0.5f, t => Arpeggio(t, 0.08f, 1318.5f, 880f));

        public static float[] CarouselHum()
        {
            return Wave(2f, 0.5f, t =>
            {
                float wobble = 0.8f + (0.2f * Sine(2f, t));
                return ((Sine(60f, t) * 0.5f) + (Sine(90f, t) * 0.3f) + (Sine(120f, t) * 0.2f) + (Sine(240f, t) * 0.05f)) * wobble;
            });
        }

        public static float[] Step(int variant)
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
    }
}
