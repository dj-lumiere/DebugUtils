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
}