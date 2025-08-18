using System.ComponentModel;
using System.Globalization;
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
[ReprOptions(needsPrefix: false)]
internal class FloatFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var info = AnalyzeFloat(obj: obj);
        return FormatWithCustomString(obj: obj, info: info,
            formatString: context.Config.FloatFormatString, culture: context.Config.Culture);
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

    private static string FormatWithCustomString(object obj, FloatInfo info, string formatString,
        CultureInfo? culture)
    {
        return formatString switch
        {
            _ when info.IsPositiveInfinity => "Infinity",
            _ when info.IsNegativeInfinity => "-Infinity",
            _ when info.IsQuietNaN => FormatQuietNaN(info: info),
            _ when info.IsSignalingNaN => FormatSignalingNaN(info: info),
            "EX" => obj.FormatAsExact(info: info),
            "HP" => obj.FormatAsHexPower(info: info),
            _ => FormatWithBuiltInToString(obj: obj, formatString: formatString, culture: culture)
        };
    }

    private static string FormatWithBuiltInToString(object obj, string formatString,
        CultureInfo? culture)
    {
        return obj switch
        {
            Half h => h.ToString(format: formatString, provider: culture),
            float f => f.ToString(format: formatString, provider: culture),
            double d => d.ToString(format: formatString, provider: culture),
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
        };
    }

    private static string FormatQuietNaN(FloatInfo info)
    {
        return info.TypeName switch
        {
            FloatTypeKind.Half => $"QuietNaN(0x{info.Mantissa:X3})",
            FloatTypeKind.Float => $"QuietNaN(0x{info.Mantissa:X3})",
            FloatTypeKind.Double => $"QuietNaN(0x{info.Mantissa:X3})",
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
        };
    }
    private static string FormatSignalingNaN(FloatInfo info)
    {
        return info.TypeName switch
        {
            FloatTypeKind.Half => $"SignalingNaN(0x{info.Mantissa:X3})",
            FloatTypeKind.Float => $"SignalingNaN(0x{info.Mantissa:X3})",
            FloatTypeKind.Double => $"SignalingNaN(0x{info.Mantissa:X3})",
            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatTypeKind")
        };
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var type = obj.GetType();
        if (context.Depth > 0)
        {
            return obj.Repr(context: context)!;
        }

        return new JsonObject
        {
            [propertyName: "type"] = type.GetReprTypeName(),
            [propertyName: "kind"] = type.GetTypeKind(),
            [propertyName: "value"] = ToRepr(obj: obj, context: context)
        };
    }
}