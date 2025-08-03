using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Formatters.Functions;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Fallback;

internal static class HierarchicalObjectExtensions
{
    public static JsonObject ToJsonObject(this object obj, ReprConfig config,
        HashSet<int> visited,
        int depth)
    {
        var type = obj.GetType();
        var objHash = RuntimeHelpers.GetHashCode(o: obj);
        var json = new JsonObject();

        json.Add(propertyName: "type", value: type.GetReprTypeName());

        if (depth > 5)
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
                var result = new JsonObject();
                result.Add(propertyName: "type", value: type.GetReprTypeName());
                result.Add(propertyName: "hashCode", value: $"0x{objHash:X8}");
                json["type"] = "CircularReference";
                json.Add("target", result);
                return json;
            }
        }

        // For enumerables
        switch (obj)
        {
            case Array array:
            {
                var rank = array.Rank;
                json.Add(propertyName: "elements",
                    value: array.GetJsonFromArrayRecursive(config: config, visited: visited,
                        indices: new int[rank], dimension: 0, depth: depth + 1));
                return json;
            }
            case ITuple tuple:
            {
                var entries = new JsonArray();
                for (var i = 0; i < tuple.Length; i++)
                {
                    entries.Add(value: tuple[index: i]
                      ?.ToJsonObject(config: config, visited: visited, depth: depth + 1) ?? null);
                }

                json.Add(propertyName: "count", value: tuple.Length);
                json.Add(propertyName: "value", value: entries);
                return json;
            }
            case IDictionary dict:
            {
                var entries = new JsonArray();
                foreach (DictionaryEntry entry in dict)
                {
                    var entryJson = new JsonObject
                    {
                        [propertyName: "key"] = entry.Key.ToJsonObject(config: config,
                            visited: visited, depth: depth + 1),
                        [propertyName: "value"] = entry.Value?.ToJsonObject(config: config,
                            visited: visited, depth: depth + 1) ?? null
                    };
                    entries.Add(value: entryJson);
                }

                json.Add(propertyName: "count", value: dict.Count);
                json.Add(propertyName: "value", value: entries);
                return json;
            }
            case IEnumerable list:
            {
                var jsonlist = new JsonArray();
                var count = 0;
                foreach (var item in list)
                {
                    count += 1;
                    jsonlist.Add(value: item?.ToJsonObject(config: config, visited: visited,
                        depth: depth + 1) ?? null);
                }

                json.Add(propertyName: "count", value: count);
                json.Add(propertyName: "value", value: jsonlist);
                return json;
            }
            case Delegate del:
            {
                var functionDetails = del.Method.ToFunctionDetails();
                json[propertyName: "type"] = "Function";
                json.Add(propertyName: "properties",
                    value: functionDetails.ToJsonObject(config: config, visited: visited,
                        depth: depth + 1));
                return json;
            }
        }

        // Get public fields
        var fields = type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(obj: obj);
            var addingValue =
                value?.ToJsonObject(config: config, visited: visited, depth: depth + 1)
                ?? null;
            json.Add(propertyName: field.Name, value: addingValue);
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
                var addingValue =
                    value?.ToJsonObject(config: config, visited: visited, depth: depth + 1) ??
                    null;
                json.Add(propertyName: prop.Name, value: addingValue);
            }
            catch (Exception ex)
            {
                json.Add(propertyName: prop.Name, value: $"Error: {ex.Message}");
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
                        item: value?.ToJsonObject(config: config, visited: visited,
                            depth: depth + 1));
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