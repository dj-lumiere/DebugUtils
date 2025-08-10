using System.Runtime.CompilerServices;
using System.Text;
using DebugUtils.Repr.Models;

namespace DebugUtils.Repr.Extensions;

internal static class ExactFormatExtensions
{
    private const ulong Base = 1000000000;
    private static readonly uint[] Digits = new uint[90];

    private static readonly ulong[] PowersOf5 =
    {
        1, 5, 25, 125, 625, 3125, 15625, 78125, 390625, 1953125, 9765625, 48828125, 244140625,
        1220703125, 6103515625ul, 30517578125ul, 152587890625ul
    };

    public static string FormatHalfAsExact(this FloatInfo info)
    {
        var realExponent = info.RealExponent - info.Spec.MantissaBitSize;
        var significand = info.Significand;
        var isNegative = info.IsNegative;
        var sign = isNegative
            ? "-"
            : "";
        if (significand == 0)
        {
            return $"{sign}0.0E0";
        }

        // Convert to exact decimal representation
        int powerOf10Denominator;
        var length = 1;
        Digits[0] = (uint)significand;

        if (realExponent >= 0)
        {
            powerOf10Denominator = 0;
            length = Digits.ScalePow2(len: length, k: realExponent);
        }
        else
        {
            // We want enough decimal places to represent 1/2^(-binaryExponent) exactly
            // Since 2^n × 5^n = 10^n, we need n = -binaryExponent decimal places
            powerOf10Denominator = -realExponent;
            length = Digits.ScalePow5(len: length, k: powerOf10Denominator);
        }

        // Now we have: numerator / 10^powerOf10Denominator
        var sb = new StringBuilder();
        for (var j = length - 1; j >= 0; j -= 1)
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

        if (sb.Length == 0)
        {
            sb.Append(value: '0');
        }

        var numeratorStr = sb.ToString();
        var realPowerOf10 = numeratorStr.Length - powerOf10Denominator - 1;
        var integerPart = numeratorStr[..1];
        var fractionalPart = numeratorStr[1..]
                            .TrimEnd(trimChar: '0')
                            .PadLeft(totalWidth: 1, paddingChar: '0');
        return $"{sign}{integerPart}.{fractionalPart}E{realPowerOf10}";
    }

    public static string FormatFloatAsExact(this FloatInfo info)
    {
        var realExponent = info.RealExponent - info.Spec.MantissaBitSize;
        var significand = info.Significand;
        var isNegative = info.IsNegative;
        var sign = isNegative
            ? "-"
            : "";
        if (significand == 0)
        {
            return $"{sign}0.0E0";
        }

        var length = 1;
        Digits[0] = (uint)significand;

        // Convert to exact decimal representation
        int powerOf10Denominator;

        if (realExponent >= 0)
        {
            powerOf10Denominator = 0;
            length = Digits.ScalePow2(len: length, k: realExponent);
        }
        else
        {
            // We want enough decimal places to represent 1/2^(-binaryExponent) exactly
            // Since 2^n × 5^n = 10^n, we need n = -binaryExponent decimal places
            powerOf10Denominator = -realExponent;
            length = Digits.ScalePow5(len: length, k: powerOf10Denominator);
        }

        // Now we have: numerator / 10^powerOf10Denominator

        var sb = new StringBuilder();
        for (var j = length - 1; j >= 0; j -= 1)
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

        if (sb.Length == 0)
        {
            sb.Append(value: '0');
        }

        var numeratorStr = sb.ToString();
        var realPowerOf10 = numeratorStr.Length - powerOf10Denominator - 1;
        var integerPart = numeratorStr[..1];
        var fractionalPart = numeratorStr[1..]
                            .TrimEnd(trimChar: '0')
                            .PadLeft(totalWidth: 1, paddingChar: '0');
        return $"{sign}{integerPart}.{fractionalPart}E{realPowerOf10}";
    }

    public static string FormatDoubleAsExact(this FloatInfo info)
    {
        var realExponent = info.RealExponent - info.Spec.MantissaBitSize;
        var significand = info.Significand;
        var isNegative = info.IsNegative;
        var sign = isNegative
            ? "-"
            : "";
        if (significand == 0)
        {
            return $"{sign}0.0E0";
        }

        // Convert to exact decimal representation

        var (digit1, digit2) = (significand % Base, significand / Base);
        var length = 1;
        if (digit2 != 0)
        {
            length = 2;
            Digits[1] = (uint)digit2;
        }

        Digits[0] = (uint)digit1;

        // Convert to exact decimal representation
        int powerOf10Denominator;

        if (realExponent >= 0)
        {
            powerOf10Denominator = 0;
            length = Digits.ScalePow2(len: length, k: realExponent);
        }
        else
        {
            // We want enough decimal places to represent 1/2^(-binaryExponent) exactly
            // Since 2^n × 5^n = 10^n, we need n = -binaryExponent decimal places
            powerOf10Denominator = -realExponent;
            length = Digits.ScalePow5(len: length, k: powerOf10Denominator);
        }


        // Now we have: numerator / 10^powerOf10Denominator

        var sb = new StringBuilder();
        for (var j = length - 1; j >= 0; j -= 1)
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

        if (sb.Length == 0)
        {
            sb.Append(value: '0');
        }

        var numeratorStr = sb.ToString();
        var realPowerOf10 = numeratorStr.Length - powerOf10Denominator - 1;
        var integerPart = numeratorStr[..1];
        var fractionalPart = numeratorStr[1..]
                            .TrimEnd(trimChar: '0')
                            .PadLeft(totalWidth: 1, paddingChar: '0');
        return $"{sign}{integerPart}.{fractionalPart}E{realPowerOf10}";
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    private static int MulBySmall(this uint[] d, int len, ulong factor)
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
    private static int ScalePow2(this uint[] a, int len, int k)
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
    private static int ScalePow5(this uint[] a, int len, int k)
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
}