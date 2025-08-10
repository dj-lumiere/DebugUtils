using System.Numerics;
using System.Text;

namespace DebugUtils.Repr.Extensions;

internal static class DecimalExtensions
{
    private const uint Base = 1_000_000_000;

    // 2^64/BASE = FIRST_QUOTIENT * 
    private const ulong FirstQuotient = 18446744073ul;
    private const ulong SecondQuotient = 4ul;
    private const ulong FirstRemainder = 709551616ul;
    private const ulong SecondRemainder = 294967296ul;
    private static readonly uint[] Digits = new uint[] { 0, 0, 0, 0 };
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
    public static string FormatAsExact(this decimal value)
    {
        // Get the internal bits
        var bits = Decimal.GetBits(d: value);
        // Extract components
        var flags = bits[3]; // Scale and sign
        var isNegative = (flags & 0x80000000) != 0;
        var scale = flags >> 16 & 0xFF; // How many digits after decimal

        var lo = (uint)bits[0]; // Low 32 bits of 96-bit integer
        var mid = (uint)bits[1]; // Middle 32 bits  
        var hi = (uint)bits[2]; // High 32 bits
        Array.Clear(array: Digits, index: 0, length: Digits.Length);

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
        var firstRemainder = b * FirstRemainder + d * SecondRemainder + f;
        var firstQuotientLower64Bit =
            b * FirstQuotient + ((ulong)c << 32) + d * SecondQuotient + e + firstRemainder / Base;

        Digits[0] = (uint)(firstRemainder % Base);
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
        var secondRemainder = a * FirstRemainder + i;
        var secondQuotient = a * FirstQuotient + h + secondRemainder / Base;
        Digits[1] = (uint)(secondRemainder % Base);
        Digits[2] = (uint)(secondQuotient % Base);
        secondQuotient /= Base;
        Digits[3] = (uint)(secondQuotient % Base);

        var sign = isNegative
            ? "-"
            : "";

        if (value == 0)
        {
            return $"{sign}0.0E0";
        }

        var sb = new StringBuilder();
        for (var j = 3; j >= 0; j -= 1)
        {
            switch (sb.Length)
            {
                case 0 when Digits[j] == 0:
                    continue;
                case 0 when Digits[j] != 0:
                    sb.Append(value: Digits[j]);
                    break;
                default:
                    sb.Append(value: $"{Digits[j]:D9}");
                    break;
            }
        }

        var valueStr = sb.ToString();

        var realPowerOf10 = valueStr.Length - (scale + 1);
        var integerPart = valueStr[..1];
        var fractionalPart = valueStr[1..]
                            .TrimEnd(trimChar: '0')
                            .PadLeft(totalWidth: 1, paddingChar: '0');
        return $"{sign}{integerPart}.{fractionalPart}E{realPowerOf10}";
    }
}