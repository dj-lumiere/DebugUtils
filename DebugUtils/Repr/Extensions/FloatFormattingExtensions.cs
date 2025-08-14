using System.ComponentModel;
using DebugUtils.Repr.Models;

namespace DebugUtils.Repr.Extensions;

internal static class FloatFormattingExtensions
{
    public static string FormatAsRounding(this object obj, FloatInfo info,
        ReprContext context)
    {
        var config = context.Config;
        var precision = config.FloatPrecision;
        if (precision is < 0 or > 100)
        {
            return obj.FormatAsExact(info: info);
        }

        var roundingFormatString = $"F{precision}";
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

    public static string FormatAsGeneral(this object obj, FloatInfo info,
        ReprContext context)
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

    public static string FormatAsScientific(this object obj, FloatInfo info,
        ReprContext context)
    {
        var config = context.Config;
        var precision = config.FloatPrecision;
        if (precision is < 0 or > 100)
        {
            return obj.FormatAsExact(info: info);
        }

        var scientificFormatString = $"E{precision}";
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

    public static string FormatAsHexPower(this object obj, FloatInfo info)
    {
        var mantissaNibbleAligned = info.Mantissa << info.TypeName switch
        {
            FloatTypeKind.Half => 2, // 10-bit mantissa, so shift left twice to align with nibble
            FloatTypeKind.Float => 1, // 23-bit mantissa, so shift left once to align with nibble
            FloatTypeKind.Double => 0, // 52-bit mantissa, so no shifting is needed
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
        };

        var power = info.RealExponent + info.Spec.MantissaBitSize;
        var sign = info.IsNegative
            ? "-"
            : "";
        var frontPart = info.IsSubnormal
            ? "0x0."
            : "0x1.";

        return info.TypeName switch
        {
            #if NET5_0_OR_GREATER
            FloatTypeKind.Half =>
                $"{sign}{frontPart}{mantissaNibbleAligned:X3}P{(power >= 0 ? "+" : "")}{power:D3}",
            #endif
            FloatTypeKind.Float =>
                $"{sign}{frontPart}{mantissaNibbleAligned:X6}P{(power >= 0 ? "+" : "")}{power:D3}",
            FloatTypeKind.Double =>
                $"{sign}{frontPart}{mantissaNibbleAligned:X13}P{(power >= 0 ? "+" : "")}{power:D3}",
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
        };
    }
}