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
        // Apply container defaults if configured
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var array = (Array)obj;

        var rank = array.Rank;
        var content = array.ArrayToReprRecursive(indices: new int[rank], dimension: 0,
            context: context);
        return content;
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        // Apply container defaults if configured
        context = context.WithContainerConfig();
        var array = (Array)obj;
        var type = array.GetType();

        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return type.CreateMaxDepthReachedJson(depth: context.Depth);
        }

        var elementType = array.GetType()
                               .GetElementType()
                              ?.GetReprTypeName() ?? "object";
        var dimensions = new JsonArray();
        for (var i = 0; i < array.Rank; i++)
        {
            dimensions.Add(value: array.GetLength(dimension: i));
        }

        var rank = array.Rank;
        var content = array.ArrayToHierarchicalReprRecursive(indices: new int[rank], dimension: 0,
            context: context);

        return new JsonObject
        {
            { "type", type.GetReprTypeName() },
            { "kind", type.GetTypeKind() },
            { "hashCode", $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}" },
            { "rank", array.Rank },
            { "dimensions", dimensions },
            { "elementType", elementType },
            { "value", content }
        };
    }
}