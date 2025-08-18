using System.Numerics;

namespace DebugUtils.Repr.Extensions;

internal static class DecimalFormattingExtensions
{
    // PERFORMANCE CONSTRAINT: Using base-10^9 instead of base-10^18 because:
    // 1. Minimum supported .NET version is 2.1 (Unity) which lacks UInt128
    // 2. uint can safely hold up to 4,294,967,295, so 10^9 = 1,000,000,000 fits comfortably
    // 3. Each uint digit maps to exactly 9 decimal digits for efficient string generation
    // 4. Avoids expensive long division operations (/10, %10 loops)

    // Pre-computed division constants for converting powers of 2 to base-10^9:
    // 2^64 = Q64 * BASE + R64 = Q64 * 10^9 + R64
    private const ulong Q64 = 18446744073ul;
    private const ulong R64 = 709551616ul;

    // 2^32 = Q32 * BASE + R32 = 4 * 10^9 + 294,967,296
    private const ulong Q32 = 4ul;
    private const ulong R32 = 294967296ul;

    public static string FormatAsExact(this decimal value)
    {
        // Get the internal bits
        var bits = Decimal.GetBits(d: value);
        // Extract components
        var flags = bits[3]; // Scale and sign
        var isNegative = (flags & 0x80000000) != 0;
        var scale = flags >> 16 & 0xFF; // How many digits after decimal
        Span<uint> digits = stackalloc uint[4];

        var lo = (uint)bits[0]; // Low 32 bits of 96-bit integer
        var mid = (uint)bits[1]; // Middle 32 bits  
        var hi = (uint)bits[2]; // High 32 bits

        // Zero short-circuit (decimal doesn't preserve negative zero)
        if (lo == 0 && mid == 0 && hi == 0)
        {
            return "0.0E+000";
        }

        // ALGORITHM: Multi-stage division to convert decimal's 96-bit integer to base-10^9 digits
        // 
        // GOAL: Convert (hi * 2^64 + mid * 2^32 + lo) to base-10^9 representation
        // 
        // APPROACH: Break each 32-bit component into (quotient*10^9 + remainder):
        // hi = a * 10^9 + b, mid = c * 10^9 + d, lo = e * 10^9 + f
        // 
        // SUBSTITUTION:
        // (hi * 2^64 + mid * 2^32 + lo) / 10^9
        // = ((a * 10^9 + b) * 2^64 + (c * 10^9 + d) * 2^32 + e * 10^9 + f) / 10^9
        // = (a * 2^64 + c * 2^32 + e) * 10^9 + (b * 2^64 + d * 2^32 + f) / 10^9
        // = (a * 2^64 + c * 2^32 + e) * 10^9 + (b * (Q64 * 10^9 + R64) + d * (Q32 * 10^9 + R32) + f) / 10^9
        // = (a * 2^64 + b * Q64 + c * 2^32 + d * Q32 + e) * 10^9 + (b * R64 + d * R32 + f) / 10^9
        // = P * 10^9 + Q / 10^9
        // WHERE: P = a * 2^64 + b * Q64 + c * 2^32 + d * Q32 + e, Q = b * R64 + d * R32 + f
        // 
        // OVERFLOW SAFETY ANALYSIS: Maximum values when hi=mid=lo=3,999,999,999:
        // a = c = e = 3, b = d = f = 999,999,999
        // P_max = 999,999,999 * Q64 + 3 * 2^32 + Q32 * 999,999,999 + 3 = 18,446,744,071,438,157,814
        // Q_max = 999,999,999 * R64 + 999,999,999 * R32 + 999,999,999 = 1,004,518,911,995,481,087
        // Both values < ulong.MaxValue (18,446,744,073,709,551,615), so ulong arithmetic is safe.
        // Even Q_max/10^9 ≈ 1,004,518,911 added to P_max won't overflow ulong.

        var a = hi / ExactFormattingHelpers.Base;
        var b = hi % ExactFormattingHelpers.Base;
        var c = mid / ExactFormattingHelpers.Base;
        var d = mid % ExactFormattingHelpers.Base;
        var e = lo / ExactFormattingHelpers.Base;
        var f = lo % ExactFormattingHelpers.Base;
        var q = b * R64 + d * R32 + f;
        var p = b * Q64 + ((ulong)c << 32) + d * Q32 + e + q / ExactFormattingHelpers.Base;

        digits[index: 0] = (uint)(q % ExactFormattingHelpers.Base);

        // SECOND STAGE: Process the higher-order quotient from first stage
        // P = h * 10^9 + i
        // (a * 2^64 + h * 10^9 + i) / 10^9
        // = (a * (Q64 * 10^9 + R64) + h * 10^9 + i) / 10^9
        // = (a * Q64 + h) * 10^9 + (a * R64 + i) / 10^9
        // = R * 10^9 + S / 10^9
        // WHERE: R = a * Q64 + h + (S / 10^9), S = a * R64 + i
        // 
        // OVERFLOW SAFETY ANALYSIS: Since a ≤ 4 from the uint constraint:
        // Maximum theoretical h = 18,446,744,072
        // Maximum theoretical i = 999,999,999
        // R_theoretical_max = 4 * Q64 + 18,446,744,072 = 92,233,720,364
        // S_theoretical_max = 4 * R64 + 999,999,999 = 3,838,206,463
        // Both values < ulong.MaxValue (18,446,744,073,709,551,615), so ulong arithmetic is safe.

        var h = p / ExactFormattingHelpers.Base;
        var i = p % ExactFormattingHelpers.Base;
        var s = a * R64 + i;
        var r = a * Q64 + h + s / ExactFormattingHelpers.Base;
        digits[index: 1] = (uint)(s % ExactFormattingHelpers.Base);
        digits[index: 2] = (uint)(r % ExactFormattingHelpers.Base);
        r /= ExactFormattingHelpers.Base;
        digits[index: 3] = (uint)(r % ExactFormattingHelpers.Base);

        var length = 4;
        while (length > 1 && digits[index: length - 1] == 0)
        {
            length -= 1;
        }

        return digits.FormatAsExactDecimal(length: length, powerOf10Denominator: scale,
            isNegative: isNegative);
    }

    public static string FormatAsHexPower(this decimal value)
    {
        // Get the internal bits
        var bits = Decimal.GetBits(d: value);
        // Extract components
        var lo = (uint)bits[0]; // Low 32 bits of 96-bit integer
        var mid = (uint)bits[1]; // Middle 32 bits  
        var hi = (uint)bits[2]; // High 32 bits
        var flags = bits[3]; // Scale and sign
        var scale = (byte)(flags >> 16); // How many digits after decimal
        var isNegative = (flags & 0x80000000) != 0 && !(hi == 0 && mid == 0 && lo == 0);
        var sign = isNegative
            ? "-"
            : "";
        return scale == 0
            ? $"{sign}0x{hi:X8}_{mid:X8}_{lo:X8}" // All 24 hex digits
            : $"{sign}0x{hi:X8}_{mid:X8}_{lo:X8}p10-{scale:D3}"; // All 24 hex digits + scale
    }
}