using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;

namespace CalvinReed;

internal static class Base32
{
    private const int DataFullLength = 16;
    private const int DigitFullLength = 26;
    private const int DataBlockLength = 5;
    private const int DigitBlockLength = 8;

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
        Debug.Assert(data.Length == DataFullLength);
        Debug.Assert(digits.Length == DigitFullLength);
        digits[0] = Digits[data[0] >> 5];
        digits[1] = Digits[data[0] & 0b11111];
        EncodeBlock(data[01..06], digits[02..10]);
        EncodeBlock(data[06..11], digits[10..18]);
        EncodeBlock(data[11..16], digits[18..26]);
    }

    public static bool TryDecode(ReadOnlySpan<char> digits, Span<byte> data)
    {
        try
        {
            return TryDecodeImpl(digits, data);
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    private static bool TryDecodeImpl(ReadOnlySpan<char> digits, Span<byte> data)
    {
        Debug.Assert(data.Length == DataFullLength);
        if (digits.Length != DigitFullLength)
        {
            return false;
        }

        var firstValue = Values[digits[0]];
        if (firstValue > 0b111)
        {
            return false;
        }

        data.Clear();
        data[0] |= (byte)(firstValue << 5);
        data[0] |= (byte)Values[digits[1]];
        DecodeBlock(digits[02..10], data[01..06]);
        DecodeBlock(digits[10..18], data[06..11]);
        DecodeBlock(digits[18..26], data[11..16]);
        return true;
    }

    private static void EncodeBlock(ReadOnlySpan<byte> data, Span<char> digits)
    {
        Debug.Assert(data.Length == DataBlockLength);
        Debug.Assert(digits.Length == DigitBlockLength);
        var bits = ReadFromBlock(data);
        for (var i = DigitBlockLength - 1; i >= 0; i--)
        {
            digits[i] = Digits[(int)(bits & 0b11111)];
            bits >>= 5;
        }
    }

    private static void DecodeBlock(ReadOnlySpan<char> digits, Span<byte> data)
    {
        Debug.Assert(digits.Length == DigitBlockLength);
        Debug.Assert(data.Length == DataBlockLength);
        var bits = 0UL;
        foreach (var digit in digits)
        {
            bits <<= 5;
            bits |= (byte)Values[digit];
        }

        WriteToBlock(bits, data);
    }

    private static ulong ReadFromBlock(ReadOnlySpan<byte> data)
    {
        Debug.Assert(data.Length == DataBlockLength);
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        bytes.Clear();
        data.CopyTo(bytes[^DataBlockLength..]);
        return BinaryPrimitives.ReadUInt64BigEndian(bytes);
    }

    private static void WriteToBlock(ulong bits, Span<byte> data)
    {
        Debug.Assert(bits <= 0xFF_FFFF_FFFF);
        Debug.Assert(data.Length == DataBlockLength);
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(bytes, bits);
        bytes[^DataBlockLength..].CopyTo(data);
    }
}
