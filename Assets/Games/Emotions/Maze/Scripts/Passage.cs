using UnityEngine;

namespace GameHub.Maze
{
    public enum PassageKind
    {
        Open,
        ClosesBehind,
        OneWay,
    }

    public sealed class Passage
    {
        public Passage(Vector2Int from, Direction side, EmotionInfo emotion, PassageKind kind)
        {
            From = from;
            Side = side;
            To = from + MazeGrid.OffsetOf(side);
            Emotion = emotion;
            Kind = kind;
        }

        public Vector2Int From { get; }

        public Vector2Int To { get; }

        public Direction Side { get; }

        public EmotionInfo Emotion { get; internal set; }

        public PassageKind Kind { get; }

        public bool IsOpen { get; private set; }

        public Vector2Int OpenedFrom { get; private set; }

        public bool RelocksBehind => Kind != PassageKind.Open;

        public bool CanTraverseFrom(Vector2Int room)
        {
            return Kind != PassageKind.OneWay || room == From;
        }

        public Vector2Int Other(Vector2Int room)
        {
            return room == From ? To : From;
        }

        public Direction SideFrom(Vector2Int room)
        {
            return room == From ? Side : MazeGrid.Opposite(Side);
        }

        public void Open(Vector2Int room)
        {
            IsOpen = true;
            OpenedFrom = room;
        }

        public void Close()
        {
            IsOpen = false;
        }
    }
}
