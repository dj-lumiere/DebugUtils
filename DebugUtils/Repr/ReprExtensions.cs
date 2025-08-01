using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Formatters;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeLibraries;

namespace DebugUtils.Repr;

/// <summary>
/// Provides extension methods for generating detailed string representations of .NET objects.
/// This class contains the core functionality for the Repr system, offering Python-like repr() 
/// capabilities with extensive customization options for debugging, logging, and diagnostic purposes.
/// </summary>
/// <remarks>
/// <para>The ReprExtensions class serves as the main entry point for object representation functionality.
/// It combines several key areas of functionality:</para>
/// <list type="bullet">
/// <item><description><strong>Object Representation:</strong> The primary Repr() extension method that converts any object to a detailed string representation</description></item>
/// <item><description><strong>Type Name Resolution:</strong> Methods for obtaining human-readable type names, handling generics, nullable types, and special .NET types</description></item>
/// <item><description><strong>Configuration Management:</strong> Type mappings and configuration utilities for controlling formatting behavior</description></item>
/// </list>
/// 
/// <para><b>Key Features:</b></para>
/// <list type="bullet">
/// <item><description>Automatic circular reference detection and prevention</description></item>
/// <item><description>Configurable formatting for numeric types, floats, and containers</description></item>
/// <item><description>Support for hierarchical JSON output mode</description></item>
/// <item><description>Extensible formatter registry system</description></item>
/// <item><description>Thread-safe operation with per-call state isolation</description></item>
/// <item><description>Comprehensive type system integration</description></item>
/// </list>
/// 
/// <para><strong>Performance Characteristics:</strong></para>
/// <list type="bullet">
/// <item><description>Optimized type name lookups using static dictionaries</description></item>
/// <item><description>Efficient circular reference detection using RuntimeHelpers.GetHashCode</description></item>
/// <item><description>Minimal allocations for simple object representations</description></item>
/// <item><description>Automatic cleanup prevents memory leaks</description></item>
/// </list>
/// 
/// <para><strong>Extensibility:</strong></para>
/// <list type="bullet">
/// <item><description>Custom formatters can be registered via IReprFormatter interface</description></item>
/// <item><description>Attribute-based configuration using ReprOptionsAttribute</description></item>
/// <item><description>Configurable container handling strategies</description></item>
/// <item><description>Support for custom type name mappings</description></item>
/// </list>
/// </remarks>
/// <seealso cref="Repr{T}(T, ReprConfig?, HashSet{int}?)"/>
/// <seealso cref="GetReprTypeName{T}(T)"/>
/// <seealso cref="GetReprTypeName(Type)"/>
/// <seealso cref="GetContainerConfig(ReprConfig)"/>
/// <seealso cref="ReprConfig"/>
/// <seealso cref="IReprFormatter"/>
public static partial class ReprExtensions
{
    /// <summary>
    /// Generates a detailed string representation of any object with configurable formatting options.
    /// Similar to Python's repr() function, this method provides unambiguous object representations 
    /// for debugging, logging, and diagnostic purposes.
    /// </summary>
    /// <typeparam name="T">The type of object to represent.</typeparam>
    /// <param name="obj">The object to represent. Can be null.</param>
    /// <param name="config">
    /// Optional configuration controlling formatting behavior. If null, uses GlobalDefaults.
    /// Controls aspects like numeric formatting, type display, and output mode.
    /// </param>
    /// <param name="visited">
    /// Optional set containing hash codes of objects currently being processed.
    /// Used internally for circular reference detection. Should typically be null for external calls.
    /// </param>
    /// <returns>
    /// A detailed string representation of the object. The format depends on the object type
    /// and configuration settings. Circular references are detected and displayed as 
    /// "&lt;Circular Reference to TypeName @HashCode&gt;".
    /// </returns>
    /// <remarks>
    /// <para>Key features:</para>
    /// <list type="bullet">
    /// <item><description>Automatic circular reference detection and prevention</description></item>
    /// <item><description>Configurable formatting for numbers, floats, and containers</description></item>
    /// <item><description>Support for hierarchical JSON output mode</description></item>
    /// <item><description>Extensible formatter registry system</description></item>
    /// <item><description>Special handling for nullable types</description></item>
    /// <item><description>Thread-safe operation with per-call state isolation</description></item>
    /// </list>
    /// <para>Performance considerations:</para>
    /// <list type="bullet">
    /// <item><description>Uses RuntimeHelpers.GetHashCode for object identity</description></item>
    /// <item><description>Maintains a visited set only for reference types</description></item>
    /// <item><description>Automatic cleanup prevents memory leaks</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic usage
    /// var list = new List&lt;int&gt; { 1, 2, 3 };
    /// Console.WriteLine(list.Repr()); 
    /// // Output: [int(1), int(2), int(3)]
    /// 
    /// // With custom configuration
    /// var config = new ReprConfig(FloatMode: FloatReprMode.Exact);
    /// Console.WriteLine(3.14f.Repr(config)); 
    /// // Output: float(3.1400001049041748046875E0)
    /// 
    /// // Nullable types
    /// int? nullable = 123;
    /// Console.WriteLine(nullable.Repr()); 
    /// // Output: int?(123)
    /// 
    /// // Circular reference detection
    /// var parent = new Children { Name = "Parent" };
    /// var child = new Children { Name = "Child", Parent = parent };
    /// parent.Parent = child;
    /// Console.WriteLine(parent.Repr());
    /// // Output: Person(Name: "Parent", Children: [Person(Name: "Child", Parent: &lt;Circular Reference to Person @A1B2C3D4&gt;)])
    /// // "A1B2C3D4" part can be different
    /// 
    /// // Hierarchical JSON mode
    /// var jsonConfig = new ReprConfig(FormattingMode: FormattingMode.Hierarchical);
    /// var obj = new { Name = "John", Age = 30 };
    /// Console.WriteLine(obj.Repr(jsonConfig));
    /// // Output: "{"type":"Anonymous","Name":{"type":"string","value":"John"},"Age":{"type":"int","value":"30"}}"
    /// // (removed any whitespaces for brevity)
    /// </code>
    /// </example>
    /// <exception cref="StackOverflowException">
    /// Should not occur due to circular reference detection, but could theoretically happen
    /// with extremely deep object hierarchies exceeding system stack limits.
    /// </exception>
    public static string Repr<T>(this T obj, ReprConfig? config = null,
        HashSet<int>? visited = null)
    {
        config ??= ReprConfig.GlobalDefaults;
        visited ??= new HashSet<int>();
        var id = RuntimeHelpers.GetHashCode(o: obj);

        if (config.FormattingMode == FormattingMode.Hierarchical)
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
            if (!visited.Add(item: id))
            {
                return $"<Circular Reference to {obj.GetReprTypeName()} @{id:X8}>";
            }
        }

        // 3. Get the correct formatter from the registry.
        var formatter = ReprFormatterRegistry.GetFormatter(type: obj.GetType(), config: config);

        string result;
        // 4. Call the formatter with the correct arguments.

        if (formatter is { } reprFormatter)
        {
            if (config.FormattingMode == FormattingMode.Hierarchical)
            {
                result = obj.InvokeHierarchicalFormatter(config: config, visited: visited, formatter: reprFormatter,
                    id: id);
            }

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

    private static string InvokeHierarchicalFormatter<T>(this T obj, ReprConfig config, HashSet<int> visited,
        IReprFormatter formatter, int id)
    {
        // prevent immediate circular reference crashing
        if (!obj?.GetType()
                 .IsValueType ?? false)
        {
            visited.Remove(item: id);
        }

        // Since I handled null checking at the first stage of repr.
        var result = formatter.ToRepr(obj: obj!, config: config, visited: visited);
        visited.Add(item: id);
        return result;
    }

    // This method remains as it is, correctly handling the logic for Nullable<T>.
    private static string FormatNullableValueType<T>(this T nullable, ReprConfig config)
    {
        var type = typeof(T);
        var reprName = type.GetReprTypeName();

        // Handle JSON modes
        if (config.FormattingMode == FormattingMode.Hierarchical)
        {
            return nullable.FormatNullableAsHierarchical(reprName: reprName,
                config: config with { TypeMode = TypeReprMode.AlwaysHide });
        }

        if (nullable == null)
        {
            return $"{reprName}(null)";
        }

        var value = type.GetProperty(name: "Value")!.GetValue(obj: nullable)!;
        return
            $"{reprName}({value.Repr(config: config with { TypeMode = TypeReprMode.AlwaysHide })})";
    }

    private static string FormatNullableAsHierarchical<T>(this T nullable, string reprName,
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
            FormattingMode = FormattingMode.Hierarchical
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