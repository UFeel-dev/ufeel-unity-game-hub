using GameHub.Common;
using UnityEngine;

namespace GameHub.Maze
{
    public enum RoomKind
    {
        Normal,
        Start,
        Exit,
        Hint,
        Trap,
        Bonus,
    }

    public static class RoomKindStyle
    {
        private static readonly Color HintColor = new(0.3f, 0.85f, 0.95f, 1f);
        private static readonly Color BonusColor = new(1f, 0.58f, 0.22f, 1f);

        public static Color? ColorOf(RoomKind kind)
        {
            return kind switch
            {
                RoomKind.Start => Theme.TextDim,
                RoomKind.Exit => Theme.Accent,
                RoomKind.Hint => HintColor,
                RoomKind.Trap => Theme.Danger,
                RoomKind.Bonus => BonusColor,
                _ => null,
            };
        }

        public static string LabelOf(RoomKind kind)
        {
            return kind switch
            {
                RoomKind.Start => "Start",
                RoomKind.Exit => "Exit",
                RoomKind.Hint => "Hint: shows the way out",
                RoomKind.Trap => "Trap: back to start",
                RoomKind.Bonus => "Bonus: reveals the map",
                _ => string.Empty,
            };
        }
    }
}
