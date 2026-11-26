using System;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public static class SongRecords
    {
        private const string Prefix = "GameHub.RhythmGame.Best.";

        public static SongRecord Load(RhythmSong song)
        {
            string key = Prefix + song.Id;
            if (!PlayerPrefs.HasKey(key + ".Score"))
                return default;

            return new SongRecord(
                PlayerPrefs.GetInt(key + ".Score"),
                PlayerPrefs.GetFloat(key + ".Accuracy"),
                (Grade)PlayerPrefs.GetInt(key + ".Grade", (int)Grade.D),
                PlayerPrefs.GetFloat(key + ".Base", DifficultySettings.BaseDefault),
                PlayerPrefs.GetFloat(key + ".Scaling", DifficultySettings.ScalingDefault));
        }

        public static bool TrySaveBest(RunResult result)
        {
            if (result.Failed)
                return false;

            int score = (int)Math.Min(result.Score, int.MaxValue);
            SongRecord current = Load(result.Song);
            if (current.HasRecord && current.Score >= score)
                return false;

            string key = Prefix + result.Song.Id;
            PlayerPrefs.SetInt(key + ".Score", score);
            PlayerPrefs.SetFloat(key + ".Accuracy", (float)result.Accuracy);
            PlayerPrefs.SetInt(key + ".Grade", (int)result.Grade);
            PlayerPrefs.SetFloat(key + ".Base", result.BaseLevel);
            PlayerPrefs.SetFloat(key + ".Scaling", result.Scaling);
            PlayerPrefs.Save();
            return true;
        }
    }
}
