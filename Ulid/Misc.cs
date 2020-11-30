using System;

namespace CalvinReed
{
    internal static class Misc
    {
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

        public static void WriteDigits(Ulid ulid, Span<char> digits)
        {
            Span<byte> data = stackalloc byte[Ulid.BinarySize];
            WriteULong(ulid.N0, data);
            WriteULong(ulid.N1, data[sizeof(ulong)..]);
            Base32.Encode(data, digits);
        }

        public static long ToTimestamp(DateTime dateTime)
        {
            if (dateTime < DateTime.UnixEpoch)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, null);
            }

            var timeSpan = dateTime - DateTime.UnixEpoch;
            return timeSpan.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static long ToTimestamp(Ulid ulid)
        {
            return (long) (ulid.N0 >> Ulid.TimestampGap);
        }
    }
}
