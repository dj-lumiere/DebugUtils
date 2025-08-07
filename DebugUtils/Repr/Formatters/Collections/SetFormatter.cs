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
        var type = list.GetType();
        // Apply container defaults if configured
        context = context.WithContainerConfig();

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

        return "{" + String.Join(separator: ", ", values: items) + "}";
    }
}