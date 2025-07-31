using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Fallback;

/// <summary>
///     The default object pointer that handles any type not specifically registered.
///     It uses reflection to represent the record's public properties.
/// </summary>
public class ObjectJsonFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        config = config.GetContainerConfig() with { TypeMode = TypeReprMode.AlwaysHide };
        var visited2 = new HashSet<int>();
        return obj.GetJson(config: config, visited: visited2, depth: 0)
                  .ToString();
    }
}

internal static class ObjectJsonFormatterLogic
{
    public static JsonObject GetJson(this object obj, ReprConfig config, HashSet<int>? visited,
        int depth)
    {
        var type = obj.GetType();
        visited ??= new HashSet<int>();
        var objHash = RuntimeHelpers.GetHashCode(o: obj);
        var json = new JsonObject();
        
        if (depth > 10)
        {
            json.Add(propertyName: "value", value: "Truncated for brevity.");
            return json;
        }

        // Handle primitives specially
        if (type.IsPrimitive || type == typeof(string))
        {
            var repr = obj.Repr(
                config: config with { FormattingMode = FormattingMode.Smart },
                visited: visited);
            if (obj is string str)
            {
                repr = repr.Substring(startIndex: 1, length: repr.Length - 2);
            }

            json.Add(propertyName: "value", value: repr);
            return json;
        }

        if (!type.IsValueType)
        {
            if (!visited.Add(item: objHash))
            {
                // json["type"] = $"CircularRef[{json["type"]}]";
                return json;
            }
        }

        // For enumerables
        switch (obj)
        {
            case Array array:
            {
                // Console.WriteLine(value: $"[{GetCallerMethod()}] {obj} is array");
                var rank = array.Rank;
                json.Add(propertyName: "elements",
                    value: array.GetJsonFromArrayRecursive(config: config, visited: visited,
                        indices: new int[rank], dimension: 0, depth: depth + 1));
                return json;
            }
            case ITuple tuple:
            {
                // Console.WriteLine(value: $"[{GetCallerMethod()}] {obj} is tuple");
                var entries = new JsonArray();
                for (var i = 0; i < tuple.Length; i++)
                {
                    entries.Add(value: tuple[index: i]
                      ?.GetJson(config: config, visited: visited, depth: depth + 1) ?? null);
                }

                json.Add(propertyName: "count", value: tuple.Length);
                json.Add(propertyName: "value", value: entries);
                return json;
            }
            case IDictionary dict:
            {
                // Console.WriteLine(value: $"[{GetCallerMethod()}] {obj} is list");
                var entries = new JsonArray();
                foreach (DictionaryEntry entry in dict)
                {
                    var entryJson = new JsonObject
                    {
                        [propertyName: "key"] = entry.Key?.GetJson(config: config,
                            visited: visited, depth: depth + 1),
                        [propertyName: "value"] = entry.Value?.GetJson(config: config,
                            visited: visited, depth: depth + 1)
                    };
                    entries.Add(value: entryJson);
                }

                json.Add(propertyName: "count", value: dict.Count);
                json.Add(propertyName: "value", value: entries);
                return json;
            }
            case IEnumerable list:
            {
                // Console.WriteLine(value: $"[{GetCallerMethod()}] {obj} is list");
                var jsonlist = new JsonArray();
                var count = 0;
                foreach (var item in list)
                {
                    count += 1;
                    jsonlist.Add(value: item.GetJson(config: config, visited: visited,
                        depth: depth + 1));
                }

                json.Add(propertyName: "count", value: count);
                json.Add(propertyName: "value", value: jsonlist);
                return json;
            }
        }


        // Get public fields
        var fields = type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(obj: obj);
            var addingValue = value.GetJson(config: config, visited: visited, depth: depth + 1);
            // Console.WriteLine(value: $"[{GetCallerMethod()}] {obj} {field.Name} {addingValue}");
            json.Add(propertyName: field.Name, value: addingValue);
        }

        // Get public properties with getters
        var properties = type
                        .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                        .Where(predicate: p => p is { CanRead: true, GetMethod.IsPublic: true });
        foreach (var prop in properties)
        {
            if (prop.Name is "Assembly" or "Module" or "StructLayoutAttribute")
            {
                continue;
            }

            if (prop.Name.Contains(value: "Assembly") ||
                prop.Name.StartsWith(value: "Is") && Char.IsUpper(c: prop.Name[index: 2]))
            {
                continue;
            }

            try
            {
                var value = prop.GetValue(obj: obj);
                var addingValue =
                    value.GetJson(config: config, visited: visited, depth: depth + 1);
                // Console.WriteLine(value: $"[{GetCallerMethod()}] {obj} {prop.Name} {addingValue}");
                json.Add(propertyName: prop.Name, value: addingValue);
            }
            catch (Exception ex)
            {
                // json.Add(propertyName: prop.Name, value: $"Error: {ex.Message}");
            }
        }

        if (!obj.GetType()
                .IsValueType)
        {
            visited.Remove(item: RuntimeHelpers.GetHashCode(o: obj));
        }

        return json;
    }

    public static JsonArray GetJsonFromArrayRecursive(this Array array, ReprConfig config,
        HashSet<int>? visited, int dimension, int[] indices,
        int depth = 0)
    {
        // Console.WriteLine(
        //     value: $"[{GetCallerMethod()}] {array} {array.Rank} {indices.Repr(config: null)}");
        if (dimension == array.Rank - 1)
        {
            // Last dimension - collect actual values
            var items = new JsonArray();
            for (var i = 0; i < array.GetLength(dimension: dimension); i++)
            {
                indices[dimension] = i;
                var value = array.GetValue(indices: indices);
                if (value is Array innerArray)
                {
                    // If the element is a jagged array, recurse directly to format its content
                    // without adding another "Array(...)" wrapper.
                    items.Add(item: innerArray.GetJsonFromArrayRecursive(
                        indices: new int[innerArray.Rank], dimension: 0,
                        config: config, visited: visited, depth: depth + 1));
                }
                else
                {
                    // Otherwise, format the element normally.
                    items.Add(
                        item: value?.GetJson(config: config, visited: visited, depth: depth + 1));
                }
            }

            return items;
        } // Not last dimension - recurse deeper

        var subArrays = new JsonArray();
        for (var i = 0; i < array.GetLength(dimension: dimension); i++)
        {
            indices[dimension] = i;
            subArrays.Add(item: array.GetJsonFromArrayRecursive(indices: indices,
                dimension: dimension + 1, config: config,
                visited: visited, depth: depth + 1));
        }

        return subArrays;
    }
}