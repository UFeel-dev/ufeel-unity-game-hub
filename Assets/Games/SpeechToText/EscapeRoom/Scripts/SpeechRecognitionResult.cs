using System.Collections.Generic;

namespace GameHub.EscapeRoom
{
    public sealed class SpeechRecognitionResult
    {
        public bool IsValid { get; }
        public float Score { get; }
        public IReadOnlyList<SpeechConceptMatch> Matches { get; }

        public SpeechRecognitionResult(
            bool isValid,
            float score,
            IReadOnlyList<SpeechConceptMatch> matches)
        {
            IsValid = isValid;
            Score = score;
            Matches = matches;
        }
    }
}
