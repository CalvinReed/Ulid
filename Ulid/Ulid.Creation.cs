﻿using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace CalvinReed;

partial struct Ulid
{
    /// <summary>
    /// Monotonically generates a <see cref="Ulid"/> from the current UTC time.
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
    public static Ulid Create()
    {
        return UlidFactory.Instance.Create();
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
    /// <see cref="dateTime"/> predates the Unix epoch.
    /// </exception>
    public static Ulid Create(DateTimeOffset dateTime)
    {
        var timestamp = ToTimestamp(dateTime);
        return Create(timestamp);
    }

    internal static Ulid Increment(Ulid ulid)
    {
        if (ulid.n1 < ulong.MaxValue)
        {
            return new Ulid(ulid.n0, ulid.n1 + 1);
        }

        if (unchecked((ushort) ulid.n0) < ushort.MaxValue)
        {
            return new Ulid(ulid.n0 + 1, 0);
        }

        throw new OverflowException();
    }

    internal static Ulid Create(long timestamp)
    {
        Span<byte> data = stackalloc byte[BinarySize];
        BinaryPrimitives.WriteInt64BigEndian(data, timestamp << TimestampGap);
        RandomNumberGenerator.Fill(data[6..]);
        return new Ulid(data);
    }
}
