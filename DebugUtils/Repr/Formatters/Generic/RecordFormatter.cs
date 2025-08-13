using System.Reflection;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Extensions;
using DebugUtils.Repr.Interfaces;

namespace DebugUtils.Repr.Formatters;

/// <summary>
///     A generic formatter for any record type.
///     It uses reflection to represent the record's public properties.
/// </summary>
[ReprOptions(needsPrefix: true)]
internal class RecordFormatter : IReprFormatter
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

        // Optionally include public instance fields declared on this type (rare on records)
        foreach (var f in type.GetFields(bindingAttr: BindingFlags.Public | BindingFlags.Instance |
                                                      BindingFlags.DeclaredOnly))
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
                   .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance |
                                               BindingFlags.DeclaredOnly)
                   .Where(predicate: p => p.CanRead && p.GetIndexParameters()
                                                        .Length == 0 &&
                                          !p.Name.IsCompilerGeneratedName())
                   .ToDictionary(keySelector: p => p.Name, elementSelector: p => p);

        // Non-public instance fields for backing-field detection
        var nonPubInstFields = type.GetFields(bindingAttr: BindingFlags.NonPublic |
                                                           BindingFlags.Instance |
                                                           BindingFlags.DeclaredOnly);

        // Collect (prop, backingField) pairs
        var pairs = new List<(PropertyInfo prop, FieldInfo backing)>();
        foreach (var f in nonPubInstFields)
        {
            string propName;
            // Try both auto-property and anonymous type backing fields
            if (!f.TryGetAutoPropInfo(propName: out propName))
            {
                continue;
            }

            if (!props.TryGetValue(key: propName, value: out var p))
            {
                continue;
            }

            pairs.Add(item: (p, f));
        }

        // Stable order (close to ctor order for records)
        pairs.Sort(comparison: (a, b) =>
            a.prop.MetadataToken.CompareTo(value: b.prop.MetadataToken));

        // Emit properties by reading their backing fields (NO getter calls)
        foreach (var (prop, backing) in pairs)
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
            foreach (var f in nonPubInstFields)
            {
                if (usedBackers.Contains(item: f))
                {
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
        }

        if (truncated)
        {
            parts.Add(item: "...");
        }

        return $"{{ {String.Join(separator: ", ", values: parts)} }}";
    }
}