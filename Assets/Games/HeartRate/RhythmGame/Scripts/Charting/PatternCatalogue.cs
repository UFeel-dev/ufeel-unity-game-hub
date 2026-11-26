using System;
using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class PatternCatalogue
    {
        private const PatternTransforms Mirror = PatternTransforms.Mirror;
        private const PatternTransforms MirrorShift = PatternTransforms.Mirror | PatternTransforms.Shift;
        private const float Any = 99f;

        private readonly List<PatternDefinition> patterns;

        public PatternCatalogue(IEnumerable<PatternDefinition> definitions)
        {
            patterns = new List<PatternDefinition>(definitions);
        }

        public IReadOnlyList<PatternDefinition> Patterns => patterns;

        public static PatternCatalogue CreateDefault()
        {
            return new PatternCatalogue(new[]
            {
                new PatternDefinition("Stair", PatternFamily.Roll, 0, 1f, 0f, 7f, Mirror, "1000 0100 0010 0001"),
                new PatternDefinition("Bounce stair", PatternFamily.Roll, 1, 2f, 1.5f, 9f, Mirror, "1000 0100 0010 0001 0010 0100"),
                new PatternDefinition("Snake", PatternFamily.Roll, 2, 4f, 2.5f, 12f, Mirror, "1000 0010 0100 0001 0010 1000 0001 0100"),
                new PatternDefinition("Broken stair", PatternFamily.Roll, 3, 5f, 3f, 14f, Mirror, "1000 0100 0001 0010 0100 1000 0010 0001"),

                new PatternDefinition("Hand swap", PatternFamily.Stream, 0, 1f, 0f, 4f, MirrorShift, "1000 0001"),
                new PatternDefinition("Split stream", PatternFamily.Stream, 1, 2f, 1.5f, 10f, Mirror, "1000 0010 0100 0001"),
                new PatternDefinition("Cross stream", PatternFamily.Stream, 2, 4f, 3f, 14f, Mirror, "0100 0001 1000 0010 0001 0100 0010 1000"),
                new PatternDefinition("Anchor", PatternFamily.Stream, 3, 5f, 3f, 14f, MirrorShift, "1000 0010 1000 0001 1000 0100"),
                new PatternDefinition("Runningman", PatternFamily.Stream, 4, 6f, 4f, 16f, MirrorShift, "1000 0010 1000 0001"),

                new PatternDefinition("Trill", PatternFamily.Trill, 0, 1f, 0f, 6f, MirrorShift, "0100 0010", 0.8f),
                new PatternDefinition("Split trill", PatternFamily.Trill, 1, 2f, 1f, 8f, MirrorShift, "1000 0010", 0.8f),
                new PatternDefinition("One-hand trill", PatternFamily.Trill, 2, 5f, 2f, 9f, MirrorShift, "1000 0100", 0.6f),
                new PatternDefinition("Jump trill", PatternFamily.Trill, 3, 5f, 3f, 12f, MirrorShift, "1010 0101"),

                new PatternDefinition("Bracket", PatternFamily.Jump, 0, 3f, 0f, 6f, MirrorShift, "1001 0110"),
                new PatternDefinition("Jumpstream", PatternFamily.Jump, 1, 5f, 2.5f, 12f, Mirror, "1010 0001 0100 1001 0110 1000 0010 0101"),
                new PatternDefinition("Hand split", PatternFamily.Jump, 2, 6f, 3f, 13f, Mirror, "1100 0001 0010 0100 0011 1000 0100 0010"),

                new PatternDefinition("Triple", PatternFamily.Hand, 0, 7f, 3f, 14f, Mirror, "1011 0100"),
                new PatternDefinition("Handstream", PatternFamily.Hand, 1, 8f, 4f, 16f, Mirror, "1011 0100 1001 0010 0101 1000 0011 0100"),

                new PatternDefinition("Minijack", PatternFamily.Jack, 0, 8f, 3f, 14f, MirrorShift, "1000 1000 0010 0100 0001 0001 0100 0010", 0.7f),
                new PatternDefinition("Chordjack", PatternFamily.Jack, 1, 9f, 3f, 14f, Mirror, "1100 1010 0110 0101 0011 1001", 0.7f),

                new PatternDefinition("Roll burst", PatternFamily.Burst, 0, 1f, 0f, Any, Mirror, "1000 0100 0010 0001"),
                new PatternDefinition("Trill burst", PatternFamily.Burst, 1, 1f, 0f, Any, MirrorShift, "1000 0010", 0.8f),
                new PatternDefinition("Zig burst", PatternFamily.Burst, 2, 7f, 0f, Any, Mirror, "1000 0001 0100 0010"),
                new PatternDefinition("Split roll burst", PatternFamily.Burst, 3, 9f, 0f, Any, Mirror, "1000 0010 0001 0100"),
            });
        }

        public PatternDefinition? ClosestInFamily(PatternFamily family, int order, float level, float notesPerSecond)
        {
            PatternDefinition? best = null;
            int bestDistance = int.MaxValue;
            foreach (PatternDefinition pattern in patterns)
            {
                if (pattern.Family != family || !pattern.IsUnlocked(level) || !pattern.Fits(notesPerSecond))
                    continue;

                int distance = Math.Abs(pattern.Order - order);
                if (distance < bestDistance)
                {
                    best = pattern;
                    bestDistance = distance;
                }
            }
            return best;
        }
    }
}
