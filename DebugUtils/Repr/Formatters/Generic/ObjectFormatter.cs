using System.Reflection;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters.Generic;

/// <summary>
///     The default object pointer that handles any type not specifically registered.
///     It uses reflection to represent the record's public properties.
/// </summary>
[ReprOptions(needsPrefix: true)]
internal class ObjectFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var type = obj.GetType();
        var parts = new List<string>();
        var accessedFieldNames = new HashSet<string>();
        var propertyCount = 0;
        var isTruncated = false;
        var content = "";
        context = context.WithContainerConfig();
        // Get public fields
        var fields = type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                propertyCount >= context.Config.MaxPropertiesPerObject)
            {
                isTruncated = true;
                break;
            }

            var value = field.GetValue(obj: obj);
            parts.Add(
                item: $"{field.Name}: {value.Repr(context: context.WithIncrementedDepth())}");
            accessedFieldNames.Add(item: field.Name);
            propertyCount += 1;
        }

        // Get public properties with getters
        var properties = type
                        .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                        .Where(predicate: p => p is { CanRead: true, GetMethod.IsPublic: true });
        foreach (var prop in properties)
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                propertyCount >= context.Config.MaxPropertiesPerObject)
            {
                isTruncated = true;
                break;
            }

            try
            {
                var value = prop.GetValue(obj: obj);
                parts.Add(
                    item: $"{prop.Name}: {value.Repr(context: context.WithIncrementedDepth())}");
            }
            catch (Exception ex)
            {
                parts.Add(item: $"{prop.Name}: <error {ex.Message}>");
            }

            accessedFieldNames.Add(item: prop.Name);
            propertyCount += 1;
        }

        if (!context.Config.ShowNonPublicProperties)
        {
            if (isTruncated)
            {
                parts.Add(item: "...");
            }

            content = parts.Count > 0
                ? String.Join(separator: ", ", values: parts)
                : "";
            return $"{content}";
        }

        var nonPublicFields =
            type.GetFields(bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance |
                                        BindingFlags.Static)
                .Where(predicate: p => !p.Name.IsCompilerGeneratedName());
        foreach (var field in nonPublicFields)
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                propertyCount >= context.Config.MaxPropertiesPerObject)
            {
                isTruncated = true;
                break;
            }

            var value = field.GetValue(obj: obj);
            var addingValue =
                value?.Repr(context: context.WithIncrementedDepth())
                ?? "null";
            var fieldName = field.Name;

            if (accessedFieldNames.Contains(item: fieldName))
            {
                continue; // Skip but don't count
            }

            parts.Add(item: $"private_{fieldName}: {addingValue}");
            propertyCount += 1;
        }

        // Get public properties with getters
        var nonPublicProperties = type
                                 .GetProperties(
                                      bindingAttr: BindingFlags.NonPublic |
                                                   BindingFlags.Instance)
                                 .Where(predicate: p => p is
                                                        {
                                                            CanRead: true,
                                                            GetMethod.IsPublic: false
                                                        } &&
                                                        !p.Name.IsCompilerGeneratedName());
        foreach (var prop in nonPublicProperties)
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                propertyCount >= context.Config.MaxPropertiesPerObject)
            {
                break;
            }

            var propName = prop.Name;

            if (accessedFieldNames.Contains(item: propName))
            {
                continue; // Skip but don't count
            }

            try
            {
                var value = prop.GetValue(obj: obj);
                var addingValue =
                    value?.Repr(context: context.WithIncrementedDepth())
                    ?? "null";
                parts.Add(item: $"private_{propName}: {addingValue}");
            }
            catch (Exception ex)
            {
                parts.Add(item: $"private_{propName}: {ex.Message}");
            }
            propertyCount += 1;
        }

        if (isTruncated)
        {
            parts.Add(item: "...");
        }

        content = parts.Count > 0
            ? String.Join(separator: ", ", values: parts)
            : "";
        return $"{content}";
    }


    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var type = obj.GetType();

        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = type.GetReprTypeName(),
                [propertyName: "kind"] = type.GetTypeKind(),
                [propertyName: "maxDepthReached"] = true,
                [propertyName: "depth"] = context.Depth
            };
        }

        var result = new JsonObject();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        var propertyCount = 0;
        // Get public fields
        var fields =
            type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance |
                                        BindingFlags.Static);
        foreach (var field in fields)
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                propertyCount >= context.Config.MaxPropertiesPerObject)
            {
                break;
            }

            var value = field.GetValue(obj: obj);
            var addingValue = value?.FormatAsJsonNode(context: context.WithIncrementedDepth())
                              ?? null;
            result.Add(propertyName: field.Name, value: addingValue);
            propertyCount += 1;
        }

        // Get public properties with getters
        var properties = type
                        .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance |
                                                    BindingFlags.Static)
                        .Where(predicate: p => p is { CanRead: true, GetMethod.IsPublic: true });
        foreach (var prop in properties)
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                propertyCount >= context.Config.MaxPropertiesPerObject)
            {
                break;
            }
            
            try
            {
                var value = prop.GetValue(obj: obj);
                var addingValue =
                    value?.FormatAsJsonNode(context: context.WithIncrementedDepth()) ??
                    null;
                result.Add(propertyName: prop.Name, value: addingValue);
            }
            catch (Exception ex)
            {
                result.Add(propertyName: prop.Name, value: $"Error: {ex.Message}");
            }
            propertyCount += 1;
        }

        if (!context.Config.ShowNonPublicProperties)
        {
            return result;
        }

        var nonPublicFields =
            type.GetFields(bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance |
                                        BindingFlags.Static)
                .Where(predicate: p => !p.Name.IsCompilerGeneratedName());
        foreach (var field in nonPublicFields)
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                propertyCount >= context.Config.MaxPropertiesPerObject)
            {
                break;
            }

            // The compiler-generated "EqualityContract" property is not useful for representation.
            if (field.Name == "EqualityContract")
            {
                continue;
            }

            var value = field.GetValue(obj: obj);
            var addingValue =
                value?.FormatAsJsonNode(context: context.WithIncrementedDepth())
                ?? null;
            var fieldName = field.Name;
            result.Add(propertyName: $"private_{fieldName}", value: addingValue);
            propertyCount += 1;
        }

        // Get public properties with getters
        var nonPublicProperties = type
                                 .GetProperties(
                                      bindingAttr: BindingFlags.NonPublic |
                                                   BindingFlags.Instance | BindingFlags.Static)
                                 .Where(predicate: p =>
                                      p.CanRead && !p.Name.IsCompilerGeneratedName());
        foreach (var prop in nonPublicProperties)
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                propertyCount >= context.Config.MaxPropertiesPerObject)
            {
                break;
            }

            try
            {
                var value = prop.GetValue(obj: obj);
                var addingValue =
                    value?.FormatAsJsonNode(context: context.WithIncrementedDepth()) ?? null;
                result.Add(propertyName: $"private_{prop.Name}", value: addingValue);
            }
            catch (Exception ex)
            {
                result.Add(propertyName: $"private_{prop.Name}", value: $"Error: {ex.Message}");
            }
            propertyCount += 1;
        }

        return result;
    }
}

internal static class ObjectExtensions
{
    public static bool IsCompilerGeneratedName(this string fieldName)
    {
        return fieldName.Contains(value: "k__BackingField") || // Auto-property backing fields
               fieldName.StartsWith(value: "<") || // Most compiler-generated fields
               fieldName.Contains(value: "__") || // Various compiler patterns
               fieldName.Contains(value: "DisplayClass") || // Closure fields
               fieldName.EndsWith(value: "__0") || // State machine fields
               fieldName.Contains(value: "c__Iterator") || // Iterator fields
               fieldName == "EqualityContract"; // Record EqualityContract
    }
}