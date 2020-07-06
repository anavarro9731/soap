namespace Soap.Utility.Objects.Blended
{
    using System;
    using Soap.Utility.Functions.Operations;

    public readonly struct Minimum5CharacterString : IEquatable<Minimum5CharacterString>
    {
        public bool Equals(Minimum5CharacterString other) => this.s == other.s;

        public override bool Equals(object obj) => obj is Minimum5CharacterString other && Equals(other);

        public override int GetHashCode() => this.s != null ? this.s.GetHashCode() : 0;

        public static bool operator ==(Minimum5CharacterString left, Minimum5CharacterString right) => left.Equals(right);

        public static bool operator !=(Minimum5CharacterString left, Minimum5CharacterString right) => !left.Equals(right);

        private readonly string s;

        public Minimum5CharacterString(string s)
        {
            Guard.Against(s.Length <= 5, "String must be at least 5 chars");
            this.s = new string(s);
        }

        public static implicit operator Minimum5CharacterString(string input) => new Minimum5CharacterString(input);
    }
}