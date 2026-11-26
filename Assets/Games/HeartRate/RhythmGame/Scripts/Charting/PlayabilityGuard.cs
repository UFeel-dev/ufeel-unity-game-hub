using System;

namespace GameHub.RhythmGame
{
    public sealed class PlayabilityGuard
    {
        private static readonly int[] OuterFirst = { 0, LaneMask.LaneCount - 1, 1, LaneMask.LaneCount - 2 };

        private readonly double[] lastNoteTime = new double[LaneMask.LaneCount];
        private readonly double[] holdEnd = new double[LaneMask.LaneCount];
        private readonly double[] reservedUntil = new double[LaneMask.LaneCount];

        private int lastMask;

        public PlayabilityGuard()
        {
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                lastNoteTime[lane] = double.NegativeInfinity;
                holdEnd[lane] = double.NegativeInfinity;
                reservedUntil[lane] = double.NegativeInfinity;
            }
        }

        public static int HoldLane(int mask)
        {
            foreach (int lane in OuterFirst)
            {
                if (LaneMask.Contains(mask, lane))
                    return 1 << lane;
            }
            return 0;
        }

        public PlayabilityGuard Clone()
        {
            PlayabilityGuard clone = new();
            Array.Copy(lastNoteTime, clone.lastNoteTime, lastNoteTime.Length);
            Array.Copy(holdEnd, clone.holdEnd, holdEnd.Length);
            Array.Copy(reservedUntil, clone.reservedUntil, reservedUntil.Length);
            clone.lastMask = lastMask;
            return clone;
        }

        public int Resolve(int mask, double time, double minJack, int maxChord)
        {
            int free = FreeLanes(time, minJack);
            int size = Math.Min(LaneMask.Count(mask), maxChord - ActiveHolds(time));
            if (size <= 0 || free == 0)
                return 0;

            if ((mask & ~free) == 0 && LaneMask.Count(mask) == size)
                return mask;

            for (int option = 0; option < LaneMask.LaneCount * 2; option++)
            {
                int usable = LaneMask.Transform(mask, option >= LaneMask.LaneCount, option % LaneMask.LaneCount) & free;
                if (LaneMask.Count(usable) >= size)
                    return Trim(usable, size);
            }
            return Trim(free, Math.Min(size, LaneMask.Count(free)));
        }

        public void Commit(int mask, int holdMask, double time, double endTime, double holdRelease)
        {
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                if (!LaneMask.Contains(mask, lane))
                    continue;

                bool isHold = LaneMask.Contains(holdMask, lane) && endTime > time;
                lastNoteTime[lane] = isHold ? endTime : time;
                if (isHold)
                {
                    holdEnd[lane] = endTime;
                    reservedUntil[lane] = endTime + holdRelease;
                }
            }
            lastMask = mask;
        }

        private int FreeLanes(double time, double minJackGap)
        {
            int free = 0;
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                if (time >= reservedUntil[lane] && time - lastNoteTime[lane] >= minJackGap)
                    free |= 1 << lane;
            }
            return free;
        }

        private int ActiveHolds(double time)
        {
            int count = 0;
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                if (holdEnd[lane] > time)
                    count++;
            }
            return count;
        }

        private int Trim(int mask, int size)
        {
            int previousHands = 0;
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                if (LaneMask.Contains(lastMask, lane))
                    previousHands |= 1 << LaneMask.Hand(lane);
            }

            for (int lane = LaneMask.LaneCount - 1; lane >= 0 && LaneMask.Count(mask) > size; lane--)
            {
                if (LaneMask.Contains(mask, lane) && (previousHands & (1 << LaneMask.Hand(lane))) != 0)
                    mask &= ~(1 << lane);
            }
            for (int lane = LaneMask.LaneCount - 1; lane >= 0 && LaneMask.Count(mask) > size; lane--)
                mask &= ~(1 << lane);
            return mask;
        }
    }
}
