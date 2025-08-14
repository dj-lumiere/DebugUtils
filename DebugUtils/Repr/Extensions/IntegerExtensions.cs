using System.Numerics;
using System.Text;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Extensions;

internal static class IntegerExtensions
{
    public static string FormatAsBinary(this object obj)
    {
        var size = obj.GetByteSize();
        var bytes = obj.GetBytes();
        var isNegative = false;
        if ((obj.IsSignedPrimitive() || obj is BigInteger) && bytes[^1] >= 0x80)
        {
            isNegative = true;
            FilpBytes(in bytes, size);
        }

        var result = new StringBuilder();
        if (isNegative)
        {
            result.Append('-');
        }
        var gotFirstNonZeroBit = false;

        // Process each byte into 8 bits
        for (var i = size - 1; i >= 0; i -= 1)
        {
            for (var bit = 7; bit >= 0; bit -= 1)
            {
                var bitValue = (bytes[i] >> bit) & 1;
                if (!gotFirstNonZeroBit && bitValue == 0)
                    continue;

                gotFirstNonZeroBit = true;
                result.Append(bitValue);
            }
        }

        return gotFirstNonZeroBit
            ? result.ToString()
            : "0";
    }
    public static string FormatAsHex(this object obj)
    {
        var size = obj.GetByteSize();
        var bytes = obj.GetBytes();
        var isNegative = false;
        if ((obj.IsSignedPrimitive() || obj is BigInteger) && bytes[^1] >= 0x80)
        {
            isNegative = true;
            FilpBytes(in bytes, size);
        }

        var result = new StringBuilder();
        if (isNegative)
        {
            result.Append('-');
        }

        var gotFirstNonZeroHex = false;

        // Process bits in groups of 2 for quaternary
        var bitArray = new List<int>();
        for (var i = 0; i < size; i += 1)
        {
            for (var bit = 0; bit < 8; bit += 1)
            {
                bitArray.Add((bytes[i] >> bit) & 1);
            }
        }

        // Pad to multiple of 2 bits if needed
        while (bitArray.Count % 4 != 0)
        {
            bitArray.Add(0);
        }

        // Process from most significant bits
        for (var i = bitArray.Count - 4; i >= 0; i -= 4)
        {
            var hexValue = bitArray[i + 3] * 8 + bitArray[i + 2] * 4 + bitArray[i + 1] * 2 +
                           bitArray[i];
            if (!gotFirstNonZeroHex && hexValue == 0)
                continue;

            gotFirstNonZeroHex = true;
            if (hexValue < 10)
            {
                result.Append((char)(hexValue + '0'));
            }
            else
            {
                result.Append((char)(hexValue - 10 + 'A')); // 10 is 'A'
            }
        }

        return gotFirstNonZeroHex
            ? result.ToString()
            : "0";
    }

    public static string FormatAsOctal(this object obj)
    {
        var size = obj.GetByteSize();
        var bytes = obj.GetBytes();
        var isNegative = false;
        if ((obj.IsSignedPrimitive() || obj is BigInteger) && bytes[^1] >= 0x80)
        {
            isNegative = true;
            FilpBytes(in bytes, size);
        }

        var result = new StringBuilder();
        if (isNegative)
        {
            result.Append('-');
        }
        var gotFirstNonZeroOctal = false;

        // Process bits in groups of 3 for octal
        var bitArray = new List<int>();
        for (var i = 0; i < size; i += 1)
        {
            for (var bit = 0; bit < 8; bit += 1)
            {
                bitArray.Add((bytes[i] >> bit) & 1);
            }
        }

        // Pad to multiple of 3 bits if needed
        while (bitArray.Count % 3 != 0)
        {
            bitArray.Add(0);
        }

        // Process from most significant bits
        for (var i = bitArray.Count - 3; i >= 0; i -= 3)
        {
            var octalValue = bitArray[i + 2] * 4 + bitArray[i + 1] * 2 + bitArray[i];
            if (!gotFirstNonZeroOctal && octalValue == 0)
                continue;

            gotFirstNonZeroOctal = true;
            result.Append(octalValue);
        }

        return gotFirstNonZeroOctal
            ? result.ToString()
            : "0";
    }

    public static string FormatAsQuaternary(this object obj)
    {
        var size = obj.GetByteSize();
        var bytes = obj.GetBytes();
        var isNegative = false;
        if ((obj.IsSignedPrimitive() || obj is BigInteger) && bytes[^1] >= 0x80)
        {
            isNegative = true;
            FilpBytes(in bytes, size);
        }

        var result = new StringBuilder();
        if (isNegative)
        {
            result.Append('-');
        }
        var gotFirstNonZeroQuaternary = false;

        // Process bits in groups of 2 for quaternary
        var bitArray = new List<int>();
        for (var i = 0; i < size; i += 1)
        {
            for (var bit = 0; bit < 8; bit += 1)
            {
                bitArray.Add((bytes[i] >> bit) & 1);
            }
        }

        // Pad to multiple of 2 bits if needed
        while (bitArray.Count % 2 != 0)
        {
            bitArray.Add(0);
        }

        // Process from most significant bits
        for (var i = bitArray.Count - 2; i >= 0; i -= 2)
        {
            var quaternaryValue = bitArray[i + 1] * 2 + bitArray[i];
            if (!gotFirstNonZeroQuaternary && quaternaryValue == 0)
                continue;

            gotFirstNonZeroQuaternary = true;
            result.Append(quaternaryValue);
        }

        return gotFirstNonZeroQuaternary
            ? result.ToString()
            : "0";
    }

    private static byte[] GetBytes(this object obj)
    {
        return obj switch
        {
            sbyte sb => new[]
            {
                (byte)(sb < 0
                    ? sb + 256
                    : sb)
            },
            byte b => new[] { b }, // BitConverter.GetBytes(byte) doesn't exist
            short s => BitConverter.GetBytes(value: s),
            ushort us => BitConverter.GetBytes(value: us),
            int i => BitConverter.GetBytes(value: i),
            uint ui => BitConverter.GetBytes(value: ui),
            long l => BitConverter.GetBytes(value: l),
            ulong ul => BitConverter.GetBytes(value: ul),
            #if NET7_0_OR_GREATER
            Int128 i128 => i128.GetBytesFromInt128(),
            UInt128 u128 => u128.GetBytesFromUInt128(),
            #endif
            BigInteger bi => bi.ToByteArray(),
            _ => throw new ArgumentException(message: "Invalid type")
        };
    }
    private static long GetByteSize<T>(this T obj)
    {
        return obj switch
        {
            byte or sbyte => 1,
            short or ushort => 2,
            int or uint => 4,
            long or ulong => 8,
            #if NET7_0_OR_GREATER
            Int128 or UInt128 => 16,
            #endif
            BigInteger bi => (bi.GetBitLength() + 7) >> 3, // ceil(log2(bi))
            _ => throw new ArgumentException(message: "Invalid type")
        };
    }

    #if NET7_0_OR_GREATER

    private static byte[] GetBytesFromInt128(this Int128 value)
    {
        var ui128 = (UInt128)value;
        var isNegative = value < 0;
        if (isNegative)
        {
            ui128 = ~ui128 +
                    1; // negating all the bits means subtracting ui128 from 0xFFFF_FFFF_FFFF_FFFF, hence the +1 offset afterward.
        }

        return ui128.GetBytesFromUInt128();
    }
    private static byte[] GetBytesFromUInt128(this UInt128 value)
    {
        var highBytes = (ulong)(value >> 64);
        var lowBytes = (ulong)value;
        var bytes = new byte[16];
        Array.Copy(sourceArray: BitConverter.GetBytes(value: lowBytes), sourceIndex: 0,
            destinationArray: bytes, destinationIndex: 0, length: 8);
        Array.Copy(sourceArray: BitConverter.GetBytes(value: highBytes), sourceIndex: 0,
            destinationArray: bytes, destinationIndex: 8, length: 8);
        return bytes;
    }

    #endif

    private static void FilpBytes(in byte[] bytes, long size)
    {
        for (var i = 0; i < size; i += 1)
        {
            bytes[i] = (byte)~bytes[i];
        }

        var carry = bytes[0] == 0xff;
        bytes[0] += 1;

        for (var i = 1; i < size; i += 1)
        {
            if (!carry)
            {
                break;
            }

            bytes[i] += 1;
            if (bytes[i] != 0) 
            {
                break;
            }
        }
    }

    public static string FormatAsHexWithPadding(this object obj, string formatString)
    {
        var padding = 0;
        if (formatString.Length >= 2 && !Int32.TryParse(s: formatString[1..], result: out padding))
        {
            throw new ArgumentException(message: "Invalid format string");
        }

        var result = obj.FormatAsHex();

        if (formatString.Length < 2)
        {
            return result[0] == '-'
                ? $"-0x{result[1..]}"
                : $"0x{result}";
        }

        return result[0] == '-'
            ? $"-0x{result[1..].PadLeft(totalWidth: padding, paddingChar: '0')}"
            : $"0x{result.PadLeft(totalWidth: padding, paddingChar: '0')}";
    }
    public static string FormatAsQuaternaryWithPadding(this object obj, string formatString)
    {
        var padding = 0;
        if (formatString.Length >= 2 && !Int32.TryParse(s: formatString[1..], result: out padding))
        {
            throw new ArgumentException(message: "Invalid format string");
        }

        var result = obj.FormatAsQuaternary();

        if (formatString.Length < 2)
        {
            return result[0] == '-'
                ? $"-0q{result[1..]}"
                : $"0q{result}";
        }

        return result[0] == '-'
            ? $"-0q{result[1..].PadLeft(totalWidth: padding, paddingChar: '0')}"
            : $"0q{result.PadLeft(totalWidth: padding, paddingChar: '0')}";
    }
    public static string FormatAsOctalWithPadding(this object obj, string formatString)
    {
        var padding = 0;
        if (formatString.Length >= 2 && !Int32.TryParse(s: formatString[1..], result: out padding))
        {
            throw new ArgumentException(message: "Invalid format string");
        }

        var result = obj.FormatAsOctal();

        if (formatString.Length < 2)
        {
            return result[0] == '-'
                ? $"-0o{result[1..]}"
                : $"0o{result}";
        }

        return result[0] == '-'
            ? $"-0o{result[1..].PadLeft(totalWidth: padding, paddingChar: '0')}"
            : $"0o{result.PadLeft(totalWidth: padding, paddingChar: '0')}";
    }

    public static string FormatAsBinaryWithPadding(this object obj, string formatString)
    {
        var padding = 0;
        if (formatString.Length >= 2 && !Int32.TryParse(s: formatString[1..], result: out padding))
        {
            throw new ArgumentException(message: "Invalid format string");
        }

        var result = obj.FormatAsBinary();

        if (formatString.Length < 2)
        {
            return result[0] == '-'
                ? $"-0b{result[1..]}"
                : $"0b{result}";
        }

        return result[0] == '-'
            ? $"-0b{result[1..].PadLeft(totalWidth: padding, paddingChar: '0')}"
            : $"0b{result.PadLeft(totalWidth: padding, paddingChar: '0')}";
    }
}