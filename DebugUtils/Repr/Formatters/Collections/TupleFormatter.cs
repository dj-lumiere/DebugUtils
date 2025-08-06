using System.Runtime.CompilerServices;
using System.Text;
using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Collections;

[ReprOptions(needsPrefix: false)]
internal class TupleFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var tuple = (ITuple)obj;
        visited ??= new HashSet<int>();
        // Apply container defaults if configured
        config = config.GetContainerConfig();

        var sb = new StringBuilder();
        sb.Append(value: "(");
        for (var i = 0; i < tuple.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(value: ", ");
            }

            sb.Append(value: tuple[index: i]
               .Repr(config: config, visited: visited));
        }

        sb.Append(value: ")");
        return sb.ToString();
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var tuple = (ITuple)obj;
        var type = tuple.GetType();
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
        var entries = new JsonArray();
        for (var i = 0; i < tuple.Length; i++)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                break;
            }

            entries.Add(value: tuple[index: i]
              ?.FormatAsJsonNode(context: context.WithIncrementedDepth()) ?? null);
        }

        if (context.Config.MaxElementsPerCollection >= 0 &&
            tuple.Length > context.Config.MaxElementsPerCollection)
        {
            var truncatedItemCount = tuple.Length -
                                     context.Config.MaxElementsPerCollection;
            entries.Add(item: $"... {truncatedItemCount} more items");
        }

        result.Add(propertyName: "count", value: tuple.Length);
        result.Add(propertyName: "value", value: entries);
        return result;
    }
}