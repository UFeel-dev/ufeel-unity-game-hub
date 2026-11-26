using System;

namespace GameHub.RhythmGame
{
    public static class HitWindows
    {
        public const double Perfect = 0.035;
        public const double Great = 0.070;
        public const double Good = 0.110;
        public const double Meh = 0.150;
        public const double EarlyRelease = Meh * TailScale;

        private const double TailScale = 1.5;

        public static Judgement? JudgeHead(double offset)
        {
            double distance = Math.Abs(offset);
            if (distance <= Perfect)
                return Judgement.Perfect;
            if (distance <= Great)
                return Judgement.Great;
            if (distance <= Good)
                return Judgement.Good;
            if (distance <= Meh)
                return Judgement.Meh;
            return null;
        }

        public static Judgement JudgeTail(double offset)
        {
            return JudgeHead(offset / TailScale) ?? Judgement.Miss;
        }
    }
}
