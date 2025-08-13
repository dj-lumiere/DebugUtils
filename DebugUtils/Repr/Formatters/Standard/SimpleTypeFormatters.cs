using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(bool))]
[ReprOptions(needsPrefix: false)]
internal class BoolFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return (bool)obj
            ? "true"
            : "false";
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

[ReprFormatter(typeof(Enum))]
[ReprOptions(needsPrefix: false)]
internal class EnumFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var e = (Enum)obj;
        var underlyingType = Enum.GetUnderlyingType(enumType: e.GetType());
        var numericValue = Convert.ChangeType(value: e, conversionType: underlyingType);
        return
            $"{e.GetReprTypeName()}.{e} ({numericValue.Repr(context: context)})";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var e = (Enum)obj;
        var underlyingType = Enum.GetUnderlyingType(enumType: e.GetType());
        var numericValue = Convert.ChangeType(value: e, conversionType: underlyingType);

        return new JsonObject
        {
            [propertyName: "type"] = e.GetReprTypeName(),
            [propertyName: "kind"] = "enum",
            [propertyName: "name"] = e.ToString(),
            [propertyName: "value"] = numericValue.FormatAsJsonNode(context: context)
        };
    }
}