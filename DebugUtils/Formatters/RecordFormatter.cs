using System.Reflection;
using DebugUtils.Records;
using DebugUtils.Interfaces;

namespace DebugUtils.Formatters;

/// <summary>
/// A generic formatter for any record type.
/// It uses reflection to represent the record's public properties.
/// </summary>
public class RecordFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var type = obj.GetType();
        var parts = new List<string>();
        visited ??= new HashSet<int>();

        // Get public properties with getters
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && (p.GetMethod?.IsPublic ?? false));

        foreach (var prop in properties)
        {
            // The compiler-generated "EqualityContract" property is not useful for representation.
            if (prop.Name == "EqualityContract")
            {
                continue;
            }

            try
            {
                var value = prop.GetValue(obj);
                // Recursively call the main Repr method for the value!
                parts.Add($"{prop.Name}: {value.Repr(config, visited)}");
            }
            catch (Exception ex)
            {
                parts.Add($"{prop.Name}: <Error: {ex.Message}>");
            }
        }

        return $"{{ {string.Join(", ", parts)} }}";
    }
}
