using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CalvinReed.Tests;

public class TimestampShould
{
    [Fact]
    public void RejectPreEpoch()
    {
        var preEpoch = DateTimeOffset.UnixEpoch - TimeSpan.FromMilliseconds(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ulid(preEpoch));
        Assert.Throws<ArgumentOutOfRangeException>(() => Ulid.Create(preEpoch));
    }

    [Fact]
    public async Task BeConsistent()
    {
        var precision = new TimeSpan(TimeSpan.TicksPerMillisecond);
        for (var i = 0; i < 25; i++)
        {
            var now = DateTimeOffset.UtcNow;
            var ulid = Ulid.Create(now);
            Assert.Equal(now.UtcDateTime, ulid.UtcTimestamp.DateTime, precision);
            await Task.Delay(20);
        }
    }

    [Theory]
    [MemberData(nameof(GetDateTimes))]
    public void BeCorrect(DateTimeOffset dateTime)
    {
        var random = Ulid.Create(dateTime);
        var blank = new Ulid(dateTime);
        var cleared = new Ulid(random);
        Assert.Equal(dateTime, random.UtcTimestamp);
        Assert.Equal(dateTime, blank.UtcTimestamp);
        Assert.Equal(blank, cleared);
    }

    public static IEnumerable<object[]> GetDateTimes()
    {
        yield return new object[] {DateTimeOffset.UnixEpoch};
        yield return new object[] {new DateTimeOffset(1998, 02, 24, 18, 05, 00, TimeZoneInfo.Local.BaseUtcOffset)};
        yield return new object[] {new DateTimeOffset(2013, 12, 11, 10, 09, 08, 765, TimeZoneInfo.Utc.BaseUtcOffset)};
        yield return new object[] {new DateTimeOffset(9999, 12, 31, 23, 59, 59, 999, TimeZoneInfo.Utc.BaseUtcOffset)};
    }
}
