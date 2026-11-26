using UnityEngine;

namespace GameHub.RhythmGame.Editor
{
    public sealed class SongDraft
    {
        public AudioClip? Clip { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        public float Bpm { get; set; }

        public float FirstBeat { get; set; }

        public float PreviewStart { get; set; }
    }
}
