using System.ComponentModel;
using System.Numerics;

public static partial class DebugUtils
{
    private static string ReprByte(this byte t, IntReprConfig intReprConfig)
    {
        return intReprConfig.mode switch
        {
            IntReprMode.Binary => "0b" + Convert.ToString(t, 2),
            IntReprMode.Decimal => t.ToString(),
            IntReprMode.Hex => "0x" + Convert.ToString(t, 16)
                .ToUpper(),
            IntReprMode.rawByteHex => "0x" + Convert.ToString(t, 16)
                .ToUpper()
                .PadLeft(2, '0'),
            _ => throw new InvalidEnumArgumentException("Invalid intReprConfig")
        };
    }

    private static string ReprSByte(this sbyte t, IntReprConfig intReprConfig)
    {
        return intReprConfig.mode switch
        {
            IntReprMode.Binary => (t < 0
                ? "-"
                : "") + "0b" + Convert.ToString(Math.Abs(t), 2),
            IntReprMode.Decimal => t.ToString(),
            IntReprMode.Hex => (t < 0
                ? "-"
                : "") + "0x" + Convert.ToString(Math.Abs(t), 16)
                .ToUpper(),
            IntReprMode.rawByteHex => "0x" + Convert.ToString((byte)t, 16)
                .ToUpper()
                .PadLeft(2, '0'),
            _ => throw new InvalidEnumArgumentException("Invalid intReprConfig")
        };
    }

    private static string ReprShort(this short t, IntReprConfig intReprConfig)
    {
        return intReprConfig.mode switch
        {
            IntReprMode.Binary => (t < 0
                ? "-"
                : "") + "0b" + Convert.ToString(Math.Abs(t), 2),
            IntReprMode.Decimal => t.ToString(),
            IntReprMode.Hex => (t < 0
                ? "-"
                : "") + "0x" + Convert.ToString(Math.Abs(t), 16)
                .ToUpper(),
            IntReprMode.rawByteHex => "0x" + Convert.ToString((ushort)t, 16)
                .ToUpper()
                .PadLeft(4, '0'),
            _ => throw new InvalidEnumArgumentException("Invalid intReprConfig")
        };
    }

    private static string ReprUShort(this ushort t, IntReprConfig intReprConfig)
    {
        return intReprConfig.mode switch
        {
            IntReprMode.Binary => "0b" + Convert.ToString(t, 2),
            IntReprMode.Decimal => t.ToString(),
            IntReprMode.Hex => "0x" + Convert.ToString(t, 16)
                .ToUpper(),
            IntReprMode.rawByteHex => "0x" + Convert.ToString(t, 16)
                .ToUpper()
                .PadLeft(4, '0'),
            _ => throw new InvalidEnumArgumentException("Invalid intReprConfig")
        };
    }

    private static string ReprInt(this int t, IntReprConfig intReprConfig)
    {
        return intReprConfig.mode switch
        {
            IntReprMode.Binary => (t < 0
                ? "-"
                : "") + "0b" + Convert.ToString(Math.Abs(t), 2),
            IntReprMode.Decimal => t.ToString(),
            IntReprMode.Hex => (t < 0
                ? "-"
                : "") + "0x" + Convert.ToString(Math.Abs(t), 16)
                .ToUpper(),
            IntReprMode.rawByteHex => "0x" + Convert.ToString((uint)t, 16)
                .ToUpper()
                .PadLeft(8, '0'),
            _ => throw new InvalidEnumArgumentException("Invalid intReprConfig")
        };
    }

    private static string ReprUInt(this uint t, IntReprConfig intReprConfig)
    {
        return intReprConfig.mode switch
        {
            IntReprMode.Binary => "0b" + Convert.ToString(t, 2),
            IntReprMode.Decimal => t.ToString(),
            IntReprMode.Hex => "0x" + Convert.ToString(t, 16)
                .ToUpper(),
            IntReprMode.rawByteHex => "0x" + Convert.ToString(t, 16)
                .ToUpper()
                .PadLeft(8, '0'),
            _ => throw new InvalidEnumArgumentException("Invalid intReprConfig")
        };
    }

    private static string ReprLong(this long t, IntReprConfig intReprConfig)
    {
        return intReprConfig.mode switch
        {
            IntReprMode.Binary => (t < 0
                ? "-"
                : "") + "0b" + Convert.ToString(Math.Abs(t), 2),
            IntReprMode.Decimal => t.ToString(),
            IntReprMode.Hex => (t < 0
                ? "-"
                : "") + "0x" + Convert.ToString(Math.Abs(t), 16)
                .ToUpper(),
            IntReprMode.rawByteHex => "0x" + ((ulong)t).ToString("X16")
                .ToUpper()
                .PadLeft(16, '0'),
            _ => throw new InvalidEnumArgumentException("Invalid intReprConfig")
        };
    }

    private static string ReprULong(this ulong t, IntReprConfig intReprConfig)
    {
        return intReprConfig.mode switch
        {
            IntReprMode.Binary => "0b" + ReprUInt((uint)(t >> 32), intReprConfig)
                                      .Substring((uint)(t >> 32) == 0
                                          ? 3
                                          : 2) +
                                  ReprUInt((uint)t, intReprConfig)
                                      .Substring(2)
                                      .PadLeft(((uint)(t >> 32) == 0
                                          ? 0
                                          : 32), '0'),
            IntReprMode.Decimal => t.ToString(),
            IntReprMode.Hex => "0x" + t.ToString("X"),
            IntReprMode.rawByteHex => "0x" + t.ToString("X16")
                .PadLeft(16, '0'),
            _ => throw new InvalidEnumArgumentException("Invalid intReprConfig")
        };
    }

    private static string ReprBigInteger(this BigInteger t, IntReprConfig intReprConfig)
    {
        return intReprConfig.mode switch
        {
            IntReprMode.Decimal => t.ToString(),
            _ => throw new InvalidEnumArgumentException("Invalid intReprConfig")
        };
    }
}