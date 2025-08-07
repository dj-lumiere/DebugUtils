using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters.Collections;

[ReprFormatter(typeof(IDictionary))]
internal class DictionaryFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var dict = (IDictionary)obj;
        // Apply container defaults if configured
        context = context.WithContainerConfig();

        if (dict.Count == 0)
        {
            return "{}";
        }

        var items = new List<string>();

        var count = 0;
        foreach (DictionaryEntry entry in dict)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                count >= context.Config.MaxElementsPerCollection)
            {
                break;
            }

            var key = entry.Key?.Repr(context: context.WithIncrementedDepth()) ?? "null";
            var value = entry.Value?.Repr(context: context.WithIncrementedDepth()) ?? "null";
            items.Add(item: $"{key}: {value}");
            count += 1;
        }


        if (context.Config.MaxElementsPerCollection >= 0 &&
            dict.Count > context.Config.MaxElementsPerCollection)
        {
            var truncatedItemCount = dict.Count -
                                     context.Config.MaxElementsPerCollection;
            items.Add(item: $"... {truncatedItemCount} more items");
        }

        return "{" + String.Join(separator: ", ", values: items) + "}";
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var dict = (IDictionary)obj;
        var type = dict.GetType();

        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = type.GetReprTypeName(),
                [propertyName: "kind"] = type.GetTypeKind(),
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth.ToString()
            };
        }

        var result = new JsonObject();
        var entries = new JsonArray();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        result.Add(propertyName: "count", value: dict.Count.ToString());
        result.Add(propertyName: "hashCode", value: $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}");
        var keyType = dict.GetType()
                          .GetGenericArguments()[0]
                          .GetReprTypeName();
        var valueType = dict.GetType()
                            .GetGenericArguments()[1]
                            .GetReprTypeName();
        result.Add(propertyName: "keyType", value: keyType);
        result.Add(propertyName: "valueType", value: valueType);
        var count = 0;
        foreach (DictionaryEntry entry in dict)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                count >= context.Config.MaxElementsPerCollection)
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

        if (context.Config.MaxElementsPerCollection >= 0 &&
            dict.Count > context.Config.MaxElementsPerCollection)
        {
            var truncatedItemCount = dict.Count -
                                     context.Config.MaxElementsPerCollection;
            entries.Add(item: $"... ({truncatedItemCount} more items)");
        }

        result.Add(propertyName: "value", value: entries);
        return result;
    }
}