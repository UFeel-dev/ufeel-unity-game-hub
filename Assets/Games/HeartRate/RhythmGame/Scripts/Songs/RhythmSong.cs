using UnityEngine;

namespace GameHub.RhythmGame
{
    [CreateAssetMenu(fileName = "Song", menuName = "GameHub/Rhythm Game/Song")]
    public sealed class RhythmSong : ScriptableObject
    {
        [SerializeField] private AudioClip clip;
        [SerializeField] private string title = string.Empty;
        [SerializeField] private string artist = string.Empty;
        [SerializeField][Min(1f)] private float bpm = 120f;
        [SerializeField] private float firstBeatOffset;
        [SerializeField][Min(0f)] private float previewStart;

        public string Id => name;

        public AudioClip Clip => clip;

        public string Title => string.IsNullOrEmpty(title) ? name : title;

        public string Artist => artist;

        public float Bpm => bpm;

        public float FirstBeatOffset => firstBeatOffset;

        public float PreviewStart => previewStart;
    }
}
