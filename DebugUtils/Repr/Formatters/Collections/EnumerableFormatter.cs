using System.Collections;
using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Collections;

[ReprOptions(needsPrefix: true)]
public class EnumerableFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var list = (IEnumerable)obj;
        // Apply container defaults if configured
        config = config.GetContainerConfig();

        var items = list.Cast<object>()
                        .Select(selector: item =>
                             item?.Repr(config: config, visited: visited) ?? "null");

        return "[" + String.Join(separator: ", ", values: items) + "]";
    }
}