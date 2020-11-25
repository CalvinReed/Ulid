using System;
using System.Text.Json.Serialization;

namespace CalvinReed
{
    /// <summary>
    /// Represents a universally unique, lexicographically sortable identifier.
    /// </summary>
    [JsonConverter(typeof(UlidJsonConverter))]
    public readonly struct Ulid : IEquatable<Ulid>, IComparable<Ulid>, IComparable
    {
        internal const int BinarySize = sizeof(ulong) * 2;
        internal const int TimestampGap = 8 * 2;
        internal const int Base32Length = BinarySize * 8 / 5 + 1;

        internal readonly ulong N0; // 6 bytes timestamp, 2 bytes randomness
        internal readonly ulong N1; // 8 bytes randomness

        /// <summary>
        /// Constructs a <see cref="Ulid"/> with a blank randomness component.
        /// </summary>
        /// <param name="dateTime">The desired timestamp.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Input predates the Unix epoch.
        /// </exception>
        public Ulid(DateTime dateTime)
        {
            N0 = (ulong) Misc.ToTimestamp(dateTime) << TimestampGap;
            N1 = 0;
        }

        /// <summary>
        /// Constructs a <see cref="Ulid"/> with a blank randomness component.
        /// </summary>
        /// <param name="ulid">A <see cref="Ulid"/> with the desired timestamp.</param>
        public Ulid(Ulid ulid)
        {
            N0 = ulid.N0 & 0xFFFF_FFFF_FFFF_0000;
            N1 = 0;
        }

        /// <summary>
        /// Constructs an arbitrary <see cref="Ulid"/>.
        /// </summary>
        /// <param name="n0">6 bytes timestamp, 2 bytes randomness</param>
        /// <param name="n1">8 bytes randomness</param>
        public Ulid(ulong n0, ulong n1)
        {
            N0 = n0;
            N1 = n1;
        }

        /// <summary>
        /// Constructs an arbitrary <see cref="Ulid"/>.
        /// </summary>
        /// <remarks>
        /// Only the first 16 bytes are used; any extras are ignored.
        /// </remarks>
        /// <param name="data">The bytes that will make up the <see cref="Ulid"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Input is not large enough to make a <see cref="Ulid"/>.
        /// </exception>
        public Ulid(ReadOnlySpan<byte> data)
        {
            if (data.Length < BinarySize)
            {
                throw new ArgumentOutOfRangeException(nameof(data), data.Length, "Input not large enough");
            }

            N0 = Misc.ReadInt(data);
            N1 = Misc.ReadInt(data[sizeof(ulong)..]);
        }

        /// <summary>
        /// The encoded timestamp as a UTC <see cref="DateTime"/>.
        /// </summary>
        public DateTime UtcTimestamp
        {
            get
            {
                var timestamp = Misc.ToTimestamp(this);
                var ticks = timestamp * TimeSpan.TicksPerMillisecond;
                return DateTime.UnixEpoch.AddTicks(ticks);
            }
        }

        public override string ToString()
        {
            Span<char> digits = stackalloc char[Base32Length];
            Misc.WriteDigits(this, digits);
            return new string(digits);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var i0 = (int) N0;
                var i1 = (int) N1;
                var i2 = (int) (N0 >> 32);
                var i3 = (int) (N1 >> 32);
                return HashCode.Combine(i0, i1, i2, i3);
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Ulid other && Equals(other);
        }

        public bool Equals(Ulid other)
        {
            return N1 == other.N1 && N0 == other.N0;
        }

        public int CompareTo(Ulid other)
        {
            var comparison = N0.CompareTo(other.N0);
            return comparison != 0 ? comparison : N1.CompareTo(other.N1);
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

        /// <summary>
        /// Monotonically generates a <see cref="Ulid"/>.
        /// </summary>
        /// <remarks>
        /// Monotonicity is not enforced between threads.
        /// </remarks>
        /// <returns>
        /// A monotonically generated <see cref="Ulid"/>.
        /// </returns>
        /// <exception cref="OverflowException">
        /// On exceptionally rare occasions, too many generations within a single millisecond will cause an overflow.
        /// </exception>
        public static Ulid Next()
        {
            return UlidFactory.Instance.Next();
        }

        /// <summary>
        /// Randomly generates a <see cref="Ulid"/>.
        /// </summary>
        /// <param name="dateTime">
        /// The timestamp of the result.
        /// </param>
        /// <returns>
        /// A randomly generated <see cref="Ulid"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Input predates the Unix epoch.
        /// </exception>
        public static Ulid Create(DateTime dateTime)
        {
            var timestamp = Misc.ToTimestamp(dateTime);
            return UlidFactory.Create(timestamp);
        }

        /// <summary>
        /// Parses the input as a <see cref="Ulid"/>.
        /// </summary>
        /// <param name="str">
        /// The span of characters to be parsed.
        /// </param>
        /// <returns>
        /// The parsed <see cref="Ulid"/>.
        /// </returns>
        /// <exception cref="FormatException">
        /// The input is not valid.
        /// </exception>
        public static Ulid Parse(ReadOnlySpan<char> str)
        {
            return TryParse(str) ?? throw new FormatException($"Input is not a valid {nameof(Ulid)}");
        }

        /// <summary>
        /// Attempts to parse the input as a <see cref="Ulid"/>.
        /// </summary>
        /// <param name="str">
        /// The characters to be parsed.
        /// </param>
        /// <returns>
        /// If input is valid, the parsed <see cref="Ulid"/>.
        /// Otherwise, <c>null</c>.
        /// </returns>
        public static Ulid? TryParse(ReadOnlySpan<char> str)
        {
            Span<byte> data = stackalloc byte[BinarySize + 1];
            return Base32.TryDecode(str, data) ? new Ulid(data[1..]) : (Ulid?) null;
        }

        public static bool operator ==(Ulid left, Ulid right) => left.Equals(right);
        public static bool operator !=(Ulid left, Ulid right) => !left.Equals(right);
        public static bool operator <(Ulid left, Ulid right) => left.CompareTo(right) < 0;
        public static bool operator >(Ulid left, Ulid right) => left.CompareTo(right) > 0;
        public static bool operator <=(Ulid left, Ulid right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Ulid left, Ulid right) => left.CompareTo(right) >= 0;
    }
}
