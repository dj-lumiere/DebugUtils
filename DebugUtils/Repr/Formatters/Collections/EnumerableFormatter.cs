using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(IEnumerable))]
[ReprOptions(needsPrefix: true)]
internal class EnumerableFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        // Apply container defaults if configured
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var list = (IEnumerable)obj;
        var type = list.GetType();

        var items = new List<string>();
        int? itemCount = null;

        if (type.GetProperty(name: "Count")
               ?.GetValue(obj: obj) is { } value)
        {
            itemCount = (int)value;
        }

        var i = 0;
        var hitLimit = false;
        foreach (var item in list)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            items.Add(item: item.Repr(context: context.WithIncrementedDepth()));
            i += 1;
        }

        if (hitLimit)
        {
            if (itemCount is not null)
            {
                var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
                if (remainingCount > 0)
                {
                    items.Add(item: $"... ({remainingCount} more items)");
                }
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
        var list = (IEnumerable)obj;
        var type = list.GetType();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return type.CreateMaxDepthReachedJson(depth: context.Depth);
        }

        int? itemCount = null;

        if (type.GetProperty(name: "Count")
               ?.GetValue(obj: obj) is { } value)
        {
            itemCount = (int)value;
        }


        var entries = new JsonArray();
        var result = new JsonObject();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        result.Add(propertyName: "hashCode", value: $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}");
        if (itemCount is not null)
        {
            result.Add(propertyName: "count", value: itemCount);
        }

        if (list.GetType()
                .GetGenericArguments()
                .Length != 0)
        {
            var elementType = list.GetType()
                                  .GetGenericArguments()[0]
                                  .GetReprTypeName();
            result.Add(propertyName: "elementType", value: elementType);
        }

        var i = 0;
        var hitLimit = false;

        foreach (var item in list)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                hitLimit = true;
                break;
            }

            entries.Add(item: item.FormatAsJsonNode(context: context.WithIncrementedDepth()));
            i += 1;
        }

        if (hitLimit)
        {
            if (itemCount is not null)
            {
                var remainingCount = itemCount - context.Config.MaxElementsPerCollection;
                if (remainingCount > 0)
                {
                    entries.Add(value: $"... ({remainingCount} more items)");
                }
            }
            else
            {
                entries.Add(value: "... (more items)");
            }
        }

        result.Add(propertyName: "value", value: entries);
        return result;
    }
}