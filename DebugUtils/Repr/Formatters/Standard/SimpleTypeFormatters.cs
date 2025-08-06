using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters.Standard;

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
        var result = new JsonObject();
        var type = obj.GetType();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        result.Add(propertyName: "value", value: ToRepr(obj: obj, context: context));
        return result;
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
        var json = new JsonObject();
        var underlyingType = Enum.GetUnderlyingType(enumType: e.GetType());
        var numericValue = Convert.ChangeType(value: e, conversionType: underlyingType);

        json.Add(propertyName: "type", value: e.GetReprTypeName());
        json.Add(propertyName: "kind", value: "enum");
        json.Add(propertyName: "name", value: e.ToString());
        json.Add(propertyName: "value", value: numericValue.Repr(context: context));
        return json;
    }
}