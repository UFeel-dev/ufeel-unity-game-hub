namespace GameHub.RhythmGame
{
    public readonly struct MenuEntry
    {
        public MenuEntry(string label, string detail = "", string caption = "")
        {
            Label = label;
            Detail = detail;
            Caption = caption;
        }

        public string Label { get; }

        public string Detail { get; }

        public string Caption { get; }
    }
}
