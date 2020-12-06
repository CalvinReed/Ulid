using System;

namespace CalvinReed
{
    internal class UlidFactory
    {
        private Ulid state;

        [ThreadStatic] private static UlidFactory? instance;

        private UlidFactory() { }

        public static UlidFactory Instance => instance ??= new UlidFactory();

        public Ulid Create()
        {
            var timestamp = Misc.ToTimestamp(DateTime.UtcNow);
            state = timestamp == state.Timestamp
                ? Ulid.Increment(state)
                : Ulid.Create(timestamp);
            return state;
        }
    }
}
