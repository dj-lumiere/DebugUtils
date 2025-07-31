using System.ComponentModel;
using System.Numerics;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Numeric;

public class DecimalFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited = null)
    {
        var dec = (decimal)obj;
        // Get the internal bits
        var bits = Decimal.GetBits(d: dec);

        // Extract components
        var lo = (uint)bits[0]; // Low 32 bits of 96-bit integer
        var mid = (uint)bits[1]; // Middle 32 bits  
        var hi = (uint)bits[2]; // High 32 bits
        var flags = bits[3]; // Scale and sign
        var scale = (byte)(flags >> 16); // How many digits after decimal
        var isNegative = (flags & 0x80000000) != 0;
        var scaleBits = Convert.ToString(value: scale, toBase: 2)
            .PadLeft(totalWidth: 8, paddingChar: '0');
        var hiBits = Convert.ToString(value: hi, toBase: 2)
            .PadLeft(totalWidth: 32, paddingChar: '0');
        var midBits = Convert.ToString(value: mid, toBase: 2)
            .PadLeft(totalWidth: 32, paddingChar: '0');
        var loBits = Convert.ToString(value: lo, toBase: 2)
            .PadLeft(totalWidth: 32, paddingChar: '0');

        return config.FloatMode switch
        {
            FloatReprMode.HexBytes =>
                $"0x{flags:X8}{hi:X8}{mid:X8}{lo:X8}",
            FloatReprMode.BitField =>
                $"{(isNegative ? 1 : 0)}|{scaleBits}|{hiBits}{midBits}{loBits}",
            FloatReprMode.Round =>
                $"{dec.ToString(format: "F" + (config.FloatPrecision > 0 ? config.FloatPrecision : 0))}",
            FloatReprMode.Scientific =>
                $"{dec.ToString(format: "E" + (config.FloatPrecision > 0 ? config.FloatPrecision - 1 : 0))}",
            FloatReprMode.Exact => $"{dec.AsExact()}",
            FloatReprMode.General => $"{dec}",
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatReprMode")
        };
    }
}

internal static class DecimalFormatterLogic
{
    public static string AsExact(this decimal value)
    {
        // Get the internal bits
        var bits = Decimal.GetBits(d: value);

        // Extract components
        var lo = (uint)bits[0]; // Low 32 bits of 96-bit integer
        var mid = (uint)bits[1]; // Middle 32 bits  
        var hi = (uint)bits[2]; // High 32 bits
        var flags = bits[3]; // Scale and sign

        var isNegative = (flags & 0x80000000) != 0;
        var scale = flags >> 16 & 0xFF; // How many digits after decimal

        // Reconstruct the 96-bit integer value
        var low64 = (ulong)mid << 32 | lo;
        var integerValue = (BigInteger)hi << 64 | low64;

        var sign = isNegative
            ? "-"
            : "";

        if (value == 0)
        {
            return $"{sign}0.0E0";
        }

        var valueStr = integerValue.ToString();
        var realPowerOf10 = valueStr.Length - (scale + 1);
        var integerPart = valueStr.Substring(startIndex: 0, length: 1);
        var fractionalPart = valueStr.Substring(startIndex: 1)
            .TrimEnd(trimChar: '0')
            .PadLeft(totalWidth: 1, paddingChar: '0');
        return $"{sign}{integerPart}.{fractionalPart}E{realPowerOf10}";
    }
}