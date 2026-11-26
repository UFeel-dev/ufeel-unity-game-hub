namespace GameHub.RhythmGame
{
    public sealed class SeededRandom
    {
        private const ulong Golden = 0x9E3779B97F4A7C15UL;

        private ulong state;

        public SeededRandom(ulong seed)
        {
            state = seed;
        }

        public static ulong Mix(int seed, int a, int b)
        {
            ulong value = ((ulong)(uint)seed << 32) | (uint)a;
            value ^= (uint)b * Golden;
            return Scramble(value + Golden);
        }

        public int Next(int maxExclusive)
        {
            return maxExclusive <= 1 ? 0 : (int)(NextULong() % (ulong)maxExclusive);
        }

        public float NextFloat()
        {
            return (NextULong() >> 40) * (1f / (1 << 24));
        }

        private static ulong Scramble(ulong value)
        {
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }

        private ulong NextULong()
        {
            state += Golden;
            return Scramble(state);
        }
    }
}
