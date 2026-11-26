using System;

namespace GameHub.RhythmGame
{
    public sealed class PatternDefinition
    {
        private readonly int[] steps;
        private readonly float minNotesPerSecond;
        private readonly float maxNotesPerSecond;
        private readonly PatternTransforms transforms;

        public PatternDefinition(string name, PatternFamily family, int order, float minLevel, float minNotesPerSecond, float maxNotesPerSecond, PatternTransforms transforms, string layout, float weight = 1f)
        {
            Name = name;
            Family = family;
            Order = order;
            MinLevel = minLevel;
            this.minNotesPerSecond = minNotesPerSecond;
            this.maxNotesPerSecond = maxNotesPerSecond;
            this.transforms = transforms;
            Weight = weight;
            steps = Parse(name, layout);

            int chordSteps = 0;
            for (int i = 0; i < steps.Length; i++)
            {
                int size = LaneMask.Count(steps[i]);
                MaxStepSize = Math.Max(MaxStepSize, size);
                if (size > 1)
                    chordSteps++;
                if ((steps[i] & steps[(i + 1) % steps.Length]) != 0 && steps.Length > 1)
                    HasJacks = true;
            }
            ChordRatio = chordSteps / (float)steps.Length;
        }

        public string Name { get; }

        public PatternFamily Family { get; }

        public int Order { get; }

        public float MinLevel { get; }

        public float Weight { get; }

        public int MaxStepSize { get; }

        public float ChordRatio { get; }

        public bool HasJacks { get; }

        public bool CanMirror => (transforms & PatternTransforms.Mirror) != 0;

        public bool CanShift => (transforms & PatternTransforms.Shift) != 0;

        public int Step(int index)
        {
            return steps[index % steps.Length];
        }

        public bool Fits(float notesPerSecond)
        {
            return notesPerSecond >= minNotesPerSecond && notesPerSecond <= maxNotesPerSecond;
        }

        public bool IsUnlocked(float level)
        {
            return level >= MinLevel;
        }

        private static int[] Parse(string name, string layout)
        {
            string[] tokens = layout.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                throw new FormatException($"Pattern '{name}' has no steps.");

            int[] parsed = new int[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
                parsed[i] = ParseStep(name, tokens[i]);
            return parsed;
        }

        private static int ParseStep(string name, string token)
        {
            if (token.Length != LaneMask.LaneCount)
                throw new FormatException($"Pattern '{name}' step '{token}' must have {LaneMask.LaneCount} columns.");

            int mask = 0;
            for (int lane = 0; lane < token.Length; lane++)
            {
                if (token[lane] == '1')
                    mask |= 1 << lane;
                else if (token[lane] != '0')
                    throw new FormatException($"Pattern '{name}' step '{token}' may only contain 0 and 1.");
            }

            if (mask == 0)
                throw new FormatException($"Pattern '{name}' step '{token}' is empty.");
            return mask;
        }
    }
}
