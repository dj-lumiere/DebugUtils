using System.Reflection;
using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Fallback;

/// <summary>
///     The default object pointer that handles any type not specifically registered.
///     It uses reflection to represent the record's public properties.
/// </summary>
[ReprOptions(needsPrefix:true)]
public class ObjectFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var type = obj.GetType();
        var parts = new List<string>();
        config = config.GetContainerConfig();

        // Get public fields
        var fields = type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(obj: obj);
            parts.Add(item: $"{field.Name}: {value.Repr(config: config, visited: visited)}");
        }

        // Get public properties with getters
        var properties = type
                        .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                        .Where(predicate: p => p is { CanRead: true, GetMethod.IsPublic: true });
        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(obj: obj);
                parts.Add(
                    item: $"{prop.Name}: {value.Repr(config: config, visited: visited)}");
            }
            catch
            {
                parts.Add(item: $"{prop.Name}: <error>");
            }
        }

        var content = parts.Count > 0
            ? String.Join(separator: ", ", values: parts)
            : "";
        return $"{content}";
    }
}