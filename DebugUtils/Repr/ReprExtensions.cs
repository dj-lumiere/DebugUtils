#region

using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Formatters;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeLibraries;

#endregion

namespace DebugUtils.Repr;

public static partial class ReprExtensions
{
    public static string Repr<T>(this T obj, ReprConfig? config = null,
        HashSet<int>? visited = null)
    {
        config ??= ReprConfig.GlobalDefaults;
        visited ??= new HashSet<int>();

        if (config.FormattingMode == FormattingMode.ReflectionJson ||
            config.FormattingMode == FormattingMode.SmartJson)
        {
            config = config with { TypeMode = TypeReprMode.AlwaysHide };
        }

        if (obj.IsNullableStruct())
        {
            return obj.FormatNullableValueType(config: config);
        }

        if (obj is null)
        {
            return "null";
        }

        // 2. Handle circular references for reference types.
        if (!obj.GetType()
                .IsValueType)
        {
            var id = RuntimeHelpers.GetHashCode(o: obj);
            if (!visited.Add(item: id))
            {
                return $"<circular @{id:X8}>";
            }
        }

        // 3. Get the correct formatter from the registry.
        var formatter = ReprFormatterRegistry.GetFormatter(type: obj.GetType(), config: config);

        string result;
        // 4. Call the formatter with the correct arguments.
        if (formatter is { } reprFormatter)
        {
            result = reprFormatter.ToRepr(obj: obj, config: config, visited: visited);
        }
        else
        {
            result = obj.ToString() ?? "";
        }

        // 5. Cleanup and apply type prefix.
        try
        {
            return config.TypeMode switch
            {
                TypeReprMode.AlwaysHide => result,
                TypeReprMode.HideObvious => obj.NeedsTypePrefix()
                    ? $"{obj.GetReprTypeName()}({result})"
                    : result,
                _ => $"{obj.GetReprTypeName()}({result})"
            };
        }
        finally
        {
            if (!obj.GetType()
                    .IsValueType)
            {
                visited.Remove(item: RuntimeHelpers.GetHashCode(o: obj));
            }
        }
    }

    // This method remains as it is, correctly handling the logic for Nullable<T>.
    private static string FormatNullableValueType<T>(this T nullable, ReprConfig config)
    {
        var type = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(nullableType: type)!;
        var reprName = underlyingType.GetReprTypeNameByTypeName();

        // Handle JSON modes
        if (config.FormattingMode == FormattingMode.SmartJson ||
            config.FormattingMode == FormattingMode.ReflectionJson)
        {
            return nullable.FormatNullableAsJson(reprName: reprName,
                config: config with { TypeMode = TypeReprMode.AlwaysHide });
        }

        if (nullable == null)
        {
            return $"{reprName}?(null)";
        }

        var value = type.GetProperty(name: "Value")!.GetValue(obj: nullable)!;
        return
            $"{reprName}?({value.Repr(config: config with { TypeMode = TypeReprMode.AlwaysHide })})";
    }

    private static string FormatNullableAsJson<T>(this T nullable, string reprName,
        ReprConfig config)
    {
        if (nullable == null)
        {
            var nullJson = new JsonObject
            {
                [propertyName: "type"] = $"{reprName}?",
                [propertyName: "hasValue"] = false,
                [propertyName: "value"] = null
            };
            return nullJson.ToString();
        }

        var type = typeof(T);
        var value = type.GetProperty(name: "Value")!.GetValue(obj: nullable)!;
        var valueRepr = value.Repr(config: config with
        {
            TypeMode = TypeReprMode.AlwaysHide,
            FormattingMode =
            config.FormattingMode == FormattingMode.ReflectionJson ||
            config.FormattingMode == FormattingMode.Reflection
                ? FormattingMode.ReflectionJson
                : FormattingMode.SmartJson
        });
        Console.WriteLine(value: valueRepr);
        // Parse the JSON and extract the value part
        string innerValue;
        try
        {
            var parsed = JsonNode.Parse(json: valueRepr);
            if (parsed is JsonObject jsonObj && jsonObj.ContainsKey(propertyName: "value"))
            {
                // CLONE the node to avoid parent ownership issues
                var originalValue = jsonObj[propertyName: "value"];
                innerValue = originalValue?.ToString() ?? valueRepr;
            }
            else
            {
                // If no "value" field, clone the whole object
                innerValue = parsed?.ToString() ?? valueRepr;
            }
        }
        catch
        {
            // Fallback - shouldn't happen in pure JSON mode, but just in case
            innerValue = valueRepr;
        }

        Console.WriteLine(value: innerValue.GetType());
        Console.WriteLine(value: innerValue);

        var hasValueJson = new JsonObject
        {
            [propertyName: "type"] = $"{reprName}?",
            [propertyName: "hasValue"] = true,
            [propertyName: "value"] = innerValue
        };
        return hasValueJson.ToString();
    }
}