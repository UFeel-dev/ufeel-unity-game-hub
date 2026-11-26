namespace GameHub.RhythmGame
{
    public readonly struct JudgementEvent
    {
        public JudgementEvent(int lane, Judgement judgement, double offset, bool isTail, bool drainsHealth, float level)
        {
            Lane = lane;
            Judgement = judgement;
            Offset = offset;
            IsTail = isTail;
            DrainsHealth = drainsHealth;
            Level = level;
        }

        public int Lane { get; }

        public Judgement Judgement { get; }

        public double Offset { get; }

        public bool IsTail { get; }

        public bool DrainsHealth { get; }

        public float Level { get; }
    }
}
