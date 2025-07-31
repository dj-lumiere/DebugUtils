using System.ComponentModel;
using System.Numerics;
using System.Text;

namespace DebugUtils.Formatters;

public class IntegerFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig reprConfig, HashSet<int>? visited = null)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        return reprConfig.IntMode switch
        {
            IntReprMode.Binary => obj.FormatAsBinary(),
            IntReprMode.Decimal => obj.ToString()!,
            IntReprMode.Hex => obj.FormatAsHex(),
            IntReprMode.HexBytes => obj.FormatAsHexBytes(),
            _ => throw new InvalidEnumArgumentException("Invalid reprConfig")
        };
    }
}

internal static class IntegerFormatterLogic
{
    public static string FormatAsBinary(this object obj)
    {
#if NET7_0_OR_GREATER
        if (obj is Int128 i128)
        {
            var ui128 = (UInt128)i128;
            var isNegative = i128 < 0;
            if (isNegative)
            {
                ui128 = ~ui128 +
                        1; // negating all the bits means subtracting ui128 from 0xFFFF_FFFF_FFFF_FFFF, hence the +1 offset afterward.
            }

            return isNegative
                ? $"-0b{ui128.FormatUInt128AsBinary()}"
                : $"0b{ui128.FormatUInt128AsBinary()}";
        }

        if (obj is UInt128 u128)
        {
            return $"0b{u128.FormatUInt128AsBinary()}";
        }
#endif
        if (obj.GetType()
            .IsSignedPrimitiveType())
        {
            // cast sbyte, short, int, long to ulong
            // Negative values get sign-extended, filling upper bits with 1s.
            var ul = obj.SignExtendConvertToUlong();
            // Check if the original value was negative by testing the sign bit (bit 63).
            var isNegative = ul >= 0x8000000000000000L;
            // If it is less than 0 in signed representation, then negate using that
            // negative number is implemented using two's complement representation.
            // negating all the bits means subtracting ul from 0xFFFF_FFFF_FFFF_FFFF,
            // hence the +1 offset afterward.
            // it has to be done this way because when the value is long(-2**63) then
            // simple negating would cause integer overflow.
            if (isNegative)
            {
                ul = ~ul + 1;
            }

            return isNegative
                ? $"-0b{ul.FormatUlongAsBinary()}"
                : $"0b{ul.FormatUlongAsBinary()}";
        }

        if (obj.GetType()
            .IsIntegerPrimitiveType())
        {
            // cast byte, ushort, uint, ulong to ulong
            var u = Convert.ToUInt64(obj);
            return $"0b{u.FormatUlongAsBinary()}";
        }

        if (obj is BigInteger bi)
        {
            return bi.FormatBigIntegerAsBinary();
        }

        throw new ArgumentException("Invalid type");
    }
    public static string FormatAsHex(this object obj)
    {
#if NET7_0_OR_GREATER
        if (obj is Int128 i128)
        {
            var ui128 = (UInt128)i128;
            var isNegative = i128 < 0;
            if (isNegative)
            {
                ui128 = ~ui128 +
                        1; // negating all the bits means subtracting ui128 from 0xFFFF_FFFF_FFFF_FFFF, hence the +1 offset afterward.
            }

            return isNegative
                ? $"-0x{ui128.FormatUInt128AsHex()}"
                : $"0x{ui128.FormatUInt128AsHex()}";
        }

        if (obj is UInt128 u128)
        {
            return $"0x{u128.FormatUInt128AsHex()}";
        }
#endif
        if (obj.GetType()
            .IsSignedPrimitiveType())
        {
            // cast sbyte, short, int, long to ulong
            // Negative values get sign-extended, filling upper bits with 1s.
            var ul = obj.SignExtendConvertToUlong();
            // Check if the original value was negative by testing the sign bit (bit 63).
            var isNegative = ul >= 0x8000000000000000L;
            // If it is less than 0 in signed representation, then negate using that
            // negative number is implemented using two's complement representation.
            // negating all the bits means subtracting ul from 0xFFFF_FFFF_FFFF_FFFF,
            // hence the +1 offset afterward.
            // it has to be done this way because when the value is long(-2**63) then
            // simple negating would cause integer overflow.
            if (isNegative)
            {
                ul = ~ul + 1;
            }

            return isNegative
                ? $"-0x{ul.FormatUlongAsHex()}"
                : $"0x{ul.FormatUlongAsHex()}";
        }

        if (obj.GetType()
            .IsIntegerPrimitiveType())
        {
            // cast byte, ushort, uint, ulong to ulong
            var u = Convert.ToUInt64(obj);
            return $"0x{u.FormatUlongAsHex()}";
        }

        if (obj is BigInteger bi)
        {
            return bi.FormatBigIntegerAsHex();
        }

        throw new ArgumentException("Invalid type");
    }
    public static string FormatAsHexBytes(this object obj)
    {
        if (obj is BigInteger bi)
        {
            return bi.FormatBigIntegerAsHex();
        }

        var bytes = obj.GetBytes();

        // Reverse for big-endian display (the most significant byte first)
        Array.Reverse(bytes);
        return "0x" + Convert.ToHexString(bytes);
    }

    private static Byte[] GetBytes(this object obj)
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
            short s => BitConverter.GetBytes(s),
            ushort us => BitConverter.GetBytes(us),
            int i => BitConverter.GetBytes(i),
            uint ui => BitConverter.GetBytes(ui),
            long l => BitConverter.GetBytes(l),
            ulong ul => BitConverter.GetBytes(ul),
#if NET7_0_OR_GREATER
            Int128 i128 => i128.GetBytesFromInt128(),
            UInt128 u128 => u128.GetBytesFromUInt128(),
#endif
            _ => throw new ArgumentException("Invalid type")
        };
    }
    public static int GetByteSize<T>(this T obj)
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
            _ => throw new ArgumentException("Invalid type")
        };
    }
    public static ulong SignExtendConvertToUlong<T>(this T obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (!obj.IsIntegerPrimitive())
        {
            throw new ArgumentException("Invalid type");
        }

        var byteSize = GetByteSize(obj);
        var buffer = new byte[8];
        var bytes = obj.GetBytes();

        for (var i = 0; i < byteSize; i++)
        {
            buffer[i] = bytes[i];
        }

        if (buffer[byteSize - 1] >= 0x80)
        {
            // sign-extend
            for (var i = byteSize; i < 8; i++)
            {
                buffer[i] = 0xFF;
            }
        }

        var result = BitConverter.ToUInt64(buffer, 0);
        return result;
    }

    public static string FormatUlongAsBinary(this ulong value)
    {
        if (value == 0) return "0";

        var highBytes = (uint)(value >> 32);
        var lowBytes = (uint)value;

        return ((highBytes != 0
                    ? Convert.ToString(highBytes, 2)
                    : "") +
                Convert.ToString(lowBytes, 2)
                    .PadLeft(highBytes == 0
                        ? 0
                        : 32, '0'))
            .ToUpper();
    }
    public static string FormatUlongAsHex(this ulong value)
    {
        if (value == 0) return "0";

        var highBytes = (uint)(value >> 32);
        var lowBytes = (uint)value;

        return ((highBytes != 0
                    ? Convert.ToString(highBytes, 16)
                    : "") +
                Convert.ToString(lowBytes, 16)
                    .PadLeft(highBytes == 0
                        ? 0
                        : 8, '0'))
            .ToUpper();
    }

    public static string FormatBigIntegerAsBinary(this BigInteger value)
    {
        if (value == 0)
        {
            return "0";
        }

        var signed = value < 0;
        if (signed)
        {
            value = -value;
        }

        var result = new StringBuilder();
        while (value != 0)
        {
            result.Append(value % 2);
            value /= 2;
        }

        var binaryString = string.Join("", result.ToString()
            .Reverse());
        return signed
            ? $"-0b{binaryString}"
            : $"0b{binaryString}";
    }
    public static string FormatBigIntegerAsHex(this BigInteger value)
    {
        if (value == 0)
        {
            return "0";
        }

        var signed = value < 0;
        var absValue = BigInteger.Abs(value);
        var hexString = absValue.ToString("X"); // Built-in hex formatting

        return signed
            ? $"-0x{hexString}"
            : $"0x{hexString}";
    }
    
#if NET7_0_OR_GREATER
    public static string FormatUInt128AsBinary(this UInt128 value)
    {
        if (value == 0) return "0";

        var highBytes = (ulong)(value >> 64);
        var lowBytes = (ulong)value;

        return ((highBytes != 0
                    ? highBytes.FormatUlongAsBinary()
                    : "") +
                lowBytes.FormatUlongAsBinary()
                    .PadLeft(highBytes == 0
                        ? 0
                        : 64, '0'))
            .ToUpper();
    }
    public static string FormatUInt128AsHex(this UInt128 value)
    {
        if (value == 0) return "0";

        var highBytes = (ulong)(value >> 64);
        var lowBytes = (ulong)value;

        return ((highBytes != 0
                    ? highBytes.FormatUlongAsHex()
                    : "") +
                lowBytes.FormatUlongAsHex()
                    .PadLeft(highBytes == 0
                        ? 0
                        : 16, '0'))
            .ToUpper();
    }

    public static Byte[] GetBytesFromInt128(this Int128 value)
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
    public static Byte[] GetBytesFromUInt128(this UInt128 value)
    {
        var highBytes = (ulong)(value >> 64);
        var lowBytes = (ulong)value;
        var bytes = new Byte[16];
        var index = 0;
        Array.Copy(BitConverter.GetBytes(lowBytes), 0, bytes, 0, 8);
        Array.Copy(BitConverter.GetBytes(highBytes), 0, bytes, 8, 8);
        return bytes;
    }

#endif
}