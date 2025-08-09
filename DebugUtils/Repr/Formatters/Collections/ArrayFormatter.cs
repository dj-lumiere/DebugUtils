using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Extensions;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(Array))]
[ReprOptions(needsPrefix: true)]
internal class ArrayFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var array = (Array)obj;
        // Apply container defaults if configured
        context = context.WithContainerConfig();

        var rank = array.Rank;
        var content = array.ArrayToReprRecursive(indices: new int[rank], dimension: 0,
            context: context);
        return content;
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var array = (Array)obj;
        var type = array.GetType();

        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = type.GetReprTypeName(),
                [propertyName: "kind"] = type.GetTypeKind(),
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth
            };
        }

        var result = new JsonObject();
        var elementType = array.GetType()
                               .GetElementType()
                              ?.GetReprTypeName() ?? "object";
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        result.Add(propertyName: "hashCode", value: $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}");
        var dimensions = new JsonArray();
        for (var i = 0; i < array.Rank; i++)
        {
            dimensions.Add(value: array.GetLength(dimension: i));
        }

        result.Add(propertyName: "rank", value: array.Rank);
        result.Add(propertyName: "dimensions", value: dimensions);
        result.Add(propertyName: "elementType", value: elementType);
        // Apply container defaults if configured
        context = context.WithContainerConfig();

        var rank = array.Rank;
        var content = array.ArrayToHierarchicalReprRecursive(indices: new int[rank], dimension: 0,
            context: context);
        result.Add(propertyName: "value", value: content);
        return result;
    }
}