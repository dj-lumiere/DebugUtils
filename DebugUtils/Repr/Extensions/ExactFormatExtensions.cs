using System.Runtime.CompilerServices;
using System.Text;
using DebugUtils.Repr.Models;

namespace DebugUtils.Repr.Extensions;

internal static class ExactFormatExtensions
{
    private const ulong Base = 1000000000;

    private static readonly ulong[] PowersOf5 =
    {
        1, 5, 25, 125, 625, 3125, 15625, 78125, 390625, 1953125, 9765625, 48828125, 244140625,
        1220703125, 6103515625ul, 30517578125ul, 152587890625ul
    };

    public static string FormatAsExact(this object obj, FloatInfo info)
    {
        var realExponent = info.RealExponent - info.Spec.MantissaBitSize;
        var significand = info.Significand;
        var isNegative = info.IsNegative;
        switch (significand)
        {
            case 0 when isNegative:
                return "-0.0E0";
            case 0:
                return "0.0E0";
        }

        // Convert to exact decimal representation
        var digits = info.TypeName switch
        {
            FloatTypeKind.Half => stackalloc uint[3],
            FloatTypeKind.Float => stackalloc uint[14],
            FloatTypeKind.Double => stackalloc uint[86],
            _ => throw new ArgumentOutOfRangeException(nameof(info.TypeName))
        };
        var (digit1, digit2) = (significand % Base, significand / Base);
        var length = 1;
        if (digit2 != 0)
        {
            length = 2;
            digits[1] = (uint)digit2;
        }

        digits[0] = (uint)digit1;

        // Convert to exact decimal representation
        int powerOf10Denominator;

        if (realExponent >= 0)
        {
            powerOf10Denominator = 0;
            length = digits.ScalePow2(len: length, k: realExponent);
        }
        else
        {
            // We want enough decimal places to represent 1/2^(-binaryExponent) exactly
            // Since 2^n × 5^n = 10^n, we need n = -binaryExponent decimal places
            powerOf10Denominator = -realExponent;
            length = digits.ScalePow5(len: length, k: powerOf10Denominator);
        }


        // Now we have: numerator / 10^powerOf10Denominator

        var topDigits = digits[length - 1]
           .DecimalLength();
        var totalDigits = (length - 1) * 9 + topDigits;

        var realPowerOf10 = totalDigits - powerOf10Denominator - 1;
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
        if (topDigits > 1)
        {
            sb.Append(lastBuf[1..]);
        } // rest of top limb, if any

        Span<char> buf = stackalloc char[9];
        for (var i = length - 2; i >= 0; i -= 1)
        {
            var digit = digits[i];
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
    private static int MulBySmall(this Span<uint> d, int len, ulong factor)
    {
        ulong carry = 0;
        for (var i = 0; i < len; i++)
        {
            var t = d[i] * factor + carry;
            d[i] = (uint)(t % Base);
            carry = t / Base;
        }

        while (carry != 0)
        {
            d[len] = (uint)(carry % Base);
            carry /= Base;
            len += 1;
        }

        return len;
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    private static int ScalePow2(this Span<uint> a, int len, int k)
    {
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