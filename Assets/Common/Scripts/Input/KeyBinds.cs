using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GameHub.Common
{
    public static class KeyBinds
    {
        private const string Prefix = "GameHub.Keys.";

        private static readonly KeyAction[] AllActions = (KeyAction[])Enum.GetValues(typeof(KeyAction));

        private static Key[]? keys;
        private static int version;

        public static int Version => version + KeyLayout.Version;

        public static int Count => AllActions.Length;

        public static bool IsCapturing { get; set; }

        public static string Name(KeyAction action)
        {
            return action switch
            {
                KeyAction.MoveForward => "Move forward",
                KeyAction.MoveBack => "Move back",
                KeyAction.MoveLeft => "Move left",
                KeyAction.MoveRight => "Move right",
                KeyAction.Interact => "Interact",
                KeyAction.Flashlight => "Flashlight",
                KeyAction.FreeMouse => "Free the mouse",
                KeyAction.EyeControl => "Eye control (hub)",
                KeyAction.FastCarousel => "Fast carousel (hub)",
                KeyAction.LevelSelect => "Level select (Maze)",
                KeyAction.Hint => "Hint (Escape Room)",
                KeyAction.RestartListening => "Listen again (Escape Room)",
                KeyAction.EyeDrivingMode => "Eye driving mode (Car)",
                KeyAction.CameraPanel => "Camera panel",
                KeyAction.EmotionCalibration => "Emotion calibration",
                KeyAction.EyeTrackingCalibration => "Eye-tracking calibration",
                _ => "Microphone",
            };
        }

        public static Key Get(KeyAction action)
        {
            return Load()[(int)action];
        }

        public static KeyAction? Set(KeyAction action, Key key)
        {
            Key previous = Get(action);
            KeyAction? swapped = null;
            foreach (KeyAction other in AllActions)
            {
                if (other != action && Get(other) == key)
                {
                    Store(other, previous);
                    swapped = other;
                }
            }

            Store(action, key);
            PlayerPrefs.Save();
            return swapped;
        }

        public static void Reset()
        {
            foreach (KeyAction action in AllActions)
                PlayerPrefs.DeleteKey(Prefix + action);
            PlayerPrefs.Save();
            keys = null;
            version++;
        }

        public static bool IsReserved(Key key)
        {
            return key is Key.None or Key.Escape or Key.Enter or Key.NumpadEnter or Key.Backspace
                or Key.UpArrow or Key.DownArrow or Key.LeftArrow or Key.RightArrow
                or (>= Key.Digit1 and <= Key.Digit0) or (>= Key.Numpad0 and <= Key.Numpad9);
        }

        public static bool Down(KeyAction action)
        {
            return Control(action)?.wasPressedThisFrame == true;
        }

        public static bool Held(KeyAction action)
        {
            return Control(action)?.isPressed == true;
        }

        public static bool Up(KeyAction action)
        {
            return Control(action)?.wasReleasedThisFrame == true;
        }

        public static string Label(KeyAction action)
        {
            return KeyLabel(Get(action));
        }

        public static string KeyLabel(Key key)
        {
            switch (key)
            {
                case Key.None:
                    return string.Empty;
                case Key.LeftArrow:
                    return KeyHint.Left;
                case Key.RightArrow:
                    return KeyHint.Right;
                case Key.UpArrow:
                    return KeyHint.Up;
                case Key.DownArrow:
                    return KeyHint.Down;
            }

            string? name = KeyLayout.Character(key);
            if (string.IsNullOrEmpty(name))
                return key.ToString();
            return name.Length == 1 ? name.ToUpperInvariant() : name;
        }

        private static KeyControl? Control(KeyAction action)
        {
            Keyboard? keyboard = Keyboard.current;
            Key key = Get(action);
            return keyboard == null || key == Key.None || IsCapturing ? null : keyboard[key];
        }

        private static Key Default(KeyAction action)
        {
            return action switch
            {
                KeyAction.MoveForward => Key.W,
                KeyAction.MoveBack => Key.S,
                KeyAction.MoveLeft => Key.A,
                KeyAction.MoveRight => Key.D,
                KeyAction.Interact => Key.E,
                KeyAction.Flashlight => Key.T,
                KeyAction.FreeMouse => Key.Tab,
                KeyAction.EyeControl => Key.N,
                KeyAction.FastCarousel => Key.F,
                KeyAction.LevelSelect => Key.L,
                KeyAction.Hint => Key.H,
                KeyAction.RestartListening => Key.V,
                KeyAction.EyeDrivingMode => Key.G,
                KeyAction.CameraPanel => Key.F1,
                KeyAction.EmotionCalibration => Key.F7,
                KeyAction.EyeTrackingCalibration => Key.F8,
                _ => Key.F9,
            };
        }

        private static Key[] Load()
        {
            if (keys != null)
                return keys;

            keys = new Key[AllActions.Length];
            foreach (KeyAction action in AllActions)
            {
                Key key = (Key)PlayerPrefs.GetInt(Prefix + action, (int)Default(action));
                keys[(int)action] = IsReserved(key) ? Default(action) : key;
            }

            return keys;
        }

        private static void Store(KeyAction action, Key key)
        {
            Load()[(int)action] = key;
            PlayerPrefs.SetInt(Prefix + action, (int)key);
            version++;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            keys = null;
            IsCapturing = false;
        }
    }
}
