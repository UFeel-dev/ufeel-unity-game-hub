namespace GameHub.RhythmGame
{
    public static class GradeCalculator
    {
        private static readonly double[] Thresholds = { 0.98, 0.94, 0.88, 0.78, 0.65 };
        private static readonly string[] Labels = { "SS", "S", "A", "B", "C", "D", "F" };

        public static Grade From(double accuracy, bool failed)
        {
            if (failed)
                return Grade.F;

            for (int i = 0; i < Thresholds.Length; i++)
            {
                if (accuracy >= Thresholds[i])
                    return (Grade)i;
            }
            return Grade.D;
        }

        public static string Label(Grade grade)
        {
            return Labels[(int)grade];
        }
    }
}
