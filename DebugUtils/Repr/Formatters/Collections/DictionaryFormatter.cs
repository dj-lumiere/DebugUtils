using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(IDictionary))]
internal class DictionaryFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        // Apply container defaults if configured
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var dict = (IDictionary)obj;

        if (dict.Count == 0)
        {
            return "{}";
        }

        var items = new List<string>();

        var count = 0;
        foreach (DictionaryEntry entry in dict)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                count >= context.Config.MaxItemsPerContainer)
            {
                break;
            }

            var key = entry.Key?.Repr(context: context.WithIncrementedDepth()) ?? "null";
            var value = entry.Value?.Repr(context: context.WithIncrementedDepth()) ?? "null";
            items.Add(item: $"{key}: {value}");
            count += 1;
        }


        if (context.Config.MaxItemsPerContainer >= 0 &&
            dict.Count > context.Config.MaxItemsPerContainer)
        {
            var truncatedItemCount = dict.Count -
                                     context.Config.MaxItemsPerContainer;
            items.Add(item: $"... {truncatedItemCount} more items");
        }

        return "{" + String.Join(separator: ", ", values: items) + "}";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        // Apply container defaults if configured
        context = context.WithContainerConfig();
        var dict = (IDictionary)obj;
        var type = dict.GetType();

        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return type.CreateMaxDepthReachedJson(depth: context.Depth);
        }

        var entries = new JsonArray();
        var keyType = dict.GetType()
                          .GetGenericArguments()[0]
                          .GetReprTypeName();
        var valueType = dict.GetType()
                            .GetGenericArguments()[1]
                            .GetReprTypeName();
        var count = 0;
        foreach (DictionaryEntry entry in dict)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                count >= context.Config.MaxItemsPerContainer)
            {
                break;
            }

            var entryJson = new JsonObject
            {
                [propertyName: "key"] =
                    entry.Key.FormatAsJsonNode(context: context.WithIncrementedDepth()),
                [propertyName: "value"] =
                    entry.Value?.FormatAsJsonNode(context: context.WithIncrementedDepth()) ?? null
            };
            entries.Add(value: entryJson);
            count += 1;
        }

        if (context.Config.MaxItemsPerContainer >= 0 &&
            dict.Count > context.Config.MaxItemsPerContainer)
        {
            var truncatedItemCount = dict.Count -
                                     context.Config.MaxItemsPerContainer;
            entries.Add(item: $"... ({truncatedItemCount} more items)");
        }

        return new JsonObject
        {
            { "type", type.GetReprTypeName() },
            { "kind", type.GetTypeKind() },
            { "hashCode", $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}" },
            { "count", dict.Count },
            { "keyType", keyType },
            { "valueType", valueType },
            { "value", entries }
        };
    }
}