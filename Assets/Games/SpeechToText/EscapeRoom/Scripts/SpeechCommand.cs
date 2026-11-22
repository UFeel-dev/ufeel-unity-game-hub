using System;
using UnityEngine;

namespace GameHub.EscapeRoom
{
    [Serializable]
    public class SpeechCommand
    {
        [SerializeField] private string id;
        [SerializeField] private SpeechConcept[] requiredConcepts;

        public string Id => id;
        public SpeechConcept[] RequiredConcepts => requiredConcepts;

        public SpeechCommand(
            string id,
            SpeechConcept[] requiredConcepts)
        {
            this.id = id;
            this.requiredConcepts = requiredConcepts;
        }
    }
}
