namespace GameHub.EscapeRoom
{
    public readonly struct SpeechConceptMatch
    {
        public readonly string ConceptName;
        public readonly string SpokenWord;
        public readonly string ExpectedWord;
        public readonly float Score;

        public SpeechConceptMatch(
            string conceptName,
            string spokenWord,
            string expectedWord,
            float score)
        {
            ConceptName = conceptName;
            SpokenWord = spokenWord;
            ExpectedWord = expectedWord;
            Score = score;
        }
    }
}
