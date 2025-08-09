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
        var info = obj switch
        {
            #if NET5_0_OR_GREATER
            Half h => h.AnalyzeHalf(),
            #endif
            float f => f.AnalyzeFloat(),
            double d => d.AnalyzeDouble(),
            _ => throw new ArgumentException(message: "Invalid type")
        };

        var config = context.Config;
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
            FloatReprMode.Round => obj.FormatAsRounding(info: info, context: context),
            FloatReprMode.Scientific => obj.FormatAsScientific(info: info, context: context),
            FloatReprMode.General => obj.FormatAsGeneral(info: info, context: context),
            FloatReprMode.Exact => obj.FormatAsExact(info: info),

            _ => throw new InvalidEnumArgumentException(message: "Invalid FloatReprMode")
        };
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var result = new JsonObject();
        var type = obj.GetType();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        result.Add(propertyName: "value", value: ToRepr(obj: obj, context: context));
        return result;
    }
}