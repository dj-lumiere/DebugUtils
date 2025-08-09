using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

[ReprFormatter(typeof(ITuple))]
[ReprOptions(needsPrefix: false)]
internal class TupleFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        // Apply container defaults if configured
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var tuple = (ITuple)obj;

        var sb = new StringBuilder();
        sb.Append(value: '(');
        for (var i = 0; i < tuple.Length; i++)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                break;
            }

            if (i > 0)
            {
                sb.Append(value: ", ");
            }

            sb.Append(value: tuple[index: i]
               .Repr(context: context.WithIncrementedDepth()));
        }

        if (context.Config.MaxElementsPerCollection >= 0 &&
            tuple.Length > context.Config.MaxElementsPerCollection)
        {
            var truncatedItemCount = tuple.Length -
                                     context.Config.MaxElementsPerCollection;
            sb.Append(value: $"... {truncatedItemCount} more items");
        }

        sb.Append(value: ')');
        return sb.ToString();
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        // Apply container defaults if configured
        context = context.WithContainerConfig();
        var tuple = (ITuple)obj;
        var type = tuple.GetType();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = type.GetReprTypeName(),
                [propertyName: "kind"] = type.GetTypeKind(),
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth
            };
        }

        var result = new JsonObject();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        if (!type.IsValueType)
        {
            result.Add(propertyName: "hashCode",
                value: $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}");
        }

        result.Add(propertyName: "length", value: tuple.Length);

        var entries = new JsonArray();
        for (var i = 0; i < tuple.Length; i++)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                i >= context.Config.MaxElementsPerCollection)
            {
                break;
            }

            entries.Add(value: tuple[index: i]
               .FormatAsJsonNode(context: context.WithIncrementedDepth()));
        }

        if (context.Config.MaxElementsPerCollection >= 0 &&
            tuple.Length > context.Config.MaxElementsPerCollection)
        {
            var truncatedItemCount = tuple.Length -
                                     context.Config.MaxElementsPerCollection;
            entries.Add(item: $"... ({truncatedItemCount} more items)");
        }

        result.Add(propertyName: "value", value: entries);
        return result;
    }
}