using System;

namespace CalvinReed;

internal static class Misc
{
    public static long ToTimestamp(DateTime dateTime)
    {
        if (dateTime < DateTime.UnixEpoch)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, null);
        }

        var timeSpan = dateTime - DateTime.UnixEpoch;
        return timeSpan.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public static ulong ReadULong(ReadOnlySpan<byte> data)
    {
        Span<byte> cut = stackalloc byte[sizeof(ulong)];
        data[..sizeof(ulong)].CopyTo(cut);
        if (BitConverter.IsLittleEndian)
        {
            cut.Reverse();
        }

        return BitConverter.ToUInt64(cut);
    }

    public static void WriteULong(ulong n, Span<byte> data)
    {
        BitConverter.TryWriteBytes(data, n);
        if (BitConverter.IsLittleEndian)
        {
            data[..sizeof(ulong)].Reverse();
        }
    }
}
