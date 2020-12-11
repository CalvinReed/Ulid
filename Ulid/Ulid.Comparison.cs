using System;

namespace CalvinReed
{
    partial struct Ulid : IEquatable<Ulid>, IComparable<Ulid>, IComparable
    {
        public override int GetHashCode()
        {
            return (n0 ^ n1).GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is Ulid other && Equals(other);
        }

        public bool Equals(Ulid other)
        {
            return n1 == other.n1 && n0 == other.n0;
        }

        public int CompareTo(Ulid other)
        {
            var comparison = n0.CompareTo(other.n0);
            return comparison != 0 ? comparison : n1.CompareTo(other.n1);
        }

        public int CompareTo(object? obj)
        {
            return obj switch
            {
                null => 1,
                Ulid other => CompareTo(other),
                _ => throw new ArgumentException($"Object must be of type {nameof(Ulid)}")
            };
        }

        public static bool operator ==(Ulid left, Ulid right) => left.Equals(right);
        public static bool operator !=(Ulid left, Ulid right) => !left.Equals(right);
        public static bool operator <(Ulid left, Ulid right) => left.CompareTo(right) < 0;
        public static bool operator >(Ulid left, Ulid right) => left.CompareTo(right) > 0;
        public static bool operator <=(Ulid left, Ulid right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Ulid left, Ulid right) => left.CompareTo(right) >= 0;
    }
}
