namespace GameHub.RhythmGame
{
    public static class LaneMask
    {
        public const int LaneCount = 4;
        public const int Full = (1 << LaneCount) - 1;

        public static bool Contains(int mask, int lane)
        {
            return (mask & (1 << lane)) != 0;
        }

        public static int Count(int mask)
        {
            int count = 0;
            for (int lane = 0; lane < LaneCount; lane++)
            {
                if (Contains(mask, lane))
                    count++;
            }
            return count;
        }

        public static int Hand(int lane)
        {
            return lane < LaneCount / 2 ? 0 : 1;
        }

        public static int HandMask(int hand)
        {
            const int half = (1 << (LaneCount / 2)) - 1;
            return hand == 0 ? half : half << (LaneCount / 2);
        }

        public static int Mirror(int mask)
        {
            int mirrored = 0;
            for (int lane = 0; lane < LaneCount; lane++)
            {
                if (Contains(mask, lane))
                    mirrored |= 1 << (LaneCount - 1 - lane);
            }
            return mirrored;
        }

        public static int Shift(int mask, int offset)
        {
            offset = ((offset % LaneCount) + LaneCount) % LaneCount;
            return ((mask << offset) | (mask >> (LaneCount - offset))) & Full;
        }

        public static int Transform(int mask, bool mirror, int shift)
        {
            return Shift(mirror ? Mirror(mask) : mask, shift);
        }
    }
}
