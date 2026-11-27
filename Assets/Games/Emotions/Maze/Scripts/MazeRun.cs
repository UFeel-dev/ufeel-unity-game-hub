namespace GameHub.Maze
{
    public readonly struct MazeRun
    {
        private MazeRun(int levelIndex, int round)
        {
            LevelIndex = levelIndex;
            Round = round;
        }

        public int LevelIndex { get; }

        public int Round { get; }

        public bool IsUnlimited => LevelIndex < 0;

        public static MazeRun Level(int index) => new(index, 0);

        public static MazeRun Unlimited(int round) => new(-1, round);
    }
}
