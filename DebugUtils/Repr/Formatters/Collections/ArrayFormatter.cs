using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters.Collections;

[ReprFormatter(typeof(Array))]
[ReprOptions(needsPrefix: true)]
internal class ArrayFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var array = (Array)obj;
        // Apply container defaults if configured
        config = config.GetContainerConfig();

        var rank = array.Rank;
        var content = array.ArrayToReprRecursive(indices: new int[rank], dimension: 0,
            config: config, visited: visited);
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
                [propertyName: "maxDepthReached"] = true,
                [propertyName: "depth"] = context.Depth
            };
        }

        var result = new JsonObject();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        // Apply container defaults if configured
        context = context.WithContainerConfig();

        var rank = array.Rank;
        var content = array.ArrayToHierarchicalReprRecursive(indices: new int[rank], dimension: 0,
            context: context);
        result.Add(propertyName: "value", value: content);
        return result;
    }
}