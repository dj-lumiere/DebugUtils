using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

internal static class SpanFormatter
{
    public static string ToRepr<T>(Span<T> obj, ReprContext context)
    {
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var items = new List<string>();
        var itemCount = obj.Length;
        var hitLimit = false;
        for (var i = 0; i < obj.Length; i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: obj[index: i]
               .Repr(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxItemsPerContainer;
            if (remainingCount > 0)
            {
                items.Add(item: $"... ({remainingCount} more items)");
            }
        }

        return "[" + String.Join(separator: ", ", values: items) + "]";
    }
    public static JsonNode ToReprTree<T>(Span<T> obj, ReprContext context)
    {
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = "Span",
                [propertyName: "kind"] = "ref struct",
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth
            };
        }

        var entries = new JsonArray();
        var itemCount = obj.Length;
        var hitLimit = false;

        for (var i = 0; i < obj.Length; i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: obj[index: i]
               .FormatAsJsonNode(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxItemsPerContainer;
            if (remainingCount > 0)
            {
                entries.Add(value: $"... ({remainingCount} more items)");
            }
        }

        return new JsonObject
        {
            [propertyName: "type"] = "Span",
            [propertyName: "kind"] = "ref struct",
            [propertyName: "length"] = itemCount,
            [propertyName: "elementType"] = typeof(T).GetReprTypeName(),
            [propertyName: "value"] = entries
        };
    }
}

internal static class ReadOnlySpanFormatter
{
    public static string ToRepr<T>(ReadOnlySpan<T> obj, ReprContext context)
    {
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var items = new List<string>();
        var itemCount = obj.Length;
        var hitLimit = false;
        for (var i = 0; i < obj.Length; i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: obj[index: i]
               .Repr(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxItemsPerContainer;
            if (remainingCount > 0)
            {
                items.Add(item: $"... ({remainingCount} more items)");
            }
        }

        return "[" + String.Join(separator: ", ", values: items) + "]";
    }
    public static JsonNode ToReprTree<T>(ReadOnlySpan<T> obj, ReprContext context)
    {
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = "ReadOnlySpan",
                [propertyName: "kind"] = "ref struct",
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth
            };
        }

        var entries = new JsonArray();
        var itemCount = obj.Length;
        var hitLimit = false;

        for (var i = 0; i < obj.Length; i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: obj[index: i]
               .FormatAsJsonNode(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxItemsPerContainer;
            if (remainingCount > 0)
            {
                entries.Add(value: $"... ({remainingCount} more items)");
            }
        }

        return new JsonObject
        {
            [propertyName: "type"] = "ReadOnlySpan",
            [propertyName: "kind"] = "ref struct",
            [propertyName: "length"] = itemCount,
            [propertyName: "elementType"] = typeof(T).GetReprTypeName(),
            [propertyName: "value"] = entries
        };
    }
}

[ReprOptions(needsPrefix: true)]
internal class MemoryFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var memoryType = obj.GetType();
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var toArrayMethod = memoryType.GetMethod(name: "ToArray");
        var array = (Array)toArrayMethod!.Invoke(obj: obj, parameters: null)!;
        var items = new List<string>();
        var itemCount = array.Length;
        var hitLimit = false;
        for (var i = 0; i < array.Length; i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: array.GetValue(index: i)
                                 .Repr(context: context.WithIncrementedDepth()));
        }

        if (!hitLimit)
        {
            return "[" + String.Join(separator: ", ", values: items) + "]";
        }

        var remainingCount = itemCount - context.Config.MaxItemsPerContainer;
        if (remainingCount > 0)
        {
            items.Add(item: $"... ({remainingCount} more items)");
        }

        return "[" + String.Join(separator: ", ", values: items) + "]";
    }
    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var type = obj.GetType();
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return type.CreateMaxDepthReachedJson(depth: context.Depth);
        }

        var toArrayMethod = type.GetMethod(name: "ToArray");
        var array = (Array)toArrayMethod!.Invoke(obj: obj, parameters: null)!;
        var entries = new JsonArray();
        var itemCount = array.Length;
        var hitLimit = false;
        var elementType = array.GetType()
                               .GetElementType()
                              ?.GetReprTypeName() ?? "object";

        for (var i = 0; i < array.Length; i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: array.GetValue(index: i)
                                   .FormatAsJsonNode(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxItemsPerContainer;
            if (remainingCount > 0)
            {
                entries.Add(value: $"... ({remainingCount} more items)");
            }
        }

        return new JsonObject
        {
            [propertyName: "type"] = "Memory",
            [propertyName: "kind"] = "struct",
            [propertyName: "length"] = itemCount,
            [propertyName: "elementType"] = elementType,
            [propertyName: "value"] = entries
        };
    }
}

[ReprOptions(needsPrefix: true)]
internal class ReadOnlyMemoryFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var type = obj.GetType();
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var toArrayMethod = type.GetMethod(name: "ToArray");
        var array = (Array)toArrayMethod!.Invoke(obj: obj, parameters: null)!;
        var items = new List<string>();
        var itemCount = array.Length;
        var hitLimit = false;
        for (var i = 0; i < array.Length; i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: array.GetValue(index: i)
                                 .Repr(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxItemsPerContainer;
            if (remainingCount > 0)
            {
                items.Add(item: $"... ({remainingCount} more items)");
            }
        }

        return "[" + String.Join(separator: ", ", values: items) + "]";
    }
    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        context = context.WithContainerConfig();
        var type = obj.GetType();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return type.CreateMaxDepthReachedJson(depth: context.Depth);
        }

        var toArrayMethod = type.GetMethod(name: "ToArray");
        var array = (Array)toArrayMethod!.Invoke(obj: obj, parameters: null)!;
        var entries = new JsonArray();
        var itemCount = array.Length;
        var hitLimit = false;
        var elementType = array.GetType()
                               .GetElementType()
                              ?.GetReprTypeName() ?? "object";

        for (var i = 0; i < array.Length; i += 1)
        {
            if (context.Config.MaxItemsPerContainer >= 0 &&
                i >= context.Config.MaxItemsPerContainer)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: array.GetValue(index: i)
                                   .FormatAsJsonNode(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxItemsPerContainer;
            if (remainingCount > 0)
            {
                entries.Add(value: $"... ({remainingCount} more items)");
            }
        }

        return new JsonObject
        {
            [propertyName: "type"] = "ReadOnlyMemory",
            [propertyName: "kind"] = "struct",
            [propertyName: "length"] = itemCount,
            [propertyName: "elementType"] = elementType,
            [propertyName: "value"] = entries
        };
    }
}

[ReprFormatter(typeof(Index))]
[ReprOptions(needsPrefix: true)]
internal class IndexFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var index = (Index)obj;
        return index.ToString();
    }
    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var index = (Index)obj;
        var result = new JsonObject
        {
            { "type", "Index" },
            { "kind", "struct" },
            { "value", index.ToString() },
            { "isFromEnd", index.IsFromEnd }
        };
        return result;
    }
}

[ReprFormatter(typeof(Range))]
[ReprOptions(needsPrefix: true)]
internal class RangeFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var range = (Range)obj;
        return range.ToString();
    }
    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var range = (Range)obj;
        var result = new JsonObject
        {
            { "type", "Range" },
            { "kind", "struct" },
            { "start", range.Start.FormatAsJsonNode(context: context) },
            { "end", range.End.FormatAsJsonNode(context: context) }
        };
        return result;
    }
}