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
}
