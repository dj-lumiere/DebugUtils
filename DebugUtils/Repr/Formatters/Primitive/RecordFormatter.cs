#region

using System.Reflection;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

#endregion

namespace DebugUtils.Repr.Formatters.Primitive;

/// <summary>
///     A generic formatter for any record type.
///     It uses reflection to represent the record's public properties.
/// </summary>
public class RecordFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var type = obj.GetType();
        var parts = new List<string>();
        visited ??= new HashSet<int>();
        config = config.GetContainerConfig();

        // Get public properties with getters
        var properties = type
            .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
            .Where(predicate: p => p.CanRead && (p.GetMethod?.IsPublic ?? false));

        foreach (var prop in properties)
        {
            // The compiler-generated "EqualityContract" property is not useful for representation.
            if (prop.Name == "EqualityContract")
            {
                continue;
            }

            try
            {
                var value = prop.GetValue(obj: obj);
                // Recursively call the main Repr method for the value!
                parts.Add(
                    item: $"{prop.Name}: {value.Repr(config: config, visited: visited)}");
            }
            catch (Exception ex)
            {
                parts.Add(item: $"{prop.Name}: <Error: {ex.Message}>");
            }
        }

        return $"{{ {String.Join(separator: ", ", values: parts)} }}";
    }
}