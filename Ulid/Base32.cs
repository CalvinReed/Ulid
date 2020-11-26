using System;
using System.Collections.Generic;
using System.Linq;

namespace CalvinReed
{
    internal static class Base32
    {
        private const int BitsPerByte = 8;
        private const int BitsPerDigit = 5;
        private const int BitDifference = BitsPerByte - BitsPerDigit;
        private const int DataBlockSize = BitsPerDigit;
        private const int DigitBlockSize = BitsPerByte;

        private const string Digits = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        private static readonly IReadOnlyDictionary<char, int> Values;

        static Base32()
        {
            var values = new Dictionary<char, int>();
            for (var i = 0; i < Digits.Length; i++)
            {
                values.Add(Digits[i], i);
            }

            for (var i = 10; i < Digits.Length; i++)
            {
                values.Add(char.ToLowerInvariant(Digits[i]), i);
            }

            values.Add('o', 0);
            values.Add('O', 0);
            values.Add('i', 1);
            values.Add('I', 1);
            values.Add('l', 1);
            values.Add('L', 1);

            Values = values;
        }

        public static void Encode(ReadOnlySpan<byte> data, Span<char> digits)
        {
            var i = data.Length % DataBlockSize;
            var k = EncodeHead(data[..i], digits);
            EncodeTail(data[i..], digits[k..]);
        }

        public static bool TryDecode(ReadOnlySpan<char> digits, Span<byte> data)
        {
            foreach (var digit in digits)
            {
                if (!Values.ContainsKey(digit)) return false;
            }

            var k = digits.Length % DigitBlockSize;
            var m = DecodeHead(digits[..k], data);
            DecodeTail(digits[k..], data[m..]);
            return true;
        }

        private static int EncodeHead(ReadOnlySpan<byte> head, Span<char> encoded)
        {
            if (head.IsEmpty)
            {
                return 0;
            }

            var len = head.Length * BitsPerByte / BitsPerDigit + 1;
            Span<byte> padded = stackalloc byte[DataBlockSize];
            Span<char> output = stackalloc char[DigitBlockSize];
            padded[..^head.Length].Clear();
            head.CopyTo(padded[^head.Length..]);
            EncodeBlock(padded, output);
            output[^len..].CopyTo(encoded);
            return len;
        }

        private static int DecodeHead(ReadOnlySpan<char> head, Span<byte> decoded)
        {
            if (head.IsEmpty)
            {
                return 0;
            }

            var len = head.Length * BitsPerDigit / BitsPerByte + 1;
            Span<char> padded = stackalloc char[DigitBlockSize];
            Span<byte> output = stackalloc byte[DataBlockSize];
            padded[..^head.Length].Fill(Digits[0]);
            head.CopyTo(padded[^head.Length..]);
            DecodeBlock(padded, output);
            output[^len..].CopyTo(decoded);
            return len;
        }

        private static void EncodeTail(ReadOnlySpan<byte> tail, Span<char> encoded)
        {
            while (!tail.IsEmpty)
            {
                EncodeBlock(tail, encoded);
                tail = tail[DataBlockSize..];
                encoded = encoded[DigitBlockSize..];
            }
        }

        private static void DecodeTail(ReadOnlySpan<char> tail, Span<byte> decoded)
        {
            while (!tail.IsEmpty)
            {
                DecodeBlock(tail, decoded);
                tail = tail[DigitBlockSize..];
                decoded = decoded[DataBlockSize..];
            }
        }

        private static void EncodeBlock(ReadOnlySpan<byte> block, Span<char> encoded)
        {
            for (var i = 0; i < DigitBlockSize; i++)
            {
                var start = i * BitsPerDigit / BitsPerByte;
                var offset = i * BitsPerDigit % BitsPerByte;
                encoded[i] = GetDigit(block[start..], offset);
            }
        }

        private static void DecodeBlock(ReadOnlySpan<char> block, Span<byte> decoded)
        {
            decoded[..DataBlockSize].Clear();
            for (var i = 0; i < DigitBlockSize; i++)
            {
                var start = i * BitsPerDigit / BitsPerByte;
                var offset = i * BitsPerDigit % BitsPerByte;
                SetDigit(decoded[start..], offset, block[i]);
            }
        }

        private static char GetDigit(ReadOnlySpan<byte> data, int offset)
        {
            if (offset <= BitDifference)
            {
                var value = data[0] >> (BitDifference - offset);
                return Digits[value & 0x1F];
            }
            else
            {
                var left = data[0] << (offset - BitDifference);
                var right = data[1] >> BitDifference >> (BitsPerByte - offset);
                var value = left | right;
                return Digits[value & 0x1F];
            }
        }

        private static void SetDigit(Span<byte> data, int offset, char digit)
        {
            var value = Values[digit];
            if (offset <= BitDifference)
            {
                var shifted = value << (BitDifference - offset);
                data[0] |= (byte) shifted;
            }
            else
            {
                var left = value >> (offset - BitDifference);
                var right = value << BitDifference << (BitsPerByte - offset);
                data[0] |= (byte) left;
                data[1] |= unchecked((byte) right);
            }
        }
    }
}
