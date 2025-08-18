using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Formatters;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.TypeHelpers;

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
/// <item><description><strong>Type Name Resolution:</strong> Methods for getting human-readable type names, handling generics, nullable types, and special .NET types</description></item>
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
/// <seealso cref="Repr{T}(T, ReprConfig?)"/>
/// <seealso cref="ReprTree{T}(T, ReprConfig?)"/>
/// <seealso cref="Repr{T}(Span{T}, ReprConfig?)"/>
/// <seealso cref="ReprTree{T}(Span{T}, ReprConfig?)"/>
/// <seealso cref="Repr{T}(ReadOnlySpan{T}, ReprConfig?)"/>
/// <seealso cref="ReprTree{T}(ReadOnlySpan{T}, ReprConfig?)"/>
/// <seealso cref="Repr{T}(T, ReprContext)"/>
/// <seealso cref="FormatAsJsonNode{T}(T, ReprContext)"/>
/// <seealso cref="Repr{T}(Span{T}, ReprContext)"/>
/// <seealso cref="FormatAsJsonNode{T}(Span{T}, ReprContext)"/>
/// <seealso cref="Repr{T}(ReadOnlySpan{T}, ReprContext)"/>
/// <seealso cref="FormatAsJsonNode{T}(ReadOnlySpan{T}, ReprContext)"/>
/// <seealso cref="TypeNaming.GetReprTypeName(Type)"/> 
/// <seealso cref="TypeNaming.GetReprTypeName{T}(T)"/>
/// <seealso cref="ReprConfig"/>
/// <seealso cref="ReprContext"/> 
/// <seealso cref="IReprFormatter"/>
/// <seealso cref="IReprTreeFormatter"/> 
public static class ReprExtensions
{
    #region End User API - Simple Configuration

    /// <summary>
    /// Generates a detailed string representation of any object using optional configuration.
    /// This is the primary method for end users who want to customize formatting behavior
    /// without dealing with internal state management.
    /// </summary>
    /// <typeparam name="T">The type of object to represent.</typeparam>
    /// <param name="obj">The object to represent. Can be null.</param>
    /// <param name="config">
    /// Optional configuration controlling formatting behavior. If null, uses default configuration.
    /// Contains settings for numeric formatting, type display, limits, and other formatting options.
    /// </param>
    /// <returns>
    /// A detailed string representation of the object with human-readable text and optional type prefixes.
    /// Circular references are detected and displayed appropriately.
    /// For tree-like JSON structure, use ReprTree() instead.
    /// </returns>
    /// <remarks>
    /// <para>This method is designed for end users and provides:</para>
    /// <list type="bullet">
    /// <item><description>Automatic circular reference detection and prevention</description></item>
    /// <item><description>Configurable formatting for numbers, floats, and containers</description></item>
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
    /// // Output: [1_i32, 2_i32, 3_i32]
    ///
    /// 
    /// // With custom configuration
    /// var config = new ReprConfig(FloatFormatString: "EX");
    /// Console.WriteLine(3.14f.Repr(config)); 
    /// // Output: 3.1400001049041748046875E+000_f32
    ///
    /// 
    /// // Nullable types
    /// int? nullable = 123;
    /// Console.WriteLine(nullable.Repr()); 
    /// // Output: int?(123)
    ///
    /// 
    /// // Circular reference detection
    /// var parent = new Node { Name = "Parent" };
    /// var child = new Node { Name = "Child", Parent = parent };
    /// parent.Child = child;
    /// Console.WriteLine(parent.Repr());
    /// // Output: Name: "Parent", Child: Name: "Child", Parent: &lt;Circular Reference to Node @0xA1B2C3D4&gt;
    /// </code>
    /// </example>
    /// <exception cref="StackOverflowException">
    /// Should not occur due to circular reference detection, but could theoretically happen
    /// with extremely deep object hierarchies exceeding system stack limits.
    /// </exception>
    public static string Repr<T>(this T obj, ReprConfig? config = null)
    {
        var context = config == null
            ? new ReprContext()
            : new ReprContext(config: config);


        if (context.Config.Culture is null)
        {
            context.Config = context.Config with { Culture = CultureInfo.CurrentCulture };
        }

        return obj.ToRepr(context: context);
    }
    /// <summary>
    /// Generates a hierarchical JSON representation of any object for structured analysis and debugging.
    /// This method produces detailed JSON output with complete type information, object relationships,
    /// and metadata suitable for debugging tools, IDEs, and automated analysis systems.
    /// </summary>
    /// <typeparam name="T">The type of object to represent.</typeparam>
    /// <param name="obj">The object to represent. Can be null.</param>
    /// <param name="config">
    /// Optional configuration controlling formatting behavior. If null, uses default configuration.
    /// Should be configured for hierarchical mode for optimal results.
    /// </param>
    /// <returns>
    /// A formatted JSON string representing the complete structure of the object, including:
    /// - Type information for all values
    /// - Object relationships and hierarchies  
    /// - Circular reference markers where detected
    /// - Null value representations with type context
    /// - Collection metadata (counts, truncation info)
    /// </returns>
    /// <remarks>
    /// <para><strong>⚠️ Important:</strong> This JSON is NOT intended for data serialization or transfer.
    /// It's designed to reveal the underlying object structure for debugging and analysis purposes.
    /// Use System.Text.Json, Newtonsoft.Json, or similar libraries for actual data serialization.</para>
    /// 
    /// <para>This method is the core of hierarchical formatting and provides:</para>
    /// <list type="bullet">
    /// <item><description><strong>Complete Type Information:</strong> Every value includes its .NET type</description></item>
    /// <item><description><strong>Structured Output:</strong> JSON format suitable for machine processing</description></item>
    /// <item><description><strong>Circular Reference Handling:</strong> Safe processing of self-referencing objects</description></item>
    /// <item><description><strong>Debugging Metadata:</strong> Additional context not available in standard JSON serialization</description></item>
    /// </list>
    /// <para>Unlike standard JSON serializers, this method preserves debugging information and handles
    /// circular references gracefully, making it ideal for development and diagnostic scenarios.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var person = new Person { Name = "John", Age = 30 };
    /// var config = new ReprConfig(EnablePrettyPrintForReprTree: true);
    /// var jsonResult = person.ReprTree(config);
    ///
    /// // jsonResult contains structured representation:
    /// // {
    /// //   "type": "Person",
    /// //   "kind": "class",
    /// //   "hashCode": "0xAAAAAAAA",
    /// //   "Name": { "type": "string", "kind": "class", "length": 4, "hashCode": "0xBBBBBBBB", "value": "John" },
    /// //   "Age": { "type": "int", "kind": "struct", "value": "30" }
    /// // }
    /// // hashCode can vary depending on when it got executed.
    /// </code>
    /// </example>
    public static string ReprTree<T>(this T obj, ReprConfig? config = null)
    {
        var context = config == null
            ? new ReprContext()
            : new ReprContext(config: config);
        var formatOption = new JsonSerializerOptions
        {
            WriteIndented = config?.EnablePrettyPrintForReprTree ?? false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        return obj.ToReprTree(context: context)
                  .ToJsonString(options: formatOption);
    }
    /// <summary>
    /// Generates a detailed string representation of a Span&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to represent.</param>
    /// <param name="config">Optional configuration. See <see cref="Repr{T}(T, ReprConfig?)"/> for details.</param>
    /// <returns>A string representation in the format "Span([element1, element2, ...])"</returns>
    /// <remarks>
    /// This method provides Span-specific formatting while maintaining consistency with the main Repr system.
    /// For detailed behavior and configuration options, see <see cref="ReprExtensions.Repr{T}(T, ReprConfig?)"/>.
    /// </remarks>
    /// <seealso cref="ReprExtensions.Repr{T}(T, ReprConfig?)"/>
    public static string Repr<T>(this Span<T> span, ReprConfig? config = null)
    {
        var context = config == null
            ? new ReprContext()
            : new ReprContext(config: config);
        var result = SpanFormatter.ToRepr(obj: span, context: context);

        // Apply type prefix
        return context.Config.TypeMode switch
        {
            TypeReprMode.AlwaysHide => result,
            _ => $"Span({result})"
        };
    }
    /// <summary>
    /// Generates a detailed string representation of a ReadOnlySpan&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to represent.</param>
    /// <param name="config">Optional configuration. See <see cref="Repr{T}(T, ReprConfig?)"/> for details.</param>
    /// <returns>A string representation in the format "ReadOnlySpan([element1, element2, ...])"</returns>
    /// <remarks>
    /// This method provides Span-specific formatting while maintaining consistency with the main Repr system.
    /// For detailed behavior and configuration options, see <see cref="ReprExtensions.Repr{T}(T, ReprConfig?)"/>.
    /// </remarks>
    /// <seealso cref="ReprExtensions.Repr{T}(T, ReprConfig?)"/>
    public static string Repr<T>(this ReadOnlySpan<T> span, ReprConfig? config = null)
    {
        var context = config == null
            ? new ReprContext()
            : new ReprContext(config: config);
        var result = ReadOnlySpanFormatter.ToRepr(obj: span, context: context);

        // Apply type prefix
        return context.Config.TypeMode switch
        {
            TypeReprMode.AlwaysHide => result,
            _ => $"ReadOnlySpan({result})"
        };
    }
    /// <summary>
    /// Generates a hierarchical JSON representation of a Span&lt;T&gt;
    /// </summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to represent.</param>
    /// <param name="config">Optional configuration. See <see cref="ReprTree{T}(T, ReprConfig?)"/> for details.</param>
    /// <returns>A JSON string with complete Span structure and metadata.</returns>
    /// <remarks>
    /// For JSON structure details and configuration options, see <see cref="ReprExtensions.ReprTree{T}(T, ReprConfig?)"/>.
    /// </remarks>
    /// <seealso cref="ReprExtensions.ReprTree{T}(T, ReprConfig?)"/>
    public static string ReprTree<T>(this Span<T> span, ReprConfig? config = null)
    {
        var context = config == null
            ? new ReprContext()
            : new ReprContext(config: config);
        var formatOption = new JsonSerializerOptions
        {
            WriteIndented = config?.EnablePrettyPrintForReprTree ?? false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        return SpanFormatter.ToReprTree(obj: span, context: context)
                            .ToJsonString(options: formatOption);
    }
    /// <summary>
    /// Generates a hierarchical JSON representation of a ReadOnlySpan&lt;T&gt;
    /// </summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to represent.</param>
    /// <param name="config">Optional configuration. See <see cref="ReprTree{T}(T, ReprConfig?)"/> for details.</param>
    /// <returns>A JSON string with complete Span structure and metadata.</returns>
    /// <remarks>
    /// For JSON structure details and configuration options, see <see cref="ReprExtensions.ReprTree{T}(T, ReprConfig?)"/>.
    /// </remarks>
    /// <seealso cref="ReprExtensions.ReprTree{T}(T, ReprConfig?)"/>
    public static string ReprTree<T>(this ReadOnlySpan<T> span, ReprConfig? config = null)
    {
        var context = config == null
            ? new ReprContext()
            : new ReprContext(config: config);
        var formatOption = new JsonSerializerOptions
        {
            WriteIndented = config?.EnablePrettyPrintForReprTree ?? false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        return ReadOnlySpanFormatter.ToReprTree(obj: span, context: context)
                                    .ToJsonString(options: formatOption);
    }

    #endregion

    #region Plugin/Formatter API - Advanced State Management

    /// <summary>
    /// Generates a detailed string representation using the provided context.
    /// This method is primarily intended for plugin developers and custom formatters
    /// who need precise control over state management and circular reference tracking.
    /// </summary>
    /// <typeparam name="T">The type of object to represent.</typeparam>
    /// <param name="obj">The object to represent. Can be null.</param>
    /// <param name="context">
    /// The context controlling formatting behavior and tracking state. Must not be null.
    /// Contains configuration settings, circular reference tracking, and depth management.
    /// </param>
    /// <returns>
    /// A detailed string representation of the object with human-readable text and optional type prefixes.
    /// Circular references are detected and displayed appropriately.
    /// </returns>
    /// <remarks>
    /// <para><strong>Target Audience:</strong> This method is designed for advanced users including:</para>
    /// <list type="bullet">
    /// <item><description>Custom formatter implementers (IReprFormatter)</description></item>
    /// <item><description>Plugin developers building on top of the Repr system</description></item>
    /// <item><description>Library authors who need precise state control</description></item>
    /// </list>
    /// <para><strong>State Management:</strong> The provided context maintains shared state across
    /// the entire representation operation, including visited object tracking and depth management.
    /// This enables proper circular reference detection and depth limiting in nested scenarios.</para>
    /// <para><strong>For End Users:</strong> Consider using the Repr(ReprConfig?) overload instead,
    /// which automatically manages context creation and cleanup.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In a custom formatter:
    /// public string ToRepr(object obj, ReprContext context)
    /// {
    ///     var person = (Person)obj;
    ///     var parts = new List&lt;string&gt;();
    ///     
    ///     // ✅ Always pass context to child object representations
    ///     parts.Add($"Name: {person.Name.Repr(context.WithIncrementedDepth())}");
    ///     parts.Add($"Manager: {person.Manager?.Repr(context.WithIncrementedDepth()) ?? "null"}");
    ///     
    ///     return string.Join(", ", parts);
    /// }
    /// 
    /// // Plugin usage:
    /// var sharedContext = new ReprContext(myConfig);
    /// var result1 = obj1.Repr(sharedContext);
    /// var result2 = obj2.Repr(sharedContext); // Shares circular reference tracking
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <seealso cref="Repr{T}(T, ReprConfig?)"/>
    public static string Repr<T>(this T obj, ReprContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: context);
        return obj.ToRepr(context: context);
    }
    /// <summary>
    /// Gets the JsonNode representation of an object for use in custom tree formatters.
    /// This method is primarily intended for developers implementing IReprTreeFormatter
    /// who need to build complex hierarchical structures with precise state control.
    /// </summary>
    /// <typeparam name="T">The type of object to represent.</typeparam>
    /// <param name="obj">The object to represent. Can be null - null objects are handled gracefully.</param>
    /// <param name="context">
    /// The context controlling formatting behavior and tracking state. Must not be null.
    /// Contains configuration settings, circular reference tracking, and depth management.
    /// </param>
    /// <returns>
    /// A JsonNode representation of the object with complete type information, structure,
    /// and metadata. This JsonNode can be incorporated into larger tree structures or
    /// converted to a JSON string as needed.
    /// </returns>
    /// <remarks>
    /// <para><strong>⚠️ Important:</strong> The resulting JsonNode is NOT intended for data serialization.
    /// It reveals the underlying object structure with debugging metadata and type information.</para>
    /// 
    /// <para><strong>Target Audience:</strong> This method is designed for advanced formatter developers who need to:</para>
    /// <list type="bullet">
    /// <item><description>Build custom tree formatters that include child objects</description></item>
    /// <item><description>Create complex nested JsonNode structures</description></item>
    /// <item><description>Access the raw JsonNode before string conversion</description></item>
    /// <item><description>Combine multiple objects into custom JSON representations</description></item>
    /// </list>
    /// <para><strong>State Management:</strong> The context parameter ensures proper circular reference
    /// detection and depth tracking when building nested structures. Always use 
    /// context.WithIncrementedDepth() when processing child objects.</para>
    /// <para><strong>For End Users:</strong> For simple JsonNode access, consider using the convenience
    /// overload that accepts ReprConfig? instead of requiring context management.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In a custom tree formatter:
    /// public JsonNode ToReprTree(object obj, ReprContext context)
    /// {
    ///     var container = (MyContainer)obj;
    ///     return new JsonObject
    ///     {
    ///         ["type"] = "MyContainer",
    ///         ["kind"] = "class",
    ///         ["Items"] = new JsonArray(
    ///             container.Items.Select(item => 
    ///                 item.FormatAsJsonNode(context.WithIncrementedDepth())
    ///             ).ToArray()
    ///         ),
    ///         ["Metadata"] = container.Meta.FormatAsJsonNode(context.WithIncrementedDepth())
    ///     };
    /// }
    /// 
    /// // Plugin building custom structure:
    /// var context = new ReprContext(config);
    /// var customStructure = new JsonObject
    /// {
    ///     ["timestamp"] = DateTime.Now.ToString(),
    ///     ["data"] = myObject.FormatAsJsonNode(context),
    ///     ["debug"] = debugInfo.FormatAsJsonNode(context)
    /// };
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <seealso cref="ReprTree{T}(T, ReprConfig?)"/>
    public static JsonNode FormatAsJsonNode<T>(this T obj, ReprContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: context);
        return obj.ToReprTree(context: context);
    }
    /// <summary>
    /// Generates a detailed string representation of a Span&lt;T&gt; using the provided context.
    /// This method is intended for plugin developers and custom formatters who need precise control 
    /// over state management and circular reference tracking.
    /// </summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to represent.</param>
    /// <param name="context">
    /// The context controlling formatting behavior and tracking state. Must not be null.
    /// Contains configuration settings, circular reference tracking, and depth management.
    /// </param>
    /// <returns>A string representation showing Span structure and contents.</returns>
    /// <remarks>
    /// <para><strong>Target Audience:</strong> This method is designed for advanced users including:</para>
    /// <list type="bullet">
    /// <item><description>Custom formatter implementers</description></item>
    /// <item><description>Plugin developers building on top of the Repr system</description></item>
    /// <item><description>Library authors who need precise state control</description></item>
    /// </list>
    /// <para>For end users, consider using <see cref="Repr{T}(Span{T}, ReprConfig?)"/> instead.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <seealso cref="Repr{T}(Span{T}, ReprConfig?)"/>
    /// <seealso cref="Repr{T}(T, ReprContext)"/>
    public static string Repr<T>(this Span<T> span, ReprContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: context);
        var result = SpanFormatter.ToRepr(obj: span, context: context);

        // Apply type prefix
        return context.Config.TypeMode switch
        {
            TypeReprMode.AlwaysHide => result,
            _ => $"ReadOnlySpan({result})"
        };
    }
    /// <summary>
    /// Generates a detailed string representation of a ReadOnlySpan&lt;T&gt; using the provided context.
    /// This method is intended for plugin developers and custom formatters who need precise control 
    /// over state management and circular reference tracking.
    /// </summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to represent.</param>
    /// <param name="context">
    /// The context controlling formatting behavior and tracking state. Must not be null.
    /// Contains configuration settings, circular reference tracking, and depth management.
    /// </param>
    /// <returns>A string representation showing Span structure and contents.</returns>
    /// <remarks>
    /// <para><strong>Target Audience:</strong> This method is designed for advanced users including:</para>
    /// <list type="bullet">
    /// <item><description>Custom formatter implementers</description></item>
    /// <item><description>Plugin developers building on top of the Repr system</description></item>
    /// <item><description>Library authors who need precise state control</description></item>
    /// </list>
    /// <para>For end users, consider using <see cref="Repr{T}(ReadOnlySpan{T}, ReprConfig?)"/> instead.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <seealso cref="Repr{T}(ReadOnlySpan{T}, ReprConfig?)"/>
    /// <seealso cref="Repr{T}(T, ReprContext)"/>
    public static string Repr<T>(this ReadOnlySpan<T> span, ReprContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: context);
        var result = ReadOnlySpanFormatter.ToRepr(obj: span, context: context);

