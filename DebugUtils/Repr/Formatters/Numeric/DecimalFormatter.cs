using System.ComponentModel;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Extensions;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(decimal))]
[ReprOptions(needsPrefix: true)]
internal class DecimalFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var dec = (decimal)obj;
        return context.Config.FloatFormatString switch
        {
            "HP" => dec.FormatAsHexPower(),
            "EX" => dec.FormatAsExact(),
            _ => dec.ToString(format: context.Config.FloatFormatString)
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