using System.ComponentModel;
using System.Globalization;
using System.Numerics;

namespace DebugUtils.Formatters;

internal class FloatFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig reprConfig, HashSet<int>? visited = null)
    {
        var info = obj switch
        {
#if NET5_0_OR_GREATER
            Half h => h.AnalyzeHalf(),
#endif
            float f => f.AnalyzeFloat(),
            double d => d.AnalyzeDouble(),
            _ => throw new ArgumentException("Invalid type")
        };

        // those two repr modes prioritize bit perfect representation, so they are processed first.
        switch (reprConfig.FloatMode)
        {
            case FloatReprMode.HexBytes:
                return info.TypeName switch
                {
                    FloatTypeKind.Half or FloatTypeKind.Float or FloatTypeKind.Double =>
                        $"0x{info.Bits.ToString($"X{(info.Spec.TotalSize + 3) / 4}")}",
                    _ => throw new InvalidEnumArgumentException("Invalid FloatTypeKind")
                };
            case FloatReprMode.BitField:
                return info.TypeName switch
                {
                    FloatTypeKind.Half or FloatTypeKind.Float or FloatTypeKind.Double =>
                        $"{(info.IsNegative ? 1 : 0)}|{info.ExpBits}|{info.MantissaBits}",
                    _ => throw new InvalidEnumArgumentException("Invalid FloatTypeKind")
                };
        }

        if (info.IsPositiveInfinity)
        {
            return reprConfig.FloatMode switch
            {
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    $"Infinity",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        if (info.IsNegativeInfinity)
        {
            return reprConfig.FloatMode switch
            {
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    $"-Infinity",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        if (info.IsQuietNaN)
        {
            return reprConfig.FloatMode switch
            {
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    $"Quiet NaN",
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        if (info.IsSignalingNaN)
        {
            var payloadFormat = info.TypeName switch
            {
                FloatTypeKind.Half or FloatTypeKind.Float or FloatTypeKind.Double =>
                    $"Signaling NaN, Payload: 0x{info.Mantissa.ToString($"X{(info.Spec.MantissaBitSize + 3) / 4}")}",
                _ => throw new InvalidEnumArgumentException("Invalid FloatTypeKind")
            };

            return reprConfig.FloatMode switch
            {
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General => payloadFormat,
                _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
            };
        }

        return reprConfig.FloatMode switch
        {
            FloatReprMode.Round => obj.AsRounding(info, reprConfig),
            FloatReprMode.Scientific => obj.AsScientific(info, reprConfig),
            FloatReprMode.General => obj.AsGeneral(info, reprConfig),
            FloatReprMode.Exact => obj.AsExact(info),

            _ => throw new InvalidEnumArgumentException("Invalid FloatReprMode")
        };
    }
}

internal static class FloatFormatterLogic
{
    public static string AsRounding(this object obj, FloatInfo info,
        ReprConfig reprConfig)
    {
        var roundingFormatString = "F" + (reprConfig.FloatPrecision > 0
            ? reprConfig.FloatPrecision
            : 0);
        return info.TypeName switch
        {
#if NET5_0_OR_GREATER
            FloatTypeKind.Half =>
                $"{((Half)obj).ToString(roundingFormatString)}",
#endif
            FloatTypeKind.Float =>
                $"{((float)obj).ToString(roundingFormatString)}",
            FloatTypeKind.Double =>
                $"{((double)obj).ToString(roundingFormatString)}",
            _ => throw new InvalidEnumArgumentException("Invalid FloatTypeKind")
        };
    }
    public static string AsGeneral(this object obj, FloatInfo info,
        ReprConfig reprConfig)
    {
        return info.TypeName switch
        {
#if NET5_0_OR_GREATER
            FloatTypeKind.Half =>
                $"{(Half)obj}",
#endif
            FloatTypeKind.Float =>
                $"{(float)obj}",
            FloatTypeKind.Double =>
                $"{(double)obj}",
            _ => throw new InvalidEnumArgumentException("Invalid FloatTypeKind")
        };
    }
    public static string AsScientific(this object obj, FloatInfo info,
        ReprConfig reprConfig)
    {
        var scientificFormatString = "E" + (reprConfig.FloatPrecision > 0
            ? reprConfig.FloatPrecision - 1
            : 0);
        return info.TypeName switch
        {
#if NET5_0_OR_GREATER
            FloatTypeKind.Half =>
                $"{((Half)obj).ToString(scientificFormatString)}",
#endif
            FloatTypeKind.Float =>
                $"{((float)obj).ToString(scientificFormatString)}",
            FloatTypeKind.Double =>
                $"{((double)obj).ToString(scientificFormatString)}",
            _ => throw new InvalidEnumArgumentException("Invalid FloatTypeKind")
        };
    }
    public static string AsExact(this object obj, FloatInfo info)
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
        BigInteger numerator;
        int powerOf10Denominator;

        if (realExponent >= 0)
        {
            numerator = significand * BigInteger.Pow(2, realExponent);
            powerOf10Denominator = 0;
        }
        else
        {
            // We want enough decimal places to represent 1/2^(-binaryExponent) exactly
            // Since 2^n × 5^n = spec.MantissaBitSize^n, we need n = -binaryExponent decimal places
            powerOf10Denominator = -realExponent;
            numerator = significand * BigInteger.Pow(5, powerOf10Denominator);
        }

        // Now we have: numerator / halfSpec.MantissaBitSize^powerOf10Denominator
        var numeratorStr = numerator.ToString(CultureInfo.InvariantCulture);
        var realPowerOf10 = numeratorStr.Length - powerOf10Denominator - 1;
        var integerPart = numeratorStr.Substring(0, 1);
        var fractionalPart = numeratorStr.Substring(1)
            .TrimEnd('0')
            .PadLeft(1, '0');
        return $"{sign}{integerPart}.{fractionalPart}E{realPowerOf10}";
    }
}