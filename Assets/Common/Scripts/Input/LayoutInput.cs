using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GameHub.Common
{
    public static class LayoutInput
    {
        private static readonly Dictionary<KeyCode, KeyControl> ResolvedKeys = new();
        private static readonly Dictionary<KeyCode, Key> FixedKeys = new()
        {
            { KeyCode.Space, Key.Space },
            { KeyCode.Return, Key.Enter },
            { KeyCode.KeypadEnter, Key.NumpadEnter },
            { KeyCode.Escape, Key.Escape },
            { KeyCode.Tab, Key.Tab },
            { KeyCode.Backspace, Key.Backspace },
            { KeyCode.Delete, Key.Delete },
            { KeyCode.UpArrow, Key.UpArrow },
            { KeyCode.DownArrow, Key.DownArrow },
            { KeyCode.LeftArrow, Key.LeftArrow },
            { KeyCode.RightArrow, Key.RightArrow },
            { KeyCode.LeftShift, Key.LeftShift },
            { KeyCode.RightShift, Key.RightShift },
            { KeyCode.LeftControl, Key.LeftCtrl },
            { KeyCode.RightControl, Key.RightCtrl },
            { KeyCode.LeftAlt, Key.LeftAlt },
            { KeyCode.RightAlt, Key.RightAlt },
            { KeyCode.F1, Key.F1 },
            { KeyCode.F2, Key.F2 },
            { KeyCode.F3, Key.F3 },
            { KeyCode.F4, Key.F4 },
            { KeyCode.F5, Key.F5 },
            { KeyCode.F6, Key.F6 },
            { KeyCode.F7, Key.F7 },
            { KeyCode.F8, Key.F8 },
            { KeyCode.F9, Key.F9 },
            { KeyCode.F10, Key.F10 },
            { KeyCode.F11, Key.F11 },
            { KeyCode.F12, Key.F12 },
        };

        private static Keyboard cachedKeyboard;
        private static string cachedLayout;

        public static bool GetKey(KeyCode code)
        {
            KeyControl? key = Resolve(code);
            return key != null ? key.isPressed : Input.GetKey(code);
        }

        public static bool GetKeyDown(KeyCode code)
        {
            KeyControl? key = Resolve(code);
            return key != null ? key.wasPressedThisFrame : Input.GetKeyDown(code);
        }

        public static bool GetKeyUp(KeyCode code)
        {
            KeyControl? key = Resolve(code);
            return key != null ? key.wasReleasedThisFrame : Input.GetKeyUp(code);
        }

        public static float GetHorizontalAxis(bool includeArrows = true)
        {
            return includeArrows
                ? GetAxis(Key.A, Key.D, Key.LeftArrow, Key.RightArrow)
                : GetAxis(Key.A, Key.D, Key.A, Key.D);
        }

        public static float GetVerticalAxis()
        {
            return GetAxis(Key.S, Key.W, Key.DownArrow, Key.UpArrow);
        }

        public static Vector2 GetMouseDelta()
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        private static float GetAxis(Key negative, Key positive, Key alternativeNegative, Key alternativePositive)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return 0f;
            float value = 0f;
            if (keyboard[negative].isPressed || keyboard[alternativeNegative].isPressed)
                value--;
            if (keyboard[positive].isPressed || keyboard[alternativePositive].isPressed)
                value++;
            return value;
        }

        private static KeyControl? Resolve(KeyCode code)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return null;
            if (keyboard != cachedKeyboard || keyboard.keyboardLayout != cachedLayout)
            {
                ResolvedKeys.Clear();
                cachedKeyboard = keyboard;
                cachedLayout = keyboard.keyboardLayout;
            }
            if (ResolvedKeys.TryGetValue(code, out KeyControl cached))
                return cached;
            KeyControl? key = Find(keyboard, code);
            if (key == null)
                return null;
            ResolvedKeys[code] = key;
            return key;
        }

        private static KeyControl? Find(Keyboard keyboard, KeyCode code)
        {
            if (code is >= KeyCode.A and <= KeyCode.Z)
            {
                int offset = code - KeyCode.A;
                string letter = ((char)('a' + offset)).ToString();
                return keyboard.FindKeyOnCurrentKeyboardLayout(letter) ?? keyboard[Key.A + offset];
            }
            if (code is >= KeyCode.Alpha1 and <= KeyCode.Alpha9)
                return keyboard[Key.Digit1 + (code - KeyCode.Alpha1)];
            if (code == KeyCode.Alpha0)
                return keyboard[Key.Digit0];
            if (code is >= KeyCode.Keypad0 and <= KeyCode.Keypad9)
                return keyboard[Key.Numpad0 + (code - KeyCode.Keypad0)];
            return FixedKeys.TryGetValue(code, out Key fixedKey) ? keyboard[fixedKey] : null;
        }
    }
}
