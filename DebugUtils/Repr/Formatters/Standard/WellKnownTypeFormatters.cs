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
        var type = obj.GetType();
        return new JsonObject
        {
            [propertyName: "type"] = type.GetReprTypeName(),
            [propertyName: "kind"] = type.GetTypeKind(),
            [propertyName: "value"] = ToRepr(obj: obj, context: context)
        };
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
        var type = obj.GetType();
        return new JsonObject
        {
            [propertyName: "type"] = type.GetReprTypeName(),
            [propertyName: "kind"] = type.GetTypeKind(),
            [propertyName: "value"] = ToRepr(obj: obj, context: context)
        };
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
        return new JsonObject
        {
            [propertyName: "type"] = "Version",
            [propertyName: "kind"] = "class",
            [propertyName: "major"] = v.Major,
            [propertyName: "minor"] = v.Minor,
            [propertyName: "build"] = v.Build,
            [propertyName: "revision"] = v.Revision
        };
    }
}