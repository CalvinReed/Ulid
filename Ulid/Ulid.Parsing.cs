using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CalvinReed
{
    [JsonConverter(typeof(UlidJsonConverter))]
    partial struct Ulid
    {
        /// <summary>
        /// Parses the input as a <see cref="Ulid"/>.
        /// </summary>
        /// <param name="str">
        /// The span of characters to be parsed.
        /// </param>
        /// <returns>
        /// The parsed <see cref="Ulid"/>.
        /// </returns>
        /// <exception cref="FormatException">
        /// The input is not valid.
        /// </exception>
        public static Ulid Parse(ReadOnlySpan<char> str)
        {
            return TryParse(str) ?? throw new FormatException($"Input is not a valid {nameof(Ulid)}");
        }

        /// <summary>
        /// Attempts to parse the input as a <see cref="Ulid"/>.
        /// </summary>
        /// <param name="str">
        /// The characters to be parsed.
        /// </param>
        /// <returns>
        /// If input is valid, the parsed <see cref="Ulid"/>.
        /// Otherwise, <c>null</c>.
        /// </returns>
        public static Ulid? TryParse(ReadOnlySpan<char> str)
        {
            Span<byte> data = stackalloc byte[BinarySize + 1];
            var success =
                str.Length == Base32Length      // Check length,
                && Base32.TryDecode(str, data)  // then check chars,
                && data[0] == 0;                // then check for overflow.
            return success ? new Ulid(data[1..]) : (Ulid?) null;
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
    }
}
