using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(Guid))]
[ReprOptions(needsPrefix: true)]
internal class GuidFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return $"{((Guid)obj).ToString()}";
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

[ReprFormatter(typeof(Uri))]
[ReprOptions(needsPrefix: true)]
internal class UriFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return $"{(Uri)obj}";
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

[ReprFormatter(typeof(Version))]
[ReprOptions(needsPrefix: true)]
internal class VersionFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        return $"{((Version)obj).ToString()}";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var v = (Version)obj;
        var result = new JsonObject();
        result.Add(propertyName: "type", value: "Version");
        result.Add(propertyName: "kind", value: "class");
        result.Add(propertyName: "major", value: v.Major);
        result.Add(propertyName: "minor", value: v.Minor);
        result.Add(propertyName: "build", value: v.Build);
        result.Add(propertyName: "revision", value: v.Revision);
        return result;
    }
}