using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace CalvinReed.Tests;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class ParsingShould
{
    [Fact]
    public void BeConsistent()
    {
        for (var i = 0; i < 10_000; i++)
        {
            var ulid = Ulid.Create(DateTime.UtcNow);
            var str = ulid.ToString();
            var parsed = Ulid.Parse(str);
            Assert.Equal(ulid, parsed);
            Assert.Equal(str, ulid.ToString());
        }
    }

    [Theory]
    [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAV")]
    [InlineData("00000000PRA1SETHE0MN1SS1AH")]
    [InlineData("00000000000000000000000000")]
    [InlineData("7ZZZZZZZZZZZZZZZZZZZZZZZZZ")]
    public void BeCorrect(string str)
    {
        var ulid = Ulid.TryParse(str.AsSpan());
        Assert.Equal(str, ulid?.ToString());
    }

    [Theory]
    [InlineData("0iARZ3NDeKTSv4RRfFQ69G5FAV")]
    [InlineData("00000000PraiseTheOmnissiah")]
    [InlineData("l000L00O0000000i000o000000")]
    [InlineData("7ZZZzZZZZZZZZZZZZZZZzZZZZZ")]
    public void AcceptAlternateChars(string str)
    {
        var ulid = Ulid.TryParse(str);
        Assert.NotNull(ulid);
    }

    [Theory]
    [MemberData(nameof(GetInvalidLength))]
    public void RejectInvalidLength(string str)
    {
        var ulid = Ulid.TryParse(str);
        Assert.Null(ulid);
    }

    [Theory]
    [MemberData(nameof(GetInvalidChar))]
    public void RejectInvalidChar(string str)
    {
        var ulid = Ulid.TryParse(str);
        Assert.Null(ulid);
    }

    [Theory]
    [MemberData(nameof(GetOverflow))]
    public void RejectOverflow(string str)
    {
        var ulid = Ulid.TryParse(str);
        Assert.Null(ulid);
    }

    [Theory]
    [MemberData(nameof(GetInvalidLength))]
    [MemberData(nameof(GetInvalidChar))]
    [MemberData(nameof(GetOverflow))]
    public void ThrowFormatException(string str)
    {
        Assert.Throws<FormatException>(() => Ulid.Parse(str));
        Assert.Throws<FormatException>(() => Ulid.Parse(str.AsSpan()));
    }

    public static IEnumerable<object[]> GetInvalidLength()
    {
        yield return new object[] {""};
        yield return new object[] {"000000000000000000000000000"};
        yield return new object[] {"0000000000000000000000000"};
    }

    public static IEnumerable<object[]> GetInvalidChar()
    {
        yield return new object[] {"0|ARZ3NDEKTSV4RRFFQ69G5FAV"};
        yield return new object[] {"0|ARZ3NDEKTSV4RRFFQ69G5FAU"};
        yield return new object[] {"01ARZ3NDEKTSV4RRFFQ69G5FAU"};
        yield return new object[] {"8000000000000000000000U000"};
    }

    public static IEnumerable<object[]> GetOverflow()
    {
        yield return new object[] {"80000000000000000000000000"};
        yield return new object[] {"ZZZZZZZZZZZZZZZZZZZZZZZZZZ"};
        yield return new object[] {"PRAISETHEOMNISSIAH00000000"};
    }
}
