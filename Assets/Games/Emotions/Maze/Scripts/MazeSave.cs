using UnityEngine;

namespace GameHub.Maze
{
    public static class MazeSave
    {
        private const string UnlockedKey = "GameHub.Maze.UnlockedLevels";
        private const string BestKey = "GameHub.Maze.UnlimitedBest";

        public static int UnlockedLevels => Mathf.Max(1, PlayerPrefs.GetInt(UnlockedKey, 1));

        public static int UnlimitedBest => PlayerPrefs.GetInt(BestKey, 0);

        public static bool IsUnlocked(int levelIndex) => levelIndex < UnlockedLevels;

        public static bool IsCleared(int levelIndex) => levelIndex + 1 < UnlockedLevels;

        public static bool IsUnlimitedUnlocked(int levelCount) => UnlockedLevels > levelCount;

        public static void CompleteLevel(int levelIndex)
        {
            if (levelIndex + 2 <= UnlockedLevels)
                return;

            PlayerPrefs.SetInt(UnlockedKey, levelIndex + 2);
            PlayerPrefs.Save();
        }

        public static void RecordUnlimited(int cleared)
        {
            if (cleared <= UnlimitedBest)
                return;

            PlayerPrefs.SetInt(BestKey, cleared);
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(UnlockedKey);
            PlayerPrefs.DeleteKey(BestKey);
            PlayerPrefs.Save();
        }
    }
}
