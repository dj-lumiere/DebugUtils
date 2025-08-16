using System.ComponentModel;
using System.Globalization;
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
[ReprOptions(needsPrefix: false)]
internal class IntegerFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return FormatWithCustomString(obj: obj, formatString: context.Config.IntFormatString,
            culture: context.Config.Culture);
    }

    private static string FormatWithCustomString(object obj, string formatString,
        CultureInfo? culture)
    {
        return formatString switch
        {
            _ when formatString.StartsWith(value: 'B') || formatString.StartsWith(value: 'b') =>
                obj.FormatAsBinaryWithPadding(format: formatString),
            _ when formatString.StartsWith(value: 'Q') || formatString.StartsWith(value: 'q') =>
                obj.FormatAsQuaternaryWithPadding(format: formatString),
            _ when formatString.StartsWith(value: 'O') || formatString.StartsWith(value: 'o') =>
                obj.FormatAsOctalWithPadding(format: formatString),
            _ when formatString.StartsWith(value: 'X') =>
                obj.FormatAsHexWithPadding(format: formatString),
            _ when formatString.StartsWith(value: 'x') =>
                obj.FormatAsHexWithPadding(format: formatString)
                   .ToLowerInvariant(),
            _ => FormatWithBuiltInToString(obj: obj, formatString: formatString, culture: culture)
        };
    }

    private static string FormatWithBuiltInToString(object obj, string formatString,
        CultureInfo? culture)
    {
        return obj switch
        {
            byte b => b.ToString(format: formatString, provider: culture),
            sbyte sb => sb.ToString(format: formatString, provider: culture),
            short s => s.ToString(format: formatString, provider: culture),
            ushort us => us.ToString(format: formatString, provider: culture),
            int i => i.ToString(format: formatString, provider: culture),
            uint ui => ui.ToString(format: formatString, provider: culture),
            long l => l.ToString(format: formatString, provider: culture),
            ulong ul => ul.ToString(format: formatString, provider: culture),
            BigInteger bi => bi.ToString(format: formatString, provider: culture),
            #if NET7_0_OR_GREATER
            Int128 i128 => i128.ToString(format: formatString, provider: culture),
            UInt128 u128 => u128.ToString(format: formatString, provider: culture),
            #endif
            _ => throw new InvalidEnumArgumentException(message: "Invalid Repr Config")
        };
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var type = obj.GetType();
        if (context.Depth > 0)
        {
            return (ToRepr(obj, context) + TypeNameMappings.TypeSuffixNames[type])!;
        }
        return new JsonObject
        {
            [propertyName: "type"] = type.GetReprTypeName(),
            [propertyName: "kind"] = type.GetTypeKind(),
            [propertyName: "value"] = ToRepr(obj: obj, context: context)
        };
    }
}