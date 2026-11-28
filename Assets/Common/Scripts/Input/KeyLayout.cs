using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GameHub.Common
{
    public static class KeyLayout
    {
        private static readonly Dictionary<Key, string> Characters = new();

        private static bool loaded;

        public static int Version { get; private set; }

        public static string? Character(Key key)
        {
            Load();
            if (Characters.TryGetValue(key, out string character))
                return character;

            return Keyboard.current?[key].displayName;
        }

        public static KeyControl? Find(Keyboard keyboard, char character)
        {
            Load();
            string wanted = char.ToUpperInvariant(character).ToString();
            foreach (KeyValuePair<Key, string> pair in Characters)
            {
                if (pair.Value == wanted)
                    return keyboard[pair.Key];
            }

            return keyboard.FindKeyOnCurrentKeyboardLayout(character.ToString());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            loaded = false;
            Application.focusChanged -= OnFocusChanged;
            Application.focusChanged += OnFocusChanged;
        }

        private static void OnFocusChanged(bool focused)
        {
            if (!focused)
                return;

            loaded = false;
            Version++;
        }

        private static void Load()
        {
            if (loaded)
                return;

            loaded = true;
            Characters.Clear();
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
            LoadX11();
#endif
        }

#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
        private const string X11 = "libX11.so.6";
        private const uint CoreKeyboard = 0x0100;
        private const int KeycodeOffset = 8;

        private static readonly (Key Key, int Code)[] Codes =
        {
            (Key.Digit1, 2), (Key.Digit2, 3), (Key.Digit3, 4), (Key.Digit4, 5), (Key.Digit5, 6),
            (Key.Digit6, 7), (Key.Digit7, 8), (Key.Digit8, 9), (Key.Digit9, 10), (Key.Digit0, 11),
            (Key.Minus, 12), (Key.Equals, 13),
            (Key.Q, 16), (Key.W, 17), (Key.E, 18), (Key.R, 19), (Key.T, 20),
            (Key.Y, 21), (Key.U, 22), (Key.I, 23), (Key.O, 24), (Key.P, 25),
            (Key.LeftBracket, 26), (Key.RightBracket, 27),
            (Key.A, 30), (Key.S, 31), (Key.D, 32), (Key.F, 33), (Key.G, 34),
            (Key.H, 35), (Key.J, 36), (Key.K, 37), (Key.L, 38),
            (Key.Semicolon, 39), (Key.Quote, 40), (Key.Backquote, 41), (Key.Backslash, 43),
            (Key.Z, 44), (Key.X, 45), (Key.C, 46), (Key.V, 47), (Key.B, 48), (Key.N, 49), (Key.M, 50),
            (Key.Comma, 51), (Key.Period, 52), (Key.Slash, 53),
        };

        [DllImport(X11, CallingConvention = CallingConvention.Cdecl)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        private static extern IntPtr XOpenDisplay(IntPtr name);

        [DllImport(X11, CallingConvention = CallingConvention.Cdecl)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        private static extern int XCloseDisplay(IntPtr display);

        [DllImport(X11, CallingConvention = CallingConvention.Cdecl)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        private static extern int XkbGetState(IntPtr display, uint device, byte[] state);

        [DllImport(X11, CallingConvention = CallingConvention.Cdecl)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        private static extern UIntPtr XkbKeycodeToKeysym(IntPtr display, uint keycode, int group, int level);

        private static void LoadX11()
        {
            try
            {
                IntPtr display = XOpenDisplay(IntPtr.Zero);
                if (display == IntPtr.Zero)
                    return;

                byte[] state = new byte[32];
                int group = XkbGetState(display, CoreKeyboard, state) == 0 ? state[0] : 0;
                foreach ((Key key, int code) in Codes)
                {
                    string? character = FromKeysym((ulong)XkbKeycodeToKeysym(display, (uint)(code + KeycodeOffset), group, 0));
                    if (character != null)
                        Characters[key] = character;
                }

                _ = XCloseDisplay(display);
            }
            catch (DllNotFoundException)
            {
                Characters.Clear();
            }
            catch (EntryPointNotFoundException)
            {
                Characters.Clear();
            }
        }

        private static string? FromKeysym(ulong keysym)
        {
            ulong codepoint;
            if (keysym is >= 0x21 and <= 0xFF)
                codepoint = keysym;
            else if ((keysym & 0xFF000000) == 0x01000000)
                codepoint = keysym & 0x00FFFFFF;
            else
                return null;

            return char.ConvertFromUtf32((int)codepoint).ToUpperInvariant();
        }
#endif
    }
}
