using System.Collections.Generic;

namespace GameHub.Common
{
    public readonly struct KeyHint
    {
        public const string Up = "↑";
        public const string Down = "↓";
        public const string Left = "←";
        public const string Right = "→";

        public KeyHint(string label, int action, params string[] keys)
        {
            Label = label;
            Action = action;
            Keys = keys;
        }

        public string Label { get; }

        public int Action { get; }

        public IReadOnlyList<string> Keys { get; }
    }
}
