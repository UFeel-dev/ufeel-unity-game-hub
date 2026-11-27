using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Maze
{
    public enum RoomShape
    {
        Square,
        Rectangle,
        Circle,
        Triangle,
    }

    public readonly struct RoomLayout
    {
        public RoomLayout(RoomShape shape, Direction orientation)
        {
            Shape = shape;
            Orientation = orientation;
        }

        public RoomShape Shape { get; }

        public Direction Orientation { get; }
    }

    public static class RoomGeometry
    {
        public const float CellHalf = 10f;
        public const float WallHalf = 9.5f;
        public const float NarrowHalf = 6f;

        private const int CircleSegments = 32;

        public static List<Vector2> Outline(RoomLayout layout)
        {
            switch (layout.Shape)
            {
                case RoomShape.Rectangle:
                    bool alongZ = layout.Orientation is Direction.North or Direction.South;
                    return Box(alongZ ? NarrowHalf : WallHalf, alongZ ? WallHalf : NarrowHalf);
                case RoomShape.Circle:
                    List<Vector2> circle = new(CircleSegments);
                    for (int i = 0; i < CircleSegments; i++)
                    {
                        float angle = i * 2f * Mathf.PI / CircleSegments;
                        circle.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * WallHalf);
                    }

                    return circle;
                case RoomShape.Triangle:
                    List<Vector2> triangle = new()
                    {
                        new Vector2(-WallHalf, -WallHalf),
                        new Vector2(WallHalf, -WallHalf),
                        new Vector2(0f, WallHalf),
                    };
                    for (int i = 0; i < triangle.Count; i++)
                        triangle[i] = Rotate(triangle[i], layout.Orientation);
                    return triangle;
                default:
                    return Box(WallHalf, WallHalf);
            }
        }

        public static Vector2 Forward(Direction direction)
        {
            Vector2Int offset = MazeGrid.OffsetOf(direction);
            return new Vector2(offset.x, offset.y);
        }

        public static Vector2 Lateral(Direction direction)
        {
            Vector2 forward = Forward(direction);
            return new Vector2(-forward.y, forward.x);
        }

        public static float DistanceToOutline(List<Vector2> outline, Vector2 origin, Vector2 forward)
        {
            float best = CellHalf;
            for (int i = 0; i < outline.Count; i++)
            {
                Vector2 a = outline[i];
                Vector2 edge = outline[(i + 1) % outline.Count] - a;
                float denominator = Cross(forward, edge);
                if (Mathf.Abs(denominator) < 1e-5f)
                    continue;

                Vector2 toA = a - origin;
                float t = Cross(toA, edge) / denominator;
                float u = Cross(toA, forward) / denominator;
                if (t > 0f && u >= 0f && u <= 1f)
                    best = Mathf.Min(best, t);
            }

            return best;
        }

        public static void CutDoorway(List<(Vector2 A, Vector2 B)> segments, Direction direction, float halfWidth)
        {
            Vector2 forward = Forward(direction);
            Vector2 lateral = Lateral(direction);
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                (Vector2 a, Vector2 b) = segments[i];
                float la = Vector2.Dot(a, lateral);
                float lb = Vector2.Dot(b, lateral);
                if (Mathf.Approximately(la, lb))
                {
                    if (Mathf.Abs(la) < halfWidth && Vector2.Dot((a + b) / 2f, forward) > 0f)
                        segments.RemoveAt(i);
                    continue;
                }

                float t1 = (-halfWidth - la) / (lb - la);
                float t2 = (halfWidth - la) / (lb - la);
                if (t1 > t2)
                    (t1, t2) = (t2, t1);
                t1 = Mathf.Clamp01(t1);
                t2 = Mathf.Clamp01(t2);
                if (t2 - t1 <= 0f || Vector2.Dot(Vector2.Lerp(a, b, (t1 + t2) / 2f), forward) <= 0f)
                    continue;

                segments.RemoveAt(i);
                if (t1 > 0f)
                    segments.Add((a, Vector2.Lerp(a, b, t1)));
                if (t2 < 1f)
                    segments.Add((Vector2.Lerp(a, b, t2), b));
            }
        }

        private static List<Vector2> Box(float halfX, float halfZ)
        {
            return new List<Vector2>
            {
                new(-halfX, -halfZ),
                new(halfX, -halfZ),
                new(halfX, halfZ),
                new(-halfX, halfZ),
            };
        }

        private static Vector2 Rotate(Vector2 point, Direction direction)
        {
            return direction switch
            {
                Direction.East => new Vector2(point.y, -point.x),
                Direction.South => -point,
                Direction.West => new Vector2(-point.y, point.x),
                _ => point,
            };
        }

        private static float Cross(Vector2 a, Vector2 b) => (a.x * b.y) - (a.y * b.x);
    }
}
