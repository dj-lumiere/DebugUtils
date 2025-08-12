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
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: obj[i]
               .Repr(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
            if (remainingCount > 0)
            {
                items.Add($"... ({remainingCount} more items)");
            }
        }

        return "[" + String.Join(", ", items) + "]";
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

        var result = new JsonObject();
        var entries = new JsonArray();
        var itemCount = obj.Length;
        var hitLimit = false;
        result.Add(propertyName: "type", value: "Span");
        result.Add(propertyName: "kind", value: "ref struct");
        result.Add(propertyName: "length", value: itemCount);
        result.Add(propertyName: "elementType", value: typeof(T).GetReprTypeName());

        for (var i = 0; i < obj.Length; i += 1)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: obj[i]
               .FormatAsJsonNode(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
            if (remainingCount > 0)
            {
                entries.Add(value: $"... ({remainingCount} more items)");
            }
        }

        result.Add(propertyName: "value", value: entries);
        return result;
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
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: obj[i]
               .Repr(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
            if (remainingCount > 0)
            {
                items.Add($"... ({remainingCount} more items)");
            }
        }

        return "[" + String.Join(", ", items) + "]";
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

        var result = new JsonObject();
        var entries = new JsonArray();
        var itemCount = obj.Length;
        var hitLimit = false;
        result.Add(propertyName: "type", value: "ReadOnlySpan");
        result.Add(propertyName: "kind", value: "ref struct");
        result.Add(propertyName: "length", value: itemCount);
        result.Add(propertyName: "elementType", value: typeof(T).GetReprTypeName());

        for (var i = 0; i < obj.Length; i += 1)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: obj[i]
               .FormatAsJsonNode(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
            if (remainingCount > 0)
            {
                entries.Add(value: $"... ({remainingCount} more items)");
            }
        }

        result.Add(propertyName: "value", value: entries);
        return result;
    }
}

[ReprOptions(true)]
internal class MemoryFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var memoryType = obj.GetType();
        var toArrayMethod = memoryType.GetMethod("ToArray");
        var array = (Array)toArrayMethod!.Invoke(obj, null)!;
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var items = new List<string>();
        var itemCount = array.Length;
        var hitLimit = false;
        for (var i = 0; i < array.Length; i += 1)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: array.GetValue(i)
                                 .Repr(context: context.WithIncrementedDepth()));
        }

        if (!hitLimit)
        {
            return "[" + String.Join(separator: ", ", values: items) + "]";
        }

        var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
        if (remainingCount > 0)
        {
            items.Add(item: $"... ({remainingCount} more items)");
        }

        return "[" + String.Join(separator: ", ", values: items) + "]";
    }
    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var memoryType = obj.GetType();
        var toArrayMethod = memoryType.GetMethod("ToArray");
        var array = (Array)toArrayMethod!.Invoke(obj, null)!;
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = "Memory",
                [propertyName: "kind"] = "struct",
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth
            };
        }

        var result = new JsonObject();
        var entries = new JsonArray();
        var itemCount = array.Length;
        var hitLimit = false;
        var elementType = array.GetType()
                               .GetElementType()
                              ?.GetReprTypeName() ?? "object";
        result.Add(propertyName: "type", value: "Memory");
        result.Add(propertyName: "kind", value: "struct");
        result.Add(propertyName: "length", value: itemCount);
        result.Add(propertyName: "elementType", value: elementType);

        for (var i = 0; i < array.Length; i += 1)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: array.GetValue(i)
                                   .FormatAsJsonNode(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
            if (remainingCount > 0)
            {
                entries.Add(value: $"... ({remainingCount} more items)");
            }
        }

        result.Add(propertyName: "value", value: entries);
        return result;
    }
}

[ReprOptions(true)]
internal class ReadOnlyMemoryFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        var memoryType = obj.GetType();
        var toArrayMethod = memoryType.GetMethod("ToArray");
        var array = (Array)toArrayMethod!.Invoke(obj, null)!;
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var items = new List<string>();
        var itemCount = array.Length;
        var hitLimit = false;
        for (var i = 0; i < array.Length; i += 1)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: array.GetValue(i)
                                 .Repr(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
            if (remainingCount > 0)
            {
                items.Add($"... ({remainingCount} more items)");
            }
        }

        return "[" + String.Join(", ", items) + "]";
    }
    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var memoryType = obj.GetType();
        var toArrayMethod = memoryType.GetMethod("ToArray");
        var array = (Array)toArrayMethod!.Invoke(obj, null)!;
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = "ReadOnlyMemory",
                [propertyName: "kind"] = "struct",
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth
            };
        }

        var result = new JsonObject();
        var entries = new JsonArray();
        var itemCount = array.Length;
        var hitLimit = false;
        var elementType = array.GetType()
                               .GetElementType()
                              ?.GetReprTypeName() ?? "object";
        result.Add(propertyName: "type", value: "ReadOnlyMemory");
        result.Add(propertyName: "kind", value: "struct");
        result.Add(propertyName: "length", value: itemCount);
        result.Add(propertyName: "elementType", value: elementType);

        for (var i = 0; i < array.Length; i += 1)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: array.GetValue(i)
                                   .FormatAsJsonNode(context: context.WithIncrementedDepth()));
        }

        if (hitLimit)
        {
            var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
            if (remainingCount > 0)
            {
                entries.Add(value: $"... ({remainingCount} more items)");
            }
        }

        result.Add(propertyName: "value", value: entries);
        return result;
    }
}

[ReprFormatter(typeof(Index))]
[ReprOptions(true)]
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
[ReprOptions(true)]
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
            { "start", range.Start.FormatAsJsonNode(context) },
            { "end", range.End.FormatAsJsonNode(context) }
        };
        return result;
    }
}