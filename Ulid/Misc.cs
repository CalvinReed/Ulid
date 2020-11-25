using System;

namespace CalvinReed
{
    internal static class Misc
    {
        public static ulong ReadInt(ReadOnlySpan<byte> data)
        {
            Span<byte> cut = stackalloc byte[sizeof(ulong)];
            data[..sizeof(ulong)].CopyTo(cut);
            if (BitConverter.IsLittleEndian)
            {
                cut.Reverse();
            }

            return BitConverter.ToUInt64(cut);
        }

        public static void WriteInt(Span<byte> data, ulong n)
        {
            BitConverter.TryWriteBytes(data, n);
            if (BitConverter.IsLittleEndian)
            {
                data[..sizeof(ulong)].Reverse();
            }
        }

        public static long ToTimestamp(DateTime dateTime)
        {
            var utc = dateTime.ToUniversalTime();
            if (utc < DateTime.UnixEpoch)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, null);
            }

            var timeSpan = utc - DateTime.UnixEpoch;
            return timeSpan.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static long ToTimestamp(Ulid ulid)
        {
            return (long) (ulid.N0 >> Ulid.TimestampGap);
        }

        public static void WriteDigits(Ulid ulid, Span<char> digits)
        {
            Span<byte> data = stackalloc byte[Ulid.BinarySize];
            WriteInt(data, ulid.N0);
            WriteInt(data[sizeof(ulong)..], ulid.N1);
            Base32.Encode(data, digits);
        }
    }
}
