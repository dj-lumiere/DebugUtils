using System.Collections;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters.Collections;

[ReprFormatter(typeof(IEnumerable))]
[ReprOptions(needsPrefix: true)]
internal class EnumerableFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var list = (IEnumerable)obj;
        // Apply container defaults if configured
        context = context.WithContainerConfig();

        var items = new List<string>();

        var count = 0;
        var hitLimit = false;
        foreach (var item in list)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                count > context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: item?.Repr(context: context.WithIncrementedDepth()) ??
                            "null");
            count += 1;
        }

        if (hitLimit)
        {
            if (list is ICollection collection)
            {
                var remainingCount = collection.Count - context.Config.MaxElementsPerCollection;
                if (remainingCount > 0)
                {
                    items.Add(item: $"... {remainingCount} more items");
                }
            }
            else
            {
                items.Add(item: "... more items");
            }
        }

        return "[" + String.Join(separator: ", ", values: items) + "]";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var list = (IEnumerable)obj;
        var type = list.GetType();

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
        var entries = new JsonArray();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        var count = 0;
        var hitLimit = false;

        foreach (var item in list)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                count > context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: item?.FormatAsJsonNode(context: context.WithIncrementedDepth()) ??
                              null);
            count += 1;
        }

        if (hitLimit)
        {
            if (list is ICollection collection)
            {
                var remainingCount = collection.Count - context.Config.MaxElementsPerCollection;
                if (remainingCount > 0)
                {
                    entries.Add(value: $"... {remainingCount} more items");
                }
            }
            else
            {
                entries.Add(value: "... more items");
            }
        }

        result.Add(propertyName: "count", value: count);
        result.Add(propertyName: "value", value: entries);
        return result;
    }
}