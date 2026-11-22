namespace GameHub.EscapeRoom
{
    public readonly struct SpeechWordMatch
    {
        public readonly bool Matched;
        public readonly float Score;
        public readonly string SpokenWord;
        public readonly string ExpectedWord;

        public SpeechWordMatch(
            bool matched,
            float score,
            string spokenWord,
            string expectedWord)
        {
            Matched = matched;
            Score = score;
            SpokenWord = spokenWord;
            ExpectedWord = expectedWord;
        }
    }
}
