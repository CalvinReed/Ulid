using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CalvinReed
{
    internal class UlidJsonConverter : JsonConverter<Ulid>
    {
        public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            return Ulid.TryParse(str) ?? throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
        {
            Span<char> digits = stackalloc char[Ulid.Base32Length];
            Misc.WriteDigits(value, digits);
            writer.WriteStringValue(digits);
        }
    }
}
