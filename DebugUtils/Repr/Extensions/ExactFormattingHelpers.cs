using System.Runtime.CompilerServices;
using System.Text;

namespace DebugUtils.Repr.Extensions;

internal static class ExactFormattingHelpers
{
    internal const uint Base = 1_000_000_000;

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

    private static void AppendScientificExponent(this StringBuilder sb, int exponent)
    {
        var expSign = exponent >= 0
            ? '+'
            : '-';
        if (exponent < 0)
        {
            exponent = -exponent;
        }

        Span<char> expDigits = stackalloc char[3];
        for (var k = 2; k >= 0; k -= 1)
        {
            expDigits[index: k] = (char)('0' + exponent % 10);
            exponent /= 10;
        }

        sb.Append(value: 'E')
          .Append(value: expSign)
          .Append(value: expDigits);
    }

    private static void TrimTrailingZeros(this StringBuilder sb, int dotIndex)
    {
        var keep = sb.Length;
        while (keep > dotIndex + 1 && sb[index: keep - 1] == '0')
        {
            keep -= 1;
        }

        sb.Length = keep;

        if (sb.Length == dotIndex + 1)
        {
            sb.Append(value: '0');
        }
    }

    private static void AppendDigitsAsString(this StringBuilder sb, Span<uint> digits, int length)
    {
        // Handle the top (most significant) digit group
        var topDigits = digits[index: length - 1]
           .DecimalLength();
        Span<char> lastBuf = stackalloc char[topDigits];
        var lastDigit = digits[index: length - 1];
        for (var k = topDigits - 1; k >= 0; k -= 1)
        {
            lastBuf[index: k] = (char)('0' + lastDigit % 10);
            lastDigit /= 10;
        }

        sb.Append(value: lastBuf[index: 0]); // first digit
        sb.Append(value: '.'); // decimal point
        if (topDigits > 1)
        {
            sb.Append(value: lastBuf[1..]);
        }

        // Handle remaining digit groups (each exactly 9 digits)
        Span<char> buf = stackalloc char[9];
        for (var i = length - 2; i >= 0; i -= 1)
        {
            var digit = digits[index: i];
            for (var k = 8; k >= 0; k -= 1)
            {
                buf[index: k] = (char)('0' + digit % 10);
                digit /= 10;
            }

            sb.Append(value: buf);
        }
    }

    internal static string FormatAsExactDecimal(this Span<uint> digits, int length,
        int powerOf10Denominator, bool isNegative)
    {
        var topDigits = digits[index: length - 1]
           .DecimalLength();
        var totalDigits = (length - 1) * 9 + topDigits;

        var realPowerOf10 = totalDigits - powerOf10Denominator - 1;
        var sb = new StringBuilder(capacity: totalDigits + 16);

        if (isNegative)
        {
            sb.Append(value: '-');
        }

        sb.AppendDigitsAsString(digits: digits, length: length);

        var dotIndex = isNegative
            ? 2
            : 1;
        sb.TrimTrailingZeros(dotIndex: dotIndex);
        sb.AppendScientificExponent(exponent: realPowerOf10);

        return sb.ToString();
    }
}