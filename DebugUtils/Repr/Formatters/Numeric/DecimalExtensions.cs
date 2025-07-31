using System.Numerics;

namespace DebugUtils.Repr.Formatters.Numeric;

internal static class DecimalExtensions
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