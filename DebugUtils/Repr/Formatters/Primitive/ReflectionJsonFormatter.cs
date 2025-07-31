using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using static DebugUtils.CallStack;

namespace DebugUtils.Repr.Formatters.Primitive;

/// <summary>
///     The default object pointer that handles any type not specifically registered.
///     It uses reflection to represent the record's public properties.
/// </summary>
public class ReflectionJsonFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        config = config.GetContainerConfig() with { TypeMode = TypeReprMode.AlwaysHide };
        return obj.GetJson(config: config, visited: visited)
                  .ToString();
    }
}

internal static class JsonExtensions
{
    public static JsonObject GetJson(this object obj, ReprConfig config, HashSet<int>? visited)
    {
        var type = obj.GetType();
        Console.WriteLine(value: $"[{GetCallerMethod()}] {obj} {type}");
        var json = new JsonObject();

        json.Add(propertyName: "type", value: type.GetReprTypeNameByTypeName());

        // Handle primitives specially
        if (type.IsPrimitive || type == typeof(string))
        {
            json.Add(propertyName: "value", value: obj.Repr(
                config: config with { FormattingMode = FormattingMode.Smart },
                visited: visited));
        }

        if (type.IsArray)
        {

        }

        if (type is IEnumerable)
        {
            Console.WriteLine(value: $"[{GetCallerMethod()}] {obj} is IEnumerable");
            var list = (IEnumerable)obj;
            var jsonlist = new JsonObject();
            var idx = 0;
            foreach (var item in list)
            {
                jsonlist.Add(propertyName: $"{idx}",
                    value: item.GetJson(config: config, visited: visited));
            }

            json.Add(propertyName: "value", value: jsonlist);
        }

        // Get public fields
        var fields = type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(obj: obj);
            var addingValue = value.GetJson(config: config, visited: visited);
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
                var addingValue = value.GetJson(config: config, visited: visited);
                json.Add(propertyName: prop.Name, value: addingValue);
            }
            catch
            {
                json.Add(propertyName: prop.Name, value: "<error>");
            }
        }

        return json;
    }
}