using System.Collections.Generic;
using UnityEngine;

namespace GameHub.RhythmGame
{
    [CreateAssetMenu(fileName = "SongLibrary", menuName = "GameHub/Rhythm Game/Song Library")]
    public sealed class RhythmSongLibrary : ScriptableObject
    {
        [SerializeField] private List<RhythmSong> songs = new();

        public IReadOnlyList<RhythmSong> Songs => songs;
    }
}
