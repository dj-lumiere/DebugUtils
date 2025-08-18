using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

// ReSharper disable BuiltInTypeReferenceStyle

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(IntPtr))]
[ReprOptions(needsPrefix: false)]
internal class IntPtrFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return IntPtr.Size == 4
            ? $"0x{((IntPtr)obj).ToInt32():X8}"
            : $"0x{((IntPtr)obj).ToInt64():X16}";
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
            { "type", type.GetReprTypeName() },
            { "kind", type.GetTypeKind() },
            { "value", ToRepr(obj: obj, context: context) }
        };
    }
}

[ReprFormatter(typeof(UIntPtr))]
[ReprOptions(needsPrefix: false)]
internal class UIntPtrFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return IntPtr.Size == 4
            ? $"0x{((UIntPtr)obj).ToUInt32():X8}"
            : $"0x{((UIntPtr)obj).ToUInt64():X16}";
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
            { "type", type.GetReprTypeName() },
            { "kind", type.GetTypeKind() },
            { "value", ToRepr(obj: obj, context: context) }
        };
    }
}