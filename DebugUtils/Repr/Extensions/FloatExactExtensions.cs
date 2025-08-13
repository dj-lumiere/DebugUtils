using System.Runtime.CompilerServices;
using System.Numerics;
using System.Globalization;
using DebugUtils.Repr.Models;

namespace DebugUtils.Repr.Extensions;

internal static class FloatExactExtensions
{
    // PRE-COMPUTED POWERS OF 5: 5^0 through 5^16 for efficient scaling
    // CONSTRAINT: Using chunks of 5^14 because 5^15 = 30,517,578,125 * 999,999,999 would overflow ulong
    // OPTIMIZATION: Avoids BigInteger.Pow calls in hot path
    private static readonly ulong[] PowersOf5 =
    {
        1, 5, 25, 125, 625, 3125, 15625, 78125, 390625, 1953125, 9765625, 48828125, 244140625,
        1220703125, 6103515625ul, 30517578125ul, 152587890625ul
    };

    public static string FormatAsExact(this object obj, FloatInfo info)
    {
        var realExponent = info.RealExponent;
        var significand = info.Significand;
        var isNegative = info.IsNegative;
        switch (significand)
        {
            case 0 when isNegative:
                return "-0.0E+000";
            case 0:
                return "0.0E+000";
        }

        // STACK ALLOCATION SIZING: Based on theoretical maximum values during exact conversion
        // 
        // THEORY: IEEE 754 significand * 2^realExponent must be converted to exact decimal
        // For negative exponents: significand * 2^(-k) = significand * 5^k / 10^k
        // Maximum intermediate value: (2^(1+mantissaBits)-1) * 5^(-minRealExponent+mantissaBits)
        //
        // CALCULATED MAXIMUMS:
        // Half (1+10=11 bits): (2^11-1) * 5^(14+10) ≈ 1.2×10^20  → needs 3 uint digits
        // Float (1+23=24 bits): (2^24-1) * 5^(126+23) ≈ 2.3×10^111 → needs 13 uint digits  
        // Double (1+52=53 bits): (2^53-1) * 5^(1022+52) ≈ 4.4×10^766 → needs 86 uint digits
        // 
        // CONSTRAINT: Using base-10^9 (uint can hold ≤ 4.29×10^9), so each uint = 9 decimal digits
        var digits = info.TypeName switch
        {
            FloatTypeKind.Half => stackalloc uint[3],
            FloatTypeKind.Float => stackalloc uint[13],
            FloatTypeKind.Double => stackalloc uint[86],
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(info.TypeName))
        };

        // Split the significand into two parts:
        // 1. The first digit is the remainder of the significand divided by 10^9
        // 2. The second digit is the quotient of the significand divided by 10^9
        // Since the maximum value of significand is 2^54-1 = 18,014,398,509,481,983 < 10^18,
        // we can safely use at best 2 uints to store the result.
        var (digit1, digit2) = (significand % ExactFormattingHelpers.Base,
            significand / ExactFormattingHelpers.Base);
        var length = 1;
        if (digit2 != 0)
        {
            length = 2;
            digits[index: 1] = (uint)digit2;
        }

        digits[index: 0] = (uint)digit1;

        // CORE ALGORITHM: Convert IEEE 754 binary representation to exact decimal
        //
        // IEEE 754 value = significand * 2^realExponent
        //
        // CASE 1: realExponent ≥ 0
        //   significand * 2^realExponent = exact integer (no denominator needed)
        //
        // CASE 2: realExponent < 0  
        //   significand * 2^(-k) = significand * (5^k / 10^k) = (significand * 5^k) / 10^k
        //   KEY INSIGHT: 2^(-k) * 5^k = 10^(-k), so we can represent exact fractions in decimal
        //
        // RESULT: numerator / 10^powerOf10Denominator represents the exact decimal value
        int powerOf10Denominator;

        if (realExponent >= 0)
        {
            // Multiply by 2^realExponent, no fractional part needed
            powerOf10Denominator = 0;
            length = digits.ScalePow2(len: length, k: realExponent);
        }
        else
        {
            // Convert a binary fraction to an exact decimal fraction
            // significand * 2^(-k) → (significand * 5^k) / 10^k
            powerOf10Denominator = -realExponent;
            length = digits.ScalePow5(len: length, k: powerOf10Denominator);
        }

        // Now we have: numerator / 10^powerOf10Denominator
        return digits.FormatAsExactDecimal(length: length,
            powerOf10Denominator: powerOf10Denominator, isNegative: isNegative);
    }

    public static string FormatAsExact_Old(this object obj, FloatInfo info)
    {
        var realExponent = info.RealExponent;
        var significand = info.Significand;
        var isNegative = info.IsNegative;
        var sign = isNegative
            ? "-"
            : "";
        if (significand == 0)
        {
            return $"{sign}0.0E+000";
        }

        // Convert to exact decimal representation
        BigInteger numerator;
        int powerOf10Denominator;

        if (realExponent >= 0)
        {
            numerator = significand * BigInteger.Pow(value: 2, exponent: realExponent);
            powerOf10Denominator = 0;
        }
        else
        {
            // We want enough decimal places to represent 1/2^(-binaryExponent) exactly
            // Since 2^n × 5^n = spec.MantissaBitSize^n, we need n = -binaryExponent decimal places
            powerOf10Denominator = -realExponent;
            numerator = significand * BigInteger.Pow(value: 5, exponent: powerOf10Denominator);
        }

        // Now we have: numerator / halfSpec.MantissaBitSize^powerOf10Denominator
        var numeratorStr = numerator.ToString(provider: CultureInfo.InvariantCulture);
        var realPowerOf10 = numeratorStr.Length - powerOf10Denominator - 1;
        var integerPart = numeratorStr.Substring(startIndex: 0, length: 1);
        var fractionalPart = numeratorStr.Substring(startIndex: 1)
                                         .TrimEnd(trimChar: '0')
                                         .PadLeft(totalWidth: 1, paddingChar: '0');
        var expSign = realPowerOf10 >= 0
            ? "+"
            : "-";
        if (realPowerOf10 < 0)
        {
            realPowerOf10 = -realPowerOf10;
        }

        return $"{sign}{integerPart}.{fractionalPart}E{expSign}{realPowerOf10:D3}";
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    private static int MulBySmall(this Span<uint> d, int len, ulong factor)
    {
        ulong carry = 0;
        for (var i = 0; i < len; i++)
        {
            var t = d[index: i] * factor + carry;
            d[index: i] = (uint)(t % ExactFormattingHelpers.Base);
            carry = t / ExactFormattingHelpers.Base;
        }

        while (carry != 0)
        {
            d[index: len] = (uint)(carry % ExactFormattingHelpers.Base);
            carry /= ExactFormattingHelpers.Base;
            len += 1;
        }

        return len;
    }

    // CHUNKING STRATEGY: Process powers in chunks to avoid ulong overflow
    // 
    // CONSTRAINT: MulBySmall uses ulong intermediate calculations
    // Factor * 999,999,999 (max digit value) must fit in ulong (< 2^64)
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    private static int ScalePow2(this Span<uint> a, int len, int k)
    {
        // CHUNK SIZE: 2^34 because 2^34 * 999,999,999 ≈ 1.7×10^19 < 2^64
        // But 2^35 * 999,999,999 ≈ 3.4×10^19 would overflow ulong
        while (k >= 34)
        {
            len = a.MulBySmall(len: len, factor: 1UL << 34);
            k -= 34;
        }

        if (k > 0)
        {
            len = a.MulBySmall(len: len, factor: 1UL << k);
        }

        return len;
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    private static int ScalePow5(this Span<uint> a, int len, int k)
    {
        // CHUNK SIZE: 5^14 because 5^14 * 999,999,999 ≈ 6.1×10^18 < 2^64  
        // But 5^15 * 999,999,999 ≈ 3.1×10^19 would overflow ulong
        while (k >= 14)
        {
            len = a.MulBySmall(len: len, factor: PowersOf5[14]);
            k -= 14;
        }

        if (k > 0)
        {
            len = a.MulBySmall(len: len, factor: PowersOf5[k]);
        }

        return len;
    }
}