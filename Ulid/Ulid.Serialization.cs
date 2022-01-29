using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CalvinReed;

[JsonConverter(typeof(UlidJsonConverter))]
partial struct Ulid
{
    public override string ToString()
    {
        Span<char> digits = stackalloc char[Base32Length];
        WriteDigits(digits);
        return new string(digits);
    }

    internal void WriteDigits(Utf8JsonWriter writer)
    {
        Span<char> digits = stackalloc char[Base32Length];
        WriteDigits(digits);
        writer.WriteStringValue(digits);
    }

    private void WriteDigits(Span<char> digits)
    {
        Span<byte> data = stackalloc byte[BinarySize];
        Misc.WriteULong(n0, data);
        Misc.WriteULong(n1, data[sizeof(ulong)..]);
        Base32.Encode(data, digits);
    }

    /// <inheritdoc cref="Parse(ReadOnlySpan{char})"/>
    public static Ulid Parse(string? input)
    {
        return Parse(input.AsSpan());
    }

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char})"/>
    public static Ulid? TryParse(string? input)
    {
        return TryParse(input.AsSpan());
    }

    /// <summary>
    /// Parses <see cref="input"/> as a <see cref="Ulid"/>.
    /// </summary>
    /// <returns>
    /// The successfully parsed <see cref="Ulid"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// <see cref="input"/> is not a valid <see cref="Ulid"/>.
    /// </exception>
    /// <inheritdoc cref="TryParse(ReadOnlySpan{char})"/>
    public static Ulid Parse(ReadOnlySpan<char> input)
    {
        return TryParse(input) ?? throw new FormatException($"Input is not a valid {nameof(Ulid)}");
    }

    /// <summary>
    /// Attempts to parse <see cref="input"/> as a <see cref="Ulid"/>.
    /// </summary>
    /// <remarks>
    /// Trims leading and trailing whitespace.
    /// </remarks>
    /// <param name="input">
    /// The characters to be parsed.
    /// </param>
    /// <returns>
    /// If <see cref="input"/> is valid, the parsed <see cref="Ulid"/>. Otherwise, <c>null</c>.
    /// </returns>
    public static Ulid? TryParse(ReadOnlySpan<char> input)
    {
        Span<byte> data = stackalloc byte[BinarySize + 1];
        var trimmed = input.Trim();
        var success =
            trimmed.Length == Base32Length     // Check length,
            && Base32.TryDecode(trimmed, data) // then check chars,
            && data[0] == 0;                   // then check for overflow.
        return success ? new Ulid(data[1..]) : (Ulid?) null;
    }
}