        // Apply type prefix
        return context.Config.TypeMode switch
        {
            TypeReprMode.AlwaysHide => result,
            _ => $"ReadOnlySpan({result})"
        };
    }
    /// <summary>
    /// Gets the JsonNode representation of a Span&lt;T&gt; for use in custom tree formatters.
    /// This method is intended for developers implementing custom formatters who need to 
    /// build complex hierarchical structures with precise state control.
    /// </summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to represent.</param>
    /// <param name="context">
    /// The context controlling formatting behavior and tracking state. Must not be null.
    /// Contains configuration settings, circular reference tracking, and depth management.
    /// </param>
    /// <returns>
    /// A JsonNode representation with complete type information and structure.
    /// This can be incorporated into larger tree structures or converted to JSON as needed.
    /// </returns>
    /// <remarks>
    /// <para><strong>Target Audience:</strong> This method is designed for advanced formatter developers who need to:</para>
    /// <list type="bullet">
    /// <item><description>Build custom tree formatters that include child objects</description></item>
    /// <item><description>Create complex nested JsonNode structures</description></item>
    /// <item><description>Access the raw JsonNode before string conversion</description></item>
    /// </list>
    /// <para>For end users, consider using <see cref="ReprTree{T}(Span{T}, ReprConfig?)"/> instead.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <seealso cref="ReprTree{T}(Span{T}, ReprConfig?)"/>
    /// <seealso cref="FormatAsJsonNode{T}(T, ReprContext)"/>
    public static JsonNode FormatAsJsonNode<T>(this Span<T> span, ReprContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: context);
        return SpanFormatter.ToReprTree(obj: span, context: context);
    }
    /// <summary>
    /// Gets the JsonNode representation of a ReadOnlySpan&lt;T&gt; for use in custom tree formatters.
    /// This method is intended for developers implementing custom formatters who need to 
    /// build complex hierarchical structures with precise state control.
    /// </summary>
    /// <typeparam name="T">The element type of the span.</typeparam>
    /// <param name="span">The span to represent.</param>
    /// <param name="context">
    /// The context controlling formatting behavior and tracking state. Must not be null.
    /// Contains configuration settings, circular reference tracking, and depth management.
    /// </param>
    /// <returns>
    /// A JsonNode representation with complete type information and structure.
    /// This can be incorporated into larger tree structures or converted to JSON as needed.
    /// </returns>
    /// <remarks>
    /// <para><strong>Target Audience:</strong> This method is designed for advanced formatter developers who need to:</para>
    /// <list type="bullet">
    /// <item><description>Build custom tree formatters that include child objects</description></item>
    /// <item><description>Create complex nested JsonNode structures</description></item>
    /// <item><description>Access the raw JsonNode before string conversion</description></item>
    /// </list>
    /// <para>For end users, consider using <see cref="ReprTree{T}(ReadOnlySpan{T}, ReprConfig?)"/> instead.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <seealso cref="ReprTree{T}(ReadOnlySpan{T}, ReprConfig?)"/>
    /// <seealso cref="FormatAsJsonNode{T}(T, ReprContext)"/>
    public static JsonNode FormatAsJsonNode<T>(this ReadOnlySpan<T> span, ReprContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: context);
        return ReadOnlySpanFormatter.ToReprTree(obj: span, context: context);
    }

    #endregion
}