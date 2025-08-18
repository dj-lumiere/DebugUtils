using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprOptions(needsPrefix: true)]
internal class PriorityQueueFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        // Apply container defaults if configured
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var type = obj.GetType();
        var countProperty = type.GetProperty(name: "Count");
        var count = (int)(countProperty?.GetValue(obj: obj) ?? 0);

        var unorderedItemsProperty = type.GetProperty(name: "UnorderedItems");
        var unorderedItems = (IEnumerable)unorderedItemsProperty!.GetValue(obj: obj)!;

        var items = new List<string>();
        var i = 0;
        var hitLimit = false;

        foreach (var item in unorderedItems)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            if (item is ITuple tuple && tuple.Length == 2)
            {
                var priority = tuple[index: 1];
                var element = tuple[index: 0];
                items.Add(
                    item:
                    $"{element.Repr(context: context.WithIncrementedDepth())} (priority: {priority.Repr(context: context.WithIncrementedDepth())})");
            }

            i += 1;
        }

        if (hitLimit)
        {
            var remainingCount = count - context.Config.MaxItemsPerContainer;
            if (remainingCount > 0)
            {
                items.Add(item: $"... ({remainingCount} more items)");
            }
            else
            {
                items.Add(item: "... (more items)");
            }
        }

        return "[" + String.Join(separator: ", ", values: items) + "]";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        // Apply container defaults if configured
        context = context.WithContainerConfig();
        var type = obj.GetType();

        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return type.CreateMaxDepthReachedJson(depth: context.Depth);
        }

        var countProperty = type.GetProperty(name: "Count");
        var count = (int)(countProperty?.GetValue(obj: obj) ?? 0);
        var result = new JsonObject
        {
            { "type", type.GetReprTypeName() },
            { "kind", type.GetTypeKind() },
            { "hashCode", $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}" },
            { "count", count }
        };

        // Get generic type arguments for element and priority types
        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length >= 2)
            {
                result.Add(propertyName: "elementType", value: genericArgs[0]
                   .GetReprTypeName());
                result.Add(propertyName: "priorityType", value: genericArgs[1]
                   .GetReprTypeName());
            }
        }

        var entries = new JsonArray();
        var unorderedItemsProperty = type.GetProperty(name: "UnorderedItems");
        var unorderedItems = (IEnumerable)unorderedItemsProperty!.GetValue(obj: obj)!;

        var i = 0;
        var hitLimit = false;

        foreach (var item in unorderedItems)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            if (item is ITuple tuple && tuple.Length == 2)
            {
                var priority = tuple[index: 1]
                   .FormatAsJsonNode(context: context.WithIncrementedDepth());
                var element = tuple[index: 0]
                   .FormatAsJsonNode(context: context.WithIncrementedDepth());
                var entry = new JsonObject
                {
                    { "element", element },
                    { "priority", priority }
                };
                entries.Add(value: entry);
            }

            i += 1;
        }

        if (hitLimit)
        {
            var remainingCount = count - context.Config.MaxItemsPerContainer;
            entries.Add(value: $"... ({remainingCount} more items)");
        }

        result.Add(propertyName: "value", value: entries);
        return result;
    }
}