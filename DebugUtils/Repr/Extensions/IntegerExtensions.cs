using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Extensions;

using System;
using System.Numerics;

internal static class IntegerExtensions
{
    // Digit alphabets per base-2^k
    private static readonly string Digits = "0123456789ABCDEF";

    // Public API (with prefixes and optional zero-padding)

    public static string FormatAsHexWithPadding(this object value, string format)
    {
        var width = ParseOptionalWidth(format: format);
        var bytes = GetMagnitudeBytesLittleEndian(value: value, isNegative: out var isNegative);
        var digits =
            FormatBasePowerOfTwo(littleEndianMagnitude: bytes, bitsPerDigit: 4, digits: Digits);

        return isNegative
            ? "-0x" + digits.PadLeft(totalWidth: width, paddingChar: '0')
            : "0x" + digits.PadLeft(totalWidth: width, paddingChar: '0');
    }

    public static string FormatAsOctalWithPadding(this object value, string format)
    {
        var width = ParseOptionalWidth(format: format);
        var bytes = GetMagnitudeBytesLittleEndian(value: value, isNegative: out var isNegative);
        var digits =
            FormatBasePowerOfTwo(littleEndianMagnitude: bytes, bitsPerDigit: 3, digits: Digits);

        return isNegative
            ? "-0o" + digits.PadLeft(totalWidth: width, paddingChar: '0')
            : "0o" + digits.PadLeft(totalWidth: width, paddingChar: '0');
    }

    public static string FormatAsQuaternaryWithPadding(this object value, string format)
    {
        var width = ParseOptionalWidth(format: format);
        var bytes = GetMagnitudeBytesLittleEndian(value: value, isNegative: out var isNegative);
        var digits =
            FormatBasePowerOfTwo(littleEndianMagnitude: bytes, bitsPerDigit: 2, digits: Digits);

        return isNegative
            ? "-0q" + digits.PadLeft(totalWidth: width, paddingChar: '0')
            : "0q" + digits.PadLeft(totalWidth: width, paddingChar: '0');
    }

    public static string FormatAsBinaryWithPadding(this object value, string format)
    {
        var width = ParseOptionalWidth(format: format);
        var bytes = GetMagnitudeBytesLittleEndian(value: value, isNegative: out var isNegative);
        var digits =
            FormatBasePowerOfTwo(littleEndianMagnitude: bytes, bitsPerDigit: 1, digits: Digits);

        return isNegative
            ? "-0b" + digits.PadLeft(totalWidth: width, paddingChar: '0')
            : "0b" + digits.PadLeft(totalWidth: width, paddingChar: '0');
    }

    // ---------- Helpers ----------

    /// <summary>
    /// Format an unsigned magnitude (little-endian) into base-2^k without building a per-bit list.
    /// Emits the leading partial group (if any) first, then full-size groups.
    /// </summary>
    private static string FormatBasePowerOfTwo(byte[] littleEndianMagnitude, int bitsPerDigit,
        string digits)
    {
        if (littleEndianMagnitude == null || littleEndianMagnitude.Length == 0)
        {
            return "0";
        }

        // Find index of most-significant non-zero byte.
        var mostSignificantIndex = littleEndianMagnitude.Length - 1;
        while (mostSignificantIndex >= 0 && littleEndianMagnitude[mostSignificantIndex] == 0)
        {
            mostSignificantIndex -= 1;
        }

        if (mostSignificantIndex < 0)
        {
            return "0";
        }

        // Find index (0..7) of top set bit in that byte.
        var mostSignificantByte = littleEndianMagnitude[mostSignificantIndex];
        var mostSignificantBitIndex = 7;
        while (mostSignificantBitIndex >= 0 &&
               mostSignificantByte.GetBit(mostSignificantBitIndex) == 0)
        {
            mostSignificantBitIndex -= 1;
        }

        // Total significant bits = full bytes below MSB * 8 + bits in MSB.
        var totalBits = mostSignificantIndex * 8 + mostSignificantBitIndex + 1;
        var maxDigits = (totalBits + bitsPerDigit - 1) / bitsPerDigit;
        var outputBuffer = new char[maxDigits];
        var digitIndex = 0;
        var groupAccumulator = (totalBits - 1) % bitsPerDigit + 1;
        var cache = 0;
        for (var i = totalBits - 1; i >= 0; i -= 1)
        {
            cache <<= 1;
            cache |= littleEndianMagnitude[i >> 3]
               .GetBit(i & 7);
            groupAccumulator -= 1;
            if (groupAccumulator == 0)
            {
                groupAccumulator = bitsPerDigit;
                outputBuffer[digitIndex] = digits[cache];
                digitIndex += 1;
                cache = 0;
            }
        }

        return new string(outputBuffer, 0, digitIndex);
    }

    /// <summary>
    /// Returns the absolute-value magnitude of an integer as little-endian bytes,
    /// and a flag indicating if the original value was negative.
    /// - For signed primitives: converts two's complement to magnitude in-place.
    /// - For BigInteger: uses Abs and unsigned representation (no sign byte).
    /// </summary>
    private static byte[] GetMagnitudeBytesLittleEndian(object value, out bool isNegative)
    {
        isNegative = false;

        switch (value)
        {
            case sbyte v:
                if (v < 0)
                {
                    isNegative = true;
                    return new[] { (byte)-v };
                }

                return new[] { (byte)v };

            case byte v:
                return new[] { v };

            case short v:
            {
                var bytes = BitConverter.GetBytes(value: v);
                if (v < 0)
                {
                    isNegative = true;
                    ConvertTwosComplementToMagnitudeInPlace(bytes: bytes);
                }

                return bytes;
            }

            case ushort v:
                return BitConverter.GetBytes(value: v);

            case int v:
            {
                var bytes = BitConverter.GetBytes(value: v);
                if (v < 0)
                {
                    isNegative = true;
                    ConvertTwosComplementToMagnitudeInPlace(bytes: bytes);
                }

                return bytes;
            }

            case uint v:
                return BitConverter.GetBytes(value: v);

            case long v:
            {
                var bytes = BitConverter.GetBytes(value: v);
                if (v < 0)
                {
                    isNegative = true;
                    ConvertTwosComplementToMagnitudeInPlace(bytes: bytes);
                }

                return bytes;
            }

            case ulong v:
                return BitConverter.GetBytes(value: v);

            #if NET7_0_OR_GREATER
            case Int128 v:
            {
                isNegative = v < 0;
                var unsigned = isNegative ? (UInt128)(-v) : (UInt128)v;
                return GetBytesFromUInt128(unsigned);
            }

            case UInt128 v:
                return GetBytesFromUInt128(v);
            #endif

            case BigInteger big:
            {
                isNegative = big.Sign < 0;
                var magnitude = BigInteger.Abs(value: big);
                #if NET7_0_OR_GREATER
                return magnitude.ToByteArray(isUnsigned: true, isBigEndian: false);
                #else
                // Positive, minimal two's complement little-endian.
                var bytes = magnitude.ToByteArray();
                // Trim possible sign-extension 0x00 at the MSB end (last element).
                if (bytes.Length > 1 && bytes[bytes.Length - 1] == 0x00)
                {
                    Array.Resize(array: ref bytes, newSize: bytes.Length - 1);
                }

                return bytes;
                #endif
            }

            default:
                throw new ArgumentException(message: "Invalid type");
        }
    }

    #if NET7_0_OR_GREATER
    private static byte[] GetBytesFromUInt128(UInt128 value)
    {
        var low = (ulong)value;
        var high = (ulong)(value >> 64);

        var bytes = new byte[16];
        var lowBytes = BitConverter.GetBytes(low);
        var highBytes = BitConverter.GetBytes(high);

        Array.Copy(lowBytes,  0, bytes, 0, 8);
        Array.Copy(highBytes, 0, bytes, 8, 8);
        return bytes;
    }
    #endif

    /// <summary>
    /// Converts a two's-complement little-endian byte array to its magnitude (absolute value) in-place.
    /// </summary>
    private static void ConvertTwosComplementToMagnitudeInPlace(byte[] bytes)
    {
        var i = 0;
        while (i < bytes.Length)
        {
            bytes[i] = (byte)~bytes[i];
            i += 1;
        }

        // Add 1 with carry
        var index = 0;
        var carry = true;
        while (index < bytes.Length && carry)
        {
            var sum = bytes[index] + 1;
            bytes[index] = (byte)sum;
            carry = sum == 0;
            index += 1;
        }
    }

    /// <summary>
    /// Parses an optional width suffix from a format string like "X8" / "O12".
    /// Returns 0 when no width is provided.
    /// </summary>
    private static int ParseOptionalWidth(string format)
    {
        if (String.IsNullOrEmpty(value: format) || format.Length < 2)
        {
            return 0;
        }

        var widthText = format[1..];
        if (!Int32.TryParse(s: widthText, result: out var width))
        {
            throw new ArgumentException(message: "Invalid format string");
        }

        return width < 0
            ? 0
            : width;
    }

    private static int GetBit(this byte b, int bit) => (b >> bit) & 1;
}