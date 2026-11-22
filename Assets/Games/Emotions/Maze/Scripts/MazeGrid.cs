using UnityEngine;

namespace GameHub.Maze
{
    public static class MazeGrid
    {
        public const int Size = 3;

        public static readonly Vector2Int Exit = new(Size - 1, Size - 1);

        public static bool IsInside(Vector2Int coordinates)
        {
            return coordinates.x >= 0 && coordinates.x < Size && coordinates.y >= 0 && coordinates.y < Size;
        }

        public static Vector2Int OffsetOf(Direction direction)
        {
            return direction switch
            {
                Direction.North => Vector2Int.up,
                Direction.South => Vector2Int.down,
                Direction.East => Vector2Int.right,
                _ => Vector2Int.left,
            };
        }
    }
}
