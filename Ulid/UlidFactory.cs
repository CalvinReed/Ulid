using System;
using System.Security.Cryptography;

namespace CalvinReed
{
    internal class UlidFactory
    {
        private Ulid state;

        [ThreadStatic] private static UlidFactory? instance;

        private UlidFactory() { }

        public static UlidFactory Instance => instance ??= new UlidFactory();

        public Ulid Next()
        {
            var timestamp = Misc.ToTimestamp(DateTime.UtcNow);
            state = timestamp == Misc.ToTimestamp(state)
                ? Increment(state)
                : Create(timestamp);
            return state;
        }

        internal static Ulid Create(long timestamp)
        {
            Span<byte> data = stackalloc byte[Ulid.BinarySize];
            Misc.WriteInt(data, (ulong) timestamp << Ulid.TimestampGap);
            RandomNumberGenerator.Fill(data[6..]);
            return new Ulid(data);
        }

        private static Ulid Increment(Ulid ulid)
        {
            if (ulid.N1 < ulong.MaxValue)
            {
                return new Ulid(ulid.N0, ulid.N1 + 1);
            }

            if (unchecked((ushort) ulid.N0) < ushort.MaxValue)
            {
                return new Ulid(ulid.N0 + 1, 0);
            }

            throw new OverflowException();
        }
    }
}
