using System.ComponentModel;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Extensions;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Models;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(
    typeof(float),
    typeof(double),
    #if NET5_0_OR_GREATER
    typeof(Half)
    #endif
)]
[ReprOptions(needsPrefix: true)]
internal class FloatFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var info = AnalyzeFloat(obj: obj);

        if (!String.IsNullOrEmpty(value: context.Config.FloatFormatString))
        {
            return FormatWithCustomString(obj: obj, info: info,
                formatString: context.Config.FloatFormatString);
        }

        return FormatWithMode(obj: obj, info: info, context: context);
    }

    private static FloatInfo AnalyzeFloat(object obj)
    {
        return obj switch
        {
            #if NET5_0_OR_GREATER
            Half h => h.AnalyzeHalf(),
            #endif
            float f => f.AnalyzeFloat(),
            double d => d.AnalyzeDouble(),
            _ => throw new ArgumentException(message: "Invalid type")
        };
    }

    private static string FormatWithCustomString(object obj, FloatInfo info, string formatString)
    {
        return formatString switch
        {
            "HB" => FormatAsHexBytes(info: info),
            "BF" => FormatAsBitField(info: info),
            _ when info.IsPositiveInfinity => "Infinity",
            _ when info.IsNegativeInfinity => "-Infinity",
            _ when info.IsQuietNaN => "Quiet NaN",
            _ when info.IsSignalingNaN => FormatSignalingNaN(info: info),
            "EX" => obj.FormatAsExact(info: info),
            _ => FormatWithBuiltInToString(obj: obj, formatString: formatString)
        };
    }

    private static string FormatWithBuiltInToString(object obj, string formatString)
    {
        return obj switch
        {
            Half h => h.ToString(format: formatString),
            float f => f.ToString(format: formatString),
            double d => d.ToString(format: formatString),
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
        };
    }

    private static string FormatWithMode(object obj, FloatInfo info, ReprContext context)
    {
        var config = context.Config;

        // Handle bit-perfect representation modes first
        if (config.FloatMode == FloatReprMode.HexBytes)
        {
            return FormatAsHexBytes(info: info);
        }

        if (config.FloatMode == FloatReprMode.BitField)
        {
            return FormatAsBitField(info: info);
        }

        // Handle special values
        if (info.IsPositiveInfinity)
        {
            return "Infinity";
        }

        if (info.IsNegativeInfinity)
        {
            return "-Infinity";
        }

        if (info.IsQuietNaN)
        {
            return "Quiet NaN";
        }

        if (info.IsSignalingNaN)
        {
            return FormatSignalingNaN(info: info);
        }

        // Handle normal values
        return config.FloatMode switch
        {
            FloatReprMode.Round => obj.FormatAsRounding(info: info, context: context),
            FloatReprMode.Scientific => obj.FormatAsScientific(info: info, context: context),
            FloatReprMode.General => obj.FormatAsGeneral(info: info, context: context),
            FloatReprMode.Exact_Old => obj.FormatAsExact_Old(info: info),
            FloatReprMode.Exact => obj.FormatAsExact(info: info),
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatReprMode")
        };
    }

    private static string FormatAsHexBytes(FloatInfo info)
    {
        return info.TypeName switch
        {
            FloatTypeKind.Half or FloatTypeKind.Float or FloatTypeKind.Double =>
                $"0x{info.Bits.ToString(format: $"X{info.Spec.TotalSize / 4}")}",
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
        };
    }

    private static string FormatAsBitField(FloatInfo info)
    {
        return info.TypeName switch
        {
            FloatTypeKind.Half or FloatTypeKind.Float or FloatTypeKind.Double =>
                $"{(info.IsNegative ? 1 : 0)}|{info.ExpBits}|{info.MantissaBits}",
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
        };
    }

    private static string FormatSignalingNaN(FloatInfo info)
    {
        return info.TypeName switch
        {
            FloatTypeKind.Half or FloatTypeKind.Float or FloatTypeKind.Double =>
                $"Signaling NaN, Payload: 0x{info.Mantissa.ToString(format: $"X{(info.Spec.MantissaBitSize + 3) / 4}")}",
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
        };
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var type = obj.GetType();
        return new JsonObject
        {
            [propertyName: "type"] = type.GetReprTypeName(),
            [propertyName: "kind"] = type.GetTypeKind(),
            [propertyName: "value"] = ToRepr(obj: obj, context: context)
        };
    }
}