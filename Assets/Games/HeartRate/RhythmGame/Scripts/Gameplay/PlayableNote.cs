namespace GameHub.RhythmGame
{
    public sealed class PlayableNote
    {
        private Judgement? tailResult;

        public PlayableNote(NoteData data)
        {
            Data = data;
        }

        public NoteData Data { get; }

        public int Lane => Data.Lane;

        public double Time => Data.Time;

        public double EndTime => Data.EndTime;

        public bool IsHold => Data.IsHold;

        public Judgement? HeadResult { get; private set; }

        public bool IsHolding { get; private set; }

        public bool IsBroken => HeadResult == Judgement.Miss || tailResult == Judgement.Miss;

        public bool IsDone => IsHold ? tailResult.HasValue : HeadResult.HasValue;

        public bool IsCleared => IsDone && !IsBroken;

        public void JudgeHead(Judgement judgement)
        {
            HeadResult = judgement;
            IsHolding = IsHold && judgement != Judgement.Miss;
        }

        public void JudgeTail(Judgement judgement)
        {
            tailResult = judgement;
            IsHolding = false;
        }
    }
}
