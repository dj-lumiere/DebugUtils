using System.Collections;
using DebugUtils.Records;
using DebugUtils.Interfaces;

namespace DebugUtils.Formatters;

public class EnumerableFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var list = (IEnumerable)obj;
        // Apply container defaults if configured
        config = config.ForceFloatModeInContainer && config.ForceIntModeInContainer
            ? config
            : ReprConfig.ContainerDefaults;

        var items = list.Cast<object>()
            .Select(selector: item => item?.Repr(config: config, visited: visited) ?? "null");

        if (obj.IsSet())
        {
            return "{" + String.Join(separator: ", ", values: items) + "}";
        }

        return "[" + String.Join(separator: ", ", values: items) + "]";
    }
}