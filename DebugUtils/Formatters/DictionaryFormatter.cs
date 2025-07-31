using System.Collections;
using DebugUtils.Records;

namespace DebugUtils.Formatters;

public class DictionaryFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var dict = (IDictionary)obj;
        // Apply container defaults if configured
        config = config.ForceFloatModeInContainer && config.ForceIntModeInContainer
            ? config
            : ReprConfig.ContainerDefaults;

        if (dict.Count == 0) return "{}";
        var items = new List<string>();
        
        foreach (DictionaryEntry entry in dict)
        {
            var key = entry.Key?.Repr(config, visited) ?? "null";
            var value = entry.Value?.Repr(config, visited) ?? "null";
            items.Add($"{key}: {value}");
        }

        return "{" + string.Join(", ", items) + "}";
    }
}