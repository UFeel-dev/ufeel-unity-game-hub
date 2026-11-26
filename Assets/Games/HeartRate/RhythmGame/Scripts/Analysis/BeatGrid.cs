namespace GameHub.RhythmGame
{
    public static class BeatGrid
    {
        public const int SlotsPerBeat = 24;
        public const int BeatsPerMeasure = 4;
        public const int SlotsPerMeasure = SlotsPerBeat * BeatsPerMeasure;

        private static readonly int[] Divisors = { 1, 2, 3, 4, 6, 8 };

        public static int DivisorOf(int slot)
        {
            int inBeat = ((slot % SlotsPerBeat) + SlotsPerBeat) % SlotsPerBeat;
            foreach (int divisor in Divisors)
            {
                if ((inBeat * divisor) % SlotsPerBeat == 0)
                    return divisor;
            }
            return SlotsPerBeat;
        }

        public static bool IsTernary(int divisor)
        {
            return divisor is 3 or 6;
        }

        public static bool IsBinary(int divisor)
        {
            return divisor is 2 or 4 or 8;
        }

        public static bool Fits(int divisor, bool ternary)
        {
            return divisor == 1 || (ternary ? IsTernary(divisor) : IsBinary(divisor));
        }

        public static float MetricWeight(int slot)
        {
            int inMeasure = ((slot % SlotsPerMeasure) + SlotsPerMeasure) % SlotsPerMeasure;
            if (inMeasure == 0)
                return 1f;
            if (inMeasure == SlotsPerMeasure / 2)
                return 0.85f;

            return DivisorOf(slot) switch
            {
                1 => 0.75f,
                2 => 0.55f,
                3 or 4 => 0.4f,
                6 or 8 => 0.25f,
                _ => 0f,
            };
        }
    }
}
