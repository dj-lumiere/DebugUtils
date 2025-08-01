using System.Collections;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Collections;

internal class DictionaryFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var dict = (IDictionary)obj;
        // Apply container defaults if configured
        config = config.GetContainerConfig();

        if (dict.Count == 0)
        {
            return "{}";
        }

        var items = new List<string>();

        foreach (DictionaryEntry entry in dict)
        {
            var key = entry.Key?.Repr(config: config, visited: visited) ?? "null";
            var value = entry.Value?.Repr(config: config, visited: visited) ?? "null";
            items.Add(item: $"{key}: {value}");
        }

        return "{" + String.Join(separator: ", ", values: items) + "}";
    }
}