using System.Collections;

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

        var items = list.Cast<object>().Select(item => item?.Repr(config, visited) ?? "null");
        
        if (obj.IsSet())
        {
            return "{" + string.Join(", ", items) + "}";
        }

        return "[" + string.Join(", ", items) + "]";
    }
}