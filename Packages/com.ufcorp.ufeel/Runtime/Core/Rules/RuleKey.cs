#nullable enable

using System;

namespace UFeel
{
    public readonly struct RuleKey : IEquatable<RuleKey>
    {
        internal RuleKey(int id)
        {
            Id = id;
        }

        internal int Id { get; }

        public static bool operator ==(RuleKey left, RuleKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RuleKey left, RuleKey right)
        {
            return !left.Equals(right);
        }

        public bool Equals(RuleKey other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is RuleKey other && Equals(other);
        public override int GetHashCode() => Id;
    }
}
