using System;

namespace GameHub.RhythmGame
{
    public sealed class ScoreKeeper
    {
        private const int JudgementCount = 5;

        private static readonly int[] Points = { 300, 200, 100, 50, 0 };
        private static readonly double[] AccuracyWeights = { 1d, 2d / 3d, 1d / 3d, 1d / 6d, 0d };
        private static readonly float[] HealthChanges = { 0.015f, 0.01f, 0.005f, -0.01f, -0.08f };
        private static readonly int[] ComboTiers = { 25, 50, 100 };

        private readonly int[] counts = new int[JudgementCount];

        private double accuracySum;
        private int judged;

        public long Score { get; private set; }

        public int Combo { get; private set; }

        public int MaxCombo { get; private set; }

        public float Health { get; private set; } = 1f;

        public bool IsDead => Health <= 0f;

        public double Accuracy => judged == 0 ? 1d : accuracySum / judged;

        private int ComboMultiplier
        {
            get
            {
                int multiplier = 1;
                foreach (int tier in ComboTiers)
                {
                    if (Combo >= tier)
                        multiplier++;
                }
                return multiplier;
            }
        }

        public int Count(Judgement judgement)
        {
            return counts[(int)judgement];
        }

        public void Apply(in JudgementEvent judgementEvent)
        {
            int index = (int)judgementEvent.Judgement;
            counts[index]++;
            judged++;
            accuracySum += AccuracyWeights[index];

            if (judgementEvent.Judgement == Judgement.Miss)
            {
                Combo = 0;
            }
            else
            {
                Combo++;
                MaxCombo = Math.Max(MaxCombo, Combo);
                Score += (long)Math.Round(Points[index] * ComboMultiplier * DifficultySettings.ScoreMultiplier(judgementEvent.Level));
            }

            if (judgementEvent.DrainsHealth)
                Health = Math.Clamp(Health + HealthChanges[index], 0f, 1f);
        }
    }
}
