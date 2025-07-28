using System.Globalization;
using System.Numerics;

public static partial class DebugUtils
{
    public static string FormatDecimalScientific(string exactStr, bool isNegative)
    {
        var sign = isNegative
            ? "-"
            : "";

        // Remove decimal point and count position
        var decimalIndex = exactStr.IndexOf('.');
        var allDigits = exactStr.Replace(".", "");

        // Remove leading zeros
        allDigits = allDigits.TrimStart('0');
        if (string.IsNullOrEmpty(allDigits)) return "0E0";

        // Calculate exponent
        int exponent;
        if (decimalIndex == -1)
        {
            // Integer
            exponent = allDigits.Length - 1;
        }
        else
        {
            // Has decimal point
            exponent = decimalIndex - 1;
        }

        // Format significand
        string significand;
        if (allDigits.Length == 1)
        {
            significand = allDigits;
        }
        else
        {
            significand = allDigits[0] + "." + allDigits.Substring(1);
            significand = significand.TrimEnd('0')
                .TrimEnd('.');
        }

        var expSign = exponent >= 0
            ? ""
            : "-";
        return $"{sign}{significand}E{expSign}{exponent}";
    }
    public static string FormatDecimalExact(decimal value)
    {
        if (value == 0) return "0";

        // Get the internal bits
        var bits = decimal.GetBits(value);

        // Extract components
        var lo = (uint)bits[0]; // Low 32 bits of 96-bit integer
        var mid = (uint)bits[1]; // Middle 32 bits  
        var hi = (uint)bits[2]; // High 32 bits
        var flags = bits[3]; // Scale and sign

        var isNegative = (flags & 0x80000000) != 0;
        var scale = (flags >> 16) & 0xFF; // How many digits after decimal

        // Reconstruct the 96-bit integer value
        var low64 = ((ulong)mid << 32) | lo;
        var integerValue = ((BigInteger)hi << 64) | low64;

        var sign = isNegative
            ? "-"
            : "";

        // Format with proper decimal places
        if (scale == 0)
        {
            return $"{sign}{integerValue}";
        }

        var valueStr = integerValue.ToString()
            .PadLeft((int)scale + 1, '0');
        var integerPart = valueStr.Substring(0, valueStr.Length - (int)scale);
        var fractionalPart = valueStr.Substring(valueStr.Length - (int)scale);

        // Remove trailing zeros from fractional part
        fractionalPart = fractionalPart.TrimEnd('0');

        if (fractionalPart.Length == 0)
        {
            return $"{sign}{integerPart}";
        }

        return $"{sign}{integerPart}.{fractionalPart}";
    }
    private static string PreciseScientificSimple(int binaryExponent, BigInteger significand,
        bool isNegative = false)
    {
        var sign = isNegative
            ? "-"
            : "";
        if (significand == 0)
        {
            return $"{sign}0E0";
        }

        // Convert to exact decimal representation
        BigInteger numerator;
        int powerOf10Denominator;

        if (binaryExponent >= 0)
        {
            numerator = significand * BigInteger.Pow(2, binaryExponent);
            powerOf10Denominator = 0;
        }
        else
        {
            // We want enough decimal places to represent 1/2^(-binaryExponent) exactly
            // Since 2^n × 5^n = 10^n, we need n = -binaryExponent decimal places
            powerOf10Denominator = -binaryExponent;
            numerator = significand * BigInteger.Pow(5, powerOf10Denominator);
        }

        // Now we have: numerator / 10^powerOf10Denominator
        var numeratorStr = numerator.ToString(CultureInfo.InvariantCulture);

        // Add leading zeros if necessary
        while (numeratorStr.Length <= powerOf10Denominator)
        {
            numeratorStr = "0" + numeratorStr;
        }

        // Insert decimal point
        string decimalStr;
        if (powerOf10Denominator == 0)
        {
            decimalStr = numeratorStr;
        }
        else
        {
            var integerPart =
                numeratorStr.Substring(0, numeratorStr.Length - powerOf10Denominator);
            var fractionalPart =
                numeratorStr.Substring(numeratorStr.Length - powerOf10Denominator);

            // Remove leading zeros from integer part, but keep at least one digit
            integerPart = integerPart.TrimStart('0');
            if (string.IsNullOrEmpty(integerPart)) integerPart = "0";

            decimalStr = $"{integerPart}.{fractionalPart}";
        }

        // Convert to scientific notation
        return ConvertToScientificNotation(decimalStr, sign);
    }

    private static string ConvertToScientificNotation(string decimalStr, string sign)
    {
        // Remove leading zeros and handle decimal point
        var trimmed = decimalStr.TrimStart('0');
        if (string.IsNullOrEmpty(trimmed) || trimmed == ".")
        {
            return "0E0";
        }

        // Find first significant digit
        var decimalIndex = trimmed.IndexOf('.');
        var allDigits = trimmed.Replace(".", "");

        // Remove leading zeros
        allDigits = allDigits.TrimStart('0');
        if (string.IsNullOrEmpty(allDigits))
        {
            return "0E0";
        }

        // Calculate exponent
        int exponent;
        if (decimalIndex == -1)
        {
            // Integer
            exponent = allDigits.Length - 1;
        }
        else if (decimalIndex == 0)
        {
            // Starts with decimal (like .00123)
            var leadingZeros = 0;
            for (int i = 1; i < trimmed.Length && trimmed[i] == '0'; i++)
            {
                leadingZeros++;
            }

            exponent = -(leadingZeros + 1);
        }
        else
        {
            // Normal case
            exponent = decimalIndex - 1;
        }

        // Format significand
        string significand;
        if (allDigits.Length == 1)
        {
            significand = allDigits;
        }
        else
        {
            significand = allDigits[0] + "." + allDigits.Substring(1);
            // Remove trailing zeros
            significand = significand.TrimEnd('0')
                .TrimEnd('.');
        }

        return $"{sign}{significand}E{exponent}";
    }
}