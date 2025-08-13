using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Extensions;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters;

/// <summary>
///     The default object pointer that handles any type not specifically registered.
///     It uses reflection to represent the record's public properties.
/// </summary>
[ReprOptions(needsPrefix: true)]
internal class ObjectFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        context = context.WithContainerConfig();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var type = obj.GetType();
        var parts = new List<string>();
        var shown = 0;
        var truncated = false;

        // Optionally include public instance fields declared on this type
        foreach (var f in type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                              .OrderBy(keySelector: f => f.Name))
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                shown >= context.Config.MaxPropertiesPerObject)
            {
                truncated = true;
                break;
            }

            parts.Add(
                item:
                $"{f.Name}: {f.GetValue(obj: obj).Repr(context: context.WithIncrementedDepth())}");
            shown += 1;
        }

        // Public, non-indexer properties (by name)
        var props = type
                   .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                   .Where(predicate: p => p.CanRead && p.GetIndexParameters()
                                                        .Length == 0 &&
                                          !p.Name.IsCompilerGeneratedName())
                   .ToDictionary(keySelector: p => p.Name, elementSelector: p => p);

        // Non-public instance fields for backing-field detection
        var nonPubInstFields = type
           .GetFields(bindingAttr: BindingFlags.NonPublic |
                                   BindingFlags.Instance);

        // Collect (prop, backingField) pairs
        var pairs = new List<(PropertyInfo prop, FieldInfo backing)>();
        foreach (var f in nonPubInstFields)
        {
            string propName;
            // Try auto-property backing fields only
            if (!f.TryGetAutoPropInfo(propName: out propName) &&
                !f.TryGetAnonymousInfo(propName: out propName))
            {
                continue;
            }

            if (!props.TryGetValue(key: propName, value: out var p))
            {
                continue;
            }

            pairs.Add(item: (p, f));
        }

        // Emit properties by reading their backing fields (NO getter calls)
        foreach (var (prop, backing) in pairs.OrderBy(keySelector: p => p.prop.Name))
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                shown >= context.Config.MaxPropertiesPerObject)
            {
                truncated = true;
                break;
            }

            var val = backing.GetValue(obj: obj);
            parts.Add(item: $"{prop.Name}: {val.Repr(context: context.WithIncrementedDepth())}");
            shown += 1;
        }

        // Non-public fields (only if requested), excluding backing fields and compiler-generated noise
        if (context.Config.ShowNonPublicProperties && (context.Config.MaxPropertiesPerObject < 0 ||
                                                       shown < context.Config
                                                          .MaxPropertiesPerObject))
        {
            var usedBackers =
                new HashSet<FieldInfo>(collection: pairs.Select(selector: p => p.backing));
            var privateProps = type
                              .GetProperties(bindingAttr: BindingFlags.NonPublic |
                                                          BindingFlags.Instance)
                              .Where(predicate: p => p.CanRead && p.GetIndexParameters()
                                                        .Length == 0 &&
                                                     !p.Name.IsCompilerGeneratedName())
                              .ToDictionary(keySelector: p => p.Name, elementSelector: p => p);
            var privatePair = new List<(PropertyInfo prop, FieldInfo backing)>();
            foreach (var f in nonPubInstFields.OrderBy(keySelector: f => f.Name))
            {
                if (usedBackers.Contains(item: f))
                {
                    continue;
                }

                var propName = "";

                if ((f.TryGetAutoPropInfo(propName: out propName) ||
                     f.TryGetAnonymousInfo(propName: out propName))
                    && privateProps.TryGetValue(key: propName, value: out var privateProp))
                {
                    privatePair.Add(item: (privateProp, f));
                    continue;
                }

                if (f.Name.IsCompilerGeneratedName() || f.Name == "EqualityContract")
                {
                    continue;
                }

                if (context.Config.MaxPropertiesPerObject >= 0 &&
                    shown >= context.Config.MaxPropertiesPerObject)
                {
                    truncated = true;
                    break;
                }

                parts.Add(
                    item:
                    $"private_{f.Name}: {f.GetValue(obj: obj).Repr(context: context.WithIncrementedDepth())}");
                shown += 1;
            }

            foreach (var (prop, backing) in privatePair.OrderBy(keySelector: p => p.prop.Name))
            {
                if (context.Config.MaxPropertiesPerObject >= 0 &&
                    shown >= context.Config.MaxPropertiesPerObject)
                {
                    truncated = true;
                    break;
                }

                var val = backing.GetValue(obj: obj);
                parts.Add(
                    item:
                    $"private_{prop.Name}: {val.Repr(context: context.WithIncrementedDepth())}");
                shown += 1;
            }
        }

        if (truncated)
        {
            parts.Add(item: "...");
        }

        return String.Join(separator: ", ", values: parts);
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        context = context.WithContainerConfig();
        var type = obj.GetType();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return type.CreateMaxDepthReachedJson(depth: context.Depth);
        }

        var result = new JsonObject();
        var shown = 0;
        var truncated = false;
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        if (!type.IsValueType)
        {
            result.Add(propertyName: "hashCode",
                value: $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}");
        }

        // Optionally include public instance fields declared on this type
        foreach (var f in type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                              .OrderBy(keySelector: f => f.Name))
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                shown >= context.Config.MaxPropertiesPerObject)
            {
                break;
            }

            var val = f.GetValue(obj: obj);
            result.Add(propertyName: f.Name,
                value: val.FormatAsJsonNode(context: context.WithIncrementedDepth()));
            shown += 1;
        }

        // Public, non-indexer properties (by name)
        var props = type
                   .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                   .Where(predicate: p => p.CanRead && p.GetIndexParameters()
                                                        .Length == 0 &&
                                          !p.Name.IsCompilerGeneratedName())
                   .ToDictionary(keySelector: p => p.Name, elementSelector: p => p);

        // Non-public instance fields for backing-field detection
        var nonPubInstFields = type
           .GetFields(bindingAttr: BindingFlags.NonPublic |
                                   BindingFlags.Instance);

        // Collect (prop, backingField) pairs
        var pairs = new List<(PropertyInfo prop, FieldInfo backing)>();
        foreach (var f in nonPubInstFields)
        {
            string propName;
            // Try auto-property backing fields only
            if (!f.TryGetAutoPropInfo(propName: out propName) &&
                !f.TryGetAnonymousInfo(propName: out propName))
            {
                continue;
            }

            if (!props.TryGetValue(key: propName, value: out var p))
            {
                continue;
            }

            pairs.Add(item: (p, f));
        }

        // Emit properties by reading their backing fields (NO getter calls)
        foreach (var (prop, backing) in pairs.OrderBy(keySelector: p => p.prop.Name))
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                shown >= context.Config.MaxPropertiesPerObject)
            {
                break;
            }

            var val = backing.GetValue(obj: obj);
            result.Add(propertyName: prop.Name,
                value: val.FormatAsJsonNode(context: context.WithIncrementedDepth()));
            shown += 1;
        }

        // Non-public fields (only if requested), excluding backing fields and compiler-generated noise
        if (context.Config.ShowNonPublicProperties && (context.Config.MaxPropertiesPerObject < 0 ||
                                                       shown < context.Config
                                                          .MaxPropertiesPerObject))
        {
            var usedBackers =
                new HashSet<FieldInfo>(collection: pairs.Select(selector: p => p.backing));
            var privateProps = type
                              .GetProperties(bindingAttr: BindingFlags.NonPublic |
                                                          BindingFlags.Instance)
                              .Where(predicate: p => p.CanRead && p.GetIndexParameters()
                                                        .Length == 0 &&
                                                     !p.Name.IsCompilerGeneratedName())
                              .ToDictionary(keySelector: p => p.Name, elementSelector: p => p);
            var privatePair = new List<(PropertyInfo prop, FieldInfo backing)>();
            foreach (var f in nonPubInstFields.OrderBy(keySelector: f => f.Name))
            {
                if (usedBackers.Contains(item: f))
                {
                    continue;
                }

                var propName = "";

                if ((f.TryGetAutoPropInfo(propName: out propName) ||
                     f.TryGetAnonymousInfo(propName: out propName))
                    && privateProps.TryGetValue(key: propName, value: out var privateProp))
                {
                    privatePair.Add(item: (privateProp, f));
                    continue;
                }

                if (f.Name.IsCompilerGeneratedName() || f.Name == "EqualityContract")
                {
                    continue;
                }

                if (context.Config.MaxPropertiesPerObject >= 0 &&
                    shown >= context.Config.MaxPropertiesPerObject)
                {
                    break;
                }

                var val = f.GetValue(obj: obj);
                result.Add(propertyName: $"private_{f.Name}",
                    value: val.FormatAsJsonNode(context: context.WithIncrementedDepth()));
                shown += 1;
            }

            foreach (var (prop, backing) in privatePair.OrderBy(keySelector: p => p.prop.Name))
            {
                if (context.Config.MaxPropertiesPerObject >= 0 &&
                    shown >= context.Config.MaxPropertiesPerObject)
                {
                    break;
                }

                var val = backing.GetValue(obj: obj);
                result.Add(propertyName: $"private_{prop.Name}",
                    value: val.FormatAsJsonNode(context: context.WithIncrementedDepth()));
                shown += 1;
            }
        }

        return result;
    }
}