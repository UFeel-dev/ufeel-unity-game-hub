using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameHub.RhythmGame
{
    public static class KeyBindings
    {
        public const int SlotCount = 2;

        private const string Prefix = "GameHub.RhythmGame.Key.";

        private static readonly Key[] Defaults =
        {
            Key.D, Key.F, Key.J, Key.K,
            Key.LeftArrow, Key.DownArrow, Key.UpArrow, Key.RightArrow,
        };

        public static Key Get(int lane, int slot)
        {
            return (Key)PlayerPrefs.GetInt(Name(lane, slot), (int)Defaults[(slot * LaneMask.LaneCount) + lane]);
        }

        public static void Set(int lane, int slot, Key key)
        {
            for (int otherSlot = 0; otherSlot < SlotCount; otherSlot++)
            {
                for (int otherLane = 0; otherLane < LaneMask.LaneCount; otherLane++)
                {
                    if ((otherLane != lane || otherSlot != slot) && key != Key.None && Get(otherLane, otherSlot) == key)
                        PlayerPrefs.SetInt(Name(otherLane, otherSlot), (int)Key.None);
                }
            }
            PlayerPrefs.SetInt(Name(lane, slot), (int)key);
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            for (int slot = 0; slot < SlotCount; slot++)
            {
                for (int lane = 0; lane < LaneMask.LaneCount; lane++)
                    PlayerPrefs.DeleteKey(Name(lane, slot));
            }
            PlayerPrefs.Save();
        }

        public static string[] PrimaryLabels()
        {
            string[] labels = new string[LaneMask.LaneCount];
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
                labels[lane] = Label(Get(lane, 0));
            return labels;
        }

        public static string Label(Key key)
        {
            switch (key)
            {
                case Key.None:
                    return string.Empty;
                case Key.LeftArrow:
                    return "LEFT";
                case Key.RightArrow:
                    return "RIGHT";
                case Key.UpArrow:
                    return "UP";
                case Key.DownArrow:
                    return "DOWN";
            }

            Keyboard? keyboard = Keyboard.current;
            string name = keyboard != null ? keyboard[key].displayName : key.ToString();
            return string.IsNullOrEmpty(name) ? key.ToString() : name.ToUpperInvariant();
        }

        private static string Name(int lane, int slot)
        {
            return Prefix + slot.ToString(CultureInfo.InvariantCulture) + "." + lane.ToString(CultureInfo.InvariantCulture);
        }
    }
}
