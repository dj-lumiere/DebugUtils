using System.ComponentModel;
using System.Numerics;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Extensions;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int),
    typeof(uint), typeof(long), typeof(ulong), typeof(BigInteger)
    #if NET7_0_OR_GREATER
    , typeof(Int128), typeof(UInt128)
    #endif
)]
[ReprOptions(needsPrefix: true)]
internal class IntegerFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (!String.IsNullOrEmpty(value: context.Config.IntFormatString))
        {
            return FormatWithCustomString(obj: obj, formatString: context.Config.IntFormatString);
        }

        return FormatWithMode(obj: obj, mode: context.Config.IntMode);
    }

    private static string FormatWithCustomString(object obj, string formatString)
    {
        return formatString switch
        {
            "HB" => obj.FormatAsHexBytes(),
            "B" or "b" => obj.FormatAsBinary(),
            "X" => obj.FormatAsHex(),
            "x" => obj.FormatAsHex()
                      .ToLowerInvariant(),
            _ when formatString.StartsWith(value: "B") || formatString.StartsWith(value: "b") =>
                obj.FormatAsBinaryWithPadding(formatString: formatString),
            _ when formatString.StartsWith(value: "X") =>
                obj.FormatAsHexWithPadding(formatString: formatString),
            _ when formatString.StartsWith(value: "x") =>
                obj.FormatAsHexWithPadding(formatString: formatString)
                   .ToLowerInvariant(),
            _ => FormatWithBuiltInToString(obj: obj, formatString: formatString)
        };
    }

    private static string FormatWithBuiltInToString(object obj, string formatString)
    {
        return obj switch
        {
            byte b => b.ToString(format: formatString),
            sbyte sb => sb.ToString(format: formatString),
            short s => s.ToString(format: formatString),
            ushort us => us.ToString(format: formatString),
            int i => i.ToString(format: formatString),
            uint ui => ui.ToString(format: formatString),
            long l => l.ToString(format: formatString),
            ulong ul => ul.ToString(format: formatString),
            BigInteger bi => bi.ToString(format: formatString),
            #if NET7_0_OR_GREATER
            Int128 i128 => i128.ToString(format: formatString),
            UInt128 u128 => u128.ToString(format: formatString),
            #endif
            _ => throw new InvalidEnumArgumentException(message: "Invalid Repr Config")
        };
    }

    private static string FormatWithMode(object obj, IntReprMode mode)
    {
        return mode switch
        {
            IntReprMode.Binary => obj.FormatAsBinary(),
            IntReprMode.Decimal => obj.ToString()!,
            IntReprMode.Hex => obj.FormatAsHex(),
            IntReprMode.HexBytes => obj.FormatAsHexBytes(),
            _ => throw new InvalidEnumArgumentException(message: "Invalid Repr Config")
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