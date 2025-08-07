using System.Reflection;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Generic;

/// <summary>
///     A generic formatter for any record type.
///     It uses reflection to represent the record's public properties.
/// </summary>
[ReprOptions(needsPrefix: true)]
internal class RecordFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var type = obj.GetType();
        var parts = new List<string>();
        context = context.WithContainerConfig();

        // Get public properties with getters
        var properties = type
                        .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                        .Where(predicate: p => p is
                                                   { CanRead: true, GetMethod.IsPublic: true } &&
                                               !p.Name.IsCompilerGeneratedName());
        var propertyCount = 0;
        var isTruncated = false;

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
                // Recursively call the main Repr method for the value!
                parts.Add(
                    item: $"{prop.Name}: {value.Repr(context: context.WithIncrementedDepth())}");
            }
            catch (Exception ex)
            {
                parts.Add(item: $"{prop.Name}: <Error: {ex.Message}>");
            }

            propertyCount += 1;
        }

        if (!context.Config.ShowNonPublicProperties)
        {
            if (isTruncated)
            {
                parts.Add(item: "...");
            }

            return $"{{ {String.Join(separator: ", ", values: parts)} }}";
        }

        // Get private fields
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

            var fieldName = field.Name;
            var value = field.GetValue(obj: obj);
            var addingValue = value.Repr(context: context.WithIncrementedDepth());
            parts.Add(item: $"private_{fieldName}: {addingValue}");
            propertyCount += 1;
        }

        if (isTruncated)
        {
            parts.Add(item: "...");
        }

        return $"{{ {String.Join(separator: ", ", values: parts)} }}";
    }
}