using System;

namespace GameHub.RhythmGame
{
    [Flags]
    public enum PatternTransforms
    {
        None = 0,
        Mirror = 1,
        Shift = 1 << 1,
    }
}
