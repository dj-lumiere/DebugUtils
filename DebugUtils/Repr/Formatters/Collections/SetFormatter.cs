using System.Collections;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Collections;

[ReprOptions(needsPrefix: true)]
internal class SetFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var list = (IEnumerable)obj;
        var collectable = (ICollection)obj;
        // Apply container defaults if configured
        context = context.WithContainerConfig();

        var items = new List<string>();
        var count = 0;
        foreach (var item in list)
        {
            if (context.Config.MaxElementsPerCollection >= 0 &&
                count >= context.Config.MaxElementsPerCollection)
            {
                break;
            }

            items.Add(item: item?.Repr(context: context.WithIncrementedDepth()) ??
                            "null");
            count += 1;
        }

        if (context.Config.MaxElementsPerCollection >= 0 &&
            collectable.Count > context.Config.MaxElementsPerCollection)
        {
            var truncatedItemCount = collectable.Count -
                                     context.Config.MaxElementsPerCollection;
            items.Add(item: $"... {truncatedItemCount} more items");
        }

        return "{" + String.Join(separator: ", ", values: items) + "}";
    }
}