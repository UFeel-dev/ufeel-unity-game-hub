using UnityEngine;

namespace GameHub.RhythmGame
{
    public static class SongPreferences
    {
        private const string Prefix = "GameHub.RhythmGame.Song.";

        public static float Base(RhythmSong song)
        {
            return Mathf.Clamp(PlayerPrefs.GetFloat(Prefix + song.Id + ".Base", DifficultySettings.BaseDefault), DifficultySettings.BaseMin, DifficultySettings.BaseMax);
        }

        public static float Scaling(RhythmSong song)
        {
            return Mathf.Clamp(PlayerPrefs.GetFloat(Prefix + song.Id + ".Scaling", DifficultySettings.ScalingDefault), DifficultySettings.ScalingMin, DifficultySettings.ScalingMax);
        }

        public static void Save(RhythmSong song, float baseLevel, float scaling)
        {
            PlayerPrefs.SetFloat(Prefix + song.Id + ".Base", baseLevel);
            PlayerPrefs.SetFloat(Prefix + song.Id + ".Scaling", scaling);
        }
    }
}
