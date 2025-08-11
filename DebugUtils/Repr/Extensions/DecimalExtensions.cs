using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace DebugUtils.Repr.Extensions;

internal static class DecimalExtensions
{
    private const uint Base = 1_000_000_000;

    // 2^64 = Q64 * BASE + R64
    private const ulong Q64 = 18446744073ul;
    private const ulong R64 = 709551616ul;

    // 2^32 = Q32 * BASE + R32.
    private const ulong Q32 = 4ul;
    private const ulong R32 = 294967296ul;
    public static string FormatAsExact_Old(this decimal value)
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
            return "0.0E0";
        }

        var valueStr = integerValue.ToString();
        var realPowerOf10 = valueStr.Length - (scale + 1);
        var integerPart = valueStr.Substring(startIndex: 0, length: 1);
        var fractionalPart = valueStr.Substring(startIndex: 1)
                                     .TrimEnd(trimChar: '0')
                                     .PadLeft(totalWidth: 1, paddingChar: '0');
        return $"{sign}{integerPart}.{fractionalPart}E{realPowerOf10}";
    }
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
            return "0.0E0";
        }
        
        // (hi*2^64+mid*2^32+lo)/10^9
        // = ((a*10^9+b)*2^64+(c*10^9+d)*2^32+e*10^9+f)/10^9
        // = (a*2^64+c*2^32+e)*10^9+(b*2^64+d*2^32+f)/10^9
        // = (a*2^64+c*2^32+e)*10^9+(b*(18,446,744,073*10^9+709,551,616)+d*(4*10^9+294,967,296)+f)/10^9
        // = (a*2^64+b*18,446,744,073+c*2^32+d*4+e)*10^9+(b*709,551,616+d*294,967,296+f)/10^9
        // = (a*2^64+P)*10^9+Q/10^9
        // The maximum value of P and Q is when hi=mid=low=3,999,999,999:
        // P_max=999,999,999*18,446,744,073+3*2^32+4*999,999,999+3=18,446,744,071,438,157,814
        // Q_max=999,999,999*709,551,616+999,999,999*294,967,296+999,999,999=1,004,518,911,995,481,087
        // Since both values are less than ulong overflow threshold 2^64-1=18,446,744,073,709,551,615,
        // we can safely use ulong to store the result.
        // Also, even adding 1,004,518,911(Q_max/10^9's quotient) to P will not overflow, so we can safely handle that too.

        var a = hi / Base;
        var b = hi % Base;
        var c = mid / Base;
        var d = mid % Base;
        var e = lo / Base;
        var f = lo % Base;
        var firstRemainder = b * R64 + d * R32 + f;
        var firstQuotientLower64Bit =
            b * Q64 + ((ulong)c << 32) + d * Q32 + e + firstRemainder / Base;

        digits[0] = (uint)(firstRemainder % Base);
        // The second stage is as follows.
        // (a*2^64+h*10^9+i)/10^9
        // = (a*(18,446,744,073*10^9+709,551,616)+h*10^9+i)/10^9
        // = (a*18,446,744,073+h)*10^9+(a*709,551,616+i)/10^9
        // = P*10^9+Q/10^9
        // Since a is up to 4 at this point, the theoretical max value is
        // when a=4, h=18,446,744,072, i=999,999,999:
        // P_max=4*18,446,744,073+18,446,744,072=92,233,720,364
        // Q_max=4*709,551,616+999,999,999=3838206463
        // Since both values are less than ulong overflow threshold 2^64-1=1,8446,744,073,709,551,615,
        // we can safely use ulong to store the result.
        // The rest doesn't have ulong at this point, so it is trivial.

        var h = firstQuotientLower64Bit / Base;
        var i = firstQuotientLower64Bit % Base;
        var secondRemainder = a * R64 + i;
        var secondQuotient = a * Q64 + h + secondRemainder / Base;
        digits[1] = (uint)(secondRemainder % Base);
        digits[2] = (uint)(secondQuotient % Base);
        secondQuotient /= Base;
        digits[3] = (uint)(secondQuotient % Base);

        int length = 4;
        while (length > 1 && digits[length - 1] == 0)
        {
            length -= 1;
        }

        var topDigits = digits[length - 1]
           .DecimalLength();
        var totalDigits = (length - 1) * 9 + topDigits;

        var realPowerOf10 = totalDigits - scale - 1;
        var sb = new StringBuilder(totalDigits + 16);
        if (isNegative)
        {
            sb.Append('-');
        }

        Span<char> lastBuf = stackalloc char[topDigits];
        var lastDigit = digits[length - 1];
        for (var k = topDigits - 1; k >= 0; k -= 1)
        {
            lastBuf[k] = (char)('0' + (lastDigit % 10));
            lastDigit /= 10;
        }

        sb.Append(lastBuf[0]); // first digit
        sb.Append('.'); // decimal point
        if (topDigits > 1) sb.Append(lastBuf[1..]); // rest of top limb, if any

        Span<char> buf = stackalloc char[9];
        for (var j = length - 2; j >= 0; j -= 1)
        {
            var digit = digits[j];
            for (var k = 8; k >= 0; k -= 1)
            {
                buf[k] = (char)('0' + digit % 10);
                digit /= 10;
            }

            sb.Append(buf);
        }

        // after sign and first digit
        var dotIndex = isNegative
            ? 2
            : 1;
        var keep = sb.Length;
        while (keep > dotIndex + 1 && sb[keep - 1] == '0')
        {
            keep -= 1;
        }

        sb.Length = keep;

        if (sb.Length == dotIndex + 1)
        {
            sb.Append('0'); // if we have only one digit after the decimal point, add zero
        }

        sb.Append('E')
          .Append(realPowerOf10);

        return sb.ToString();
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    private static int DecimalLength(this uint x)
    {
        return x switch
        {
            >= 100_000_000 => 9,
            >= 10_000_000 => 8,
            >= 1_000_000 => 7,
            >= 100_000 => 6,
            >= 10_000 => 5,
            >= 1_000 => 4,
            >= 100 => 3,
            >= 10 => 2,
            >= 0 => 1
        };
    }
}