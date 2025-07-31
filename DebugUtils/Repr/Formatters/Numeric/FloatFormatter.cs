using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Numeric;

[ReprFormatter(
    typeof(float),
    typeof(double),
#if NET5_0_OR_GREATER
    typeof(Half)
#endif
)]
internal class FloatFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited = null)
    {
        var info = obj switch
        {
#if NET5_0_OR_GREATER
            Half h => h.AnalyzeHalf(),
#endif
            float f => f.AnalyzeFloat(),
            double d => d.AnalyzeDouble(),
            _ => throw new ArgumentException(message: "Invalid type")
        };

        // those two repr modes prioritize bit perfect representation, so they are processed first.
        switch (config.FloatMode)
        {
            case FloatReprMode.HexBytes:
                return info.TypeName switch
                {
                    FloatTypeKind.Half or FloatTypeKind.Float or FloatTypeKind.Double =>
                        $"0x{info.Bits.ToString(format: $"X{(info.Spec.TotalSize + 3) / 4}")}",
                    _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
                };
            case FloatReprMode.BitField:
                return info.TypeName switch
                {
                    FloatTypeKind.Half or FloatTypeKind.Float or FloatTypeKind.Double =>
                        $"{(info.IsNegative ? 1 : 0)}|{info.ExpBits}|{info.MantissaBits}",
                    _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
                };
        }

        if (info.IsPositiveInfinity)
        {
            return config.FloatMode switch
            {
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    "Infinity",
                _ => throw new InvalidEnumArgumentException(message: "Invalid FloatReprMode")
            };
        }

        if (info.IsNegativeInfinity)
        {
            return config.FloatMode switch
            {
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    "-Infinity",
                _ => throw new InvalidEnumArgumentException(message: "Invalid FloatReprMode")
            };
        }

        if (info.IsQuietNaN)
        {
            return config.FloatMode switch
            {
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General =>
                    "Quiet NaN",
                _ => throw new InvalidEnumArgumentException(message: "Invalid FloatReprMode")
            };
        }

        if (info.IsSignalingNaN)
        {
            var payloadFormat = info.TypeName switch
            {
                FloatTypeKind.Half or FloatTypeKind.Float or FloatTypeKind.Double =>
                    $"Signaling NaN, Payload: 0x{info.Mantissa.ToString(format: $"X{(info.Spec.MantissaBitSize + 3) / 4}")}",
                _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
            };

            return config.FloatMode switch
            {
                FloatReprMode.Exact or FloatReprMode.Scientific or FloatReprMode.Round
                    or FloatReprMode.General => payloadFormat,
                _ => throw new InvalidEnumArgumentException(message: "Invalid FloatReprMode")
            };
        }

        return config.FloatMode switch
        {
            FloatReprMode.Round => obj.AsRounding(info: info, reprConfig: config),
            FloatReprMode.Scientific => obj.AsScientific(info: info, reprConfig: config),
            FloatReprMode.General => obj.AsGeneral(info: info, reprConfig: config),
            FloatReprMode.Exact => obj.AsExact(info: info),

            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatReprMode")
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
                $"{((Half)obj).ToString(format: roundingFormatString)}",
#endif
            FloatTypeKind.Float =>
                $"{((float)obj).ToString(format: roundingFormatString)}",
            FloatTypeKind.Double =>
                $"{((double)obj).ToString(format: roundingFormatString)}",
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
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
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
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
                $"{((Half)obj).ToString(format: scientificFormatString)}",
#endif
            FloatTypeKind.Float =>
                $"{((float)obj).ToString(format: scientificFormatString)}",
            FloatTypeKind.Double =>
                $"{((double)obj).ToString(format: scientificFormatString)}",
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
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
        return $"{sign}{integerPart}.{fractionalPart}E{realPowerOf10}";
    }
}