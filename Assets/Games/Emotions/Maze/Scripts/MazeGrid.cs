using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Maze
{
    public sealed class MazeGrid
    {
        public static readonly Direction[] Directions = { Direction.North, Direction.South, Direction.East, Direction.West };

        private static readonly RoomShape[] Shapes = { RoomShape.Square, RoomShape.Rectangle, RoomShape.Circle, RoomShape.Triangle };

        private readonly MazeLevelDefinition level;
        private readonly IReadOnlyList<EmotionInfo> emotions;
        private readonly Passage?[,,] passages;
        private readonly RoomKind[,] kinds;
        private readonly RoomLayout[,] layouts;
        private readonly List<Passage> all = new();
        private readonly Dictionary<Vector2Int, EmotionInfo> hints = new();
        private int[,] distancesToExit = new int[0, 0];

        private MazeGrid(MazeLevelDefinition level, int size, IReadOnlyList<EmotionInfo> emotions)
        {
            this.level = level;
            this.emotions = emotions;
            Size = size;
            passages = new Passage?[Size, Size, Directions.Length];
            kinds = new RoomKind[Size, Size];
            layouts = new RoomLayout[Size, Size];
        }

        public int Size { get; }

        public Vector2Int Start { get; private set; }

        public Vector2Int Exit { get; private set; }

        public IReadOnlyList<Passage> Passages => all;

        public static MazeGrid Generate(MazeLevelDefinition level, int size, IReadOnlyList<EmotionInfo> emotions)
        {
            MazeGrid grid = new(level, size, emotions);
            grid.CarveTree();
            grid.Start = grid.RandomRoom();
            grid.Exit = grid.Farthest(grid.Start);
            grid.AddLoops();
            grid.VaryEmotions();
            grid.distancesToExit = grid.DistancesTo(grid.Exit);
            grid.PlaceRooms();
            grid.AssignShapes();
            return grid;
        }

        public RoomLayout LayoutOf(Vector2Int room)
        {
            return layouts[room.x, room.y];
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

        public static Direction Opposite(Direction direction)
        {
            return direction switch
            {
                Direction.North => Direction.South,
                Direction.South => Direction.North,
                Direction.East => Direction.West,
                _ => Direction.East,
            };
        }

        public bool IsInside(Vector2Int room)
        {
            return room.x >= 0 && room.x < Size && room.y >= 0 && room.y < Size;
        }

        public Passage? PassageAt(Vector2Int room, Direction direction)
        {
            return passages[room.x, room.y, (int)direction];
        }

        public RoomKind KindOf(Vector2Int room)
        {
            return kinds[room.x, room.y];
        }

        public bool TryGetHint(Vector2Int room, out EmotionInfo emotion)
        {
            return hints.TryGetValue(room, out emotion);
        }

        public List<Vector2Int> PathToExit(Vector2Int room)
        {
            List<Vector2Int> path = new();
            while (room != Exit)
            {
                room += OffsetOf(DirectionToExit(room));
                path.Add(room);
            }

            return path;
        }

        public Direction DirectionToExit(Vector2Int room)
        {
            int distance = distancesToExit[room.x, room.y];
            foreach (Direction direction in Directions)
            {
                Passage? passage = PassageAt(room, direction);
                if (passage == null || !passage.CanTraverseFrom(room))
                    continue;

                Vector2Int next = passage.Other(room);
                if (distancesToExit[next.x, next.y] == distance - 1)
                    return direction;
            }

            throw new System.InvalidOperationException($"No way to the exit from room {room}.");
        }

        private static void Shuffle<T>(List<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }

        private Vector2Int RandomRoom()
        {
            return new Vector2Int(Random.Range(0, Size), Random.Range(0, Size));
        }

        private IEnumerable<Vector2Int> Rooms()
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                    yield return new Vector2Int(x, y);
            }
        }

        private void Connect(Vector2Int from, Direction side, PassageKind kind)
        {
            Passage passage = new(from, side, EmotionInfo.Random(emotions), kind);
            passages[from.x, from.y, (int)side] = passage;
            passages[passage.To.x, passage.To.y, (int)Opposite(side)] = passage;
            all.Add(passage);
        }

        private int DegreeOf(Vector2Int room)
        {
            int degree = 0;
            foreach (Direction direction in Directions)
            {
                if (PassageAt(room, direction) != null)
                    degree++;
            }

            return degree;
        }

        private void CarveTree()
        {
            bool[,] visited = new bool[Size, Size];
            Stack<Vector2Int> stack = new();
            List<Direction> open = new(Directions.Length);
            Vector2Int first = RandomRoom();
            visited[first.x, first.y] = true;
            stack.Push(first);

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();
                open.Clear();
                foreach (Direction direction in Directions)
                {
                    Vector2Int next = current + OffsetOf(direction);
                    if (IsInside(next) && !visited[next.x, next.y])
                        open.Add(direction);
                }

                if (open.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                Direction side = open[Random.Range(0, open.Count)];
                bool closes = Random.value < level.ClosingChance;
                Connect(current, side, closes ? PassageKind.ClosesBehind : PassageKind.Open);
                Vector2Int target = current + OffsetOf(side);
                visited[target.x, target.y] = true;
                stack.Push(target);
            }
        }

        private int[,] DistancesTo(Vector2Int target)
        {
            int[,] distances = new int[Size, Size];
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                    distances[x, y] = -1;
            }

            Queue<Vector2Int> queue = new();
            distances[target.x, target.y] = 0;
            queue.Enqueue(target);
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                foreach (Direction direction in Directions)
                {
                    Passage? passage = PassageAt(current, direction);
                    if (passage == null)
                        continue;

                    Vector2Int previous = passage.Other(current);
                    if (distances[previous.x, previous.y] >= 0 || !passage.CanTraverseFrom(previous))
                        continue;

                    distances[previous.x, previous.y] = distances[current.x, current.y] + 1;
                    queue.Enqueue(previous);
                }
            }

            return distances;
        }

        private Vector2Int Farthest(Vector2Int from)
        {
            int[,] distances = DistancesTo(from);
            Vector2Int farthest = from;
            foreach (Vector2Int room in Rooms())
            {
                if (distances[room.x, room.y] > distances[farthest.x, farthest.y])
                    farthest = room;
            }

            return farthest;
        }

        private void AddLoops()
        {
            List<(Vector2Int Room, Direction Side)> candidates = new();
            foreach (Vector2Int room in Rooms())
            {
                if (room.x < Size - 1 && PassageAt(room, Direction.East) == null)
                    candidates.Add((room, Direction.East));
                if (room.y < Size - 1 && PassageAt(room, Direction.North) == null)
                    candidates.Add((room, Direction.North));
            }

            Shuffle(candidates);
            int count = Mathf.RoundToInt(candidates.Count * level.LoopRatio);
            for (int i = 0; i < count; i++)
            {
                (Vector2Int room, Direction side) = candidates[i];
                if (Random.value >= level.OneWayChance)
                {
                    Connect(room, side, PassageKind.Open);
                    continue;
                }

                if (Random.value < 0.5f)
                    Connect(room, side, PassageKind.OneWay);
                else
                    Connect(room + OffsetOf(side), Opposite(side), PassageKind.OneWay);
            }
        }

        private void VaryEmotions()
        {
            List<Passage> roomPassages = new(Directions.Length);
            foreach (Vector2Int room in Rooms())
            {
                roomPassages.Clear();
                foreach (Direction direction in Directions)
                {
                    Passage? passage = PassageAt(room, direction);
                    if (passage != null)
                        roomPassages.Add(passage);
                }

                if (roomPassages.Count < 2)
                    continue;

                UFeel.EmotionType first = roomPassages[0].Emotion.Emotion;
                if (roomPassages.TrueForAll(passage => passage.Emotion.Emotion == first))
                    roomPassages[Random.Range(0, roomPassages.Count)].Emotion = EmotionInfo.RandomExcept(emotions, first);
            }
        }

        private void PlaceRooms()
        {
            kinds[Start.x, Start.y] = RoomKind.Start;
            kinds[Exit.x, Exit.y] = RoomKind.Exit;

            List<Vector2Int> deadEnds = new();
            foreach (Vector2Int room in Rooms())
            {
                if (kinds[room.x, room.y] == RoomKind.Normal && DegreeOf(room) == 1)
                    deadEnds.Add(room);
            }

            Shuffle(deadEnds);
            int traps = Mathf.Min(deadEnds.Count, level.TrapRooms);
            for (int i = 0; i < traps; i++)
                kinds[deadEnds[i].x, deadEnds[i].y] = RoomKind.Trap;

            List<Vector2Int> free = new();
            foreach (Vector2Int room in Rooms())
            {
                if (kinds[room.x, room.y] == RoomKind.Normal)
                    free.Add(room);
            }

            Shuffle(free);
            int hintCount = Mathf.Min(free.Count, level.HintRooms);
            int bonusCount = Mathf.Min(free.Count - hintCount, level.BonusRooms);
            for (int i = 0; i < hintCount; i++)
            {
                kinds[free[i].x, free[i].y] = RoomKind.Hint;
                hints[free[i]] = EmotionInfo.Random(emotions);
            }

            for (int i = hintCount; i < hintCount + bonusCount; i++)
                kinds[free[i].x, free[i].y] = RoomKind.Bonus;
        }

        private void AssignShapes()
        {
            float total = 0f;
            foreach (RoomShape shape in Shapes)
                total += level.WeightOf(shape);
            if (total <= 0f)
                return;

            List<Direction> closed = new(Directions.Length);
            foreach (Vector2Int room in Rooms())
            {
                RoomShape shape = PickShape(total);
                Direction orientation = Directions[Random.Range(0, Directions.Length)];
                if (shape == RoomShape.Triangle)
                {
                    closed.Clear();
                    foreach (Direction direction in Directions)
                    {
                        if (PassageAt(room, direction) == null)
                            closed.Add(direction);
                    }

                    if (closed.Count == 0)
                        continue;
                    orientation = closed[Random.Range(0, closed.Count)];
                }

                layouts[room.x, room.y] = new RoomLayout(shape, orientation);
            }
        }

        private RoomShape PickShape(float total)
        {
            float pick = Random.value * total;
            foreach (RoomShape shape in Shapes)
            {
                pick -= level.WeightOf(shape);
                if (pick < 0f)
                    return shape;
            }

            return RoomShape.Square;
        }
    }
}
