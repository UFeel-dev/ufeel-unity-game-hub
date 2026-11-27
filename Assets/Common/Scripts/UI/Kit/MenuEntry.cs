using UnityEngine;

namespace GameHub.Common
{
    public readonly struct MenuEntry
    {
        public MenuEntry(string label, string detail = "", string caption = "", Sprite? icon = null, bool isEnabled = true)
        {
            Label = label;
            Detail = detail;
            Caption = caption;
            Icon = icon;
            IsEnabled = isEnabled;
        }

        public string Label { get; }

        public string Detail { get; }

        public string Caption { get; }

        public Sprite? Icon { get; }

        public bool IsEnabled { get; }
    }
}
