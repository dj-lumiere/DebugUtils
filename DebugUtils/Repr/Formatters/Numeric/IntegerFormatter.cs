using System.ComponentModel;
using System.Numerics;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
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
        return context.Config.IntMode switch
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
        var result = new JsonObject();
        var type = obj.GetType();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        result.Add(propertyName: "value", value: ToRepr(obj: obj, context: context));
        return result;
    }
}