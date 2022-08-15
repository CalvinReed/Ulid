using System;
using System.Buffers.Binary;

namespace CalvinReed;

/// <summary>
/// Represents a universally unique, lexicographically sortable identifier.
/// </summary>
public readonly partial struct Ulid
{
    private const int BinarySize = 16;
    private const int Base32Length = 26;
    private const int TimestampGap = 16;

    private readonly ulong n0; // 6 bytes timestamp, 2 bytes randomness
    private readonly ulong n1; // 8 bytes randomness

    /// <summary>
    /// Constructs a <see cref="Ulid"/> with a blank randomness component.
    /// </summary>
    /// <param name="dateTime">The desired timestamp.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="dateTime"/> predates the Unix epoch.
    /// </exception>
    public Ulid(DateTime dateTime)
    {
        n0 = (ulong) Misc.ToTimestamp(dateTime) << TimestampGap;
        n1 = 0;
    }

    /// <summary>
    /// Constructs a <see cref="Ulid"/> with a blank randomness component.
    /// </summary>
    /// <param name="ulid">A <see cref="Ulid"/> with the desired timestamp.</param>
    public Ulid(Ulid ulid)
    {
        n0 = ulid.n0 & 0xFFFF_FFFF_FFFF_0000;
        n1 = 0;
    }

    private Ulid(ulong n0, ulong n1)
    {
        this.n0 = n0;
        this.n1 = n1;
    }

    private Ulid(ReadOnlySpan<byte> data)
    {
        if (data.Length < BinarySize)
        {
            throw new ArgumentOutOfRangeException(nameof(data), data.Length, "Input not large enough");
        }

        n0 = BinaryPrimitives.ReadUInt64BigEndian(data);
        n1 = BinaryPrimitives.ReadUInt64BigEndian(data[sizeof(ulong)..]);
    }

    /// <summary>
    /// The encoded timestamp as a UTC <see cref="DateTime"/>.
    /// </summary>
    public DateTime UtcTimestamp => DateTime.UnixEpoch.AddTicks(Timestamp * TimeSpan.TicksPerMillisecond);

    internal long Timestamp => (long) (n0 >> TimestampGap);
}
