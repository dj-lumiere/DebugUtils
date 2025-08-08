using System.Text.Json.Nodes;

namespace DebugUtils.Repr.Interfaces;

/// <summary>
/// Defines the contract for custom object formatters in the Repr system.
/// Implement this interface to create specialized formatting logic for specific types.
/// </summary>
/// <remarks>
/// <para>Formatters are registered with the ReprFormatterRegistry and automatically
/// invoked when objects of their target types are encountered during representation.</para>
/// <para>Custom formatters provide full control over how objects are represented,
/// including the ability to inspect configuration settings and handle circular references.</para>
/// </remarks>
/// <example>
/// <code>
/// [ReprFormatter(typeof(MyCustomType))]
/// public class MyCustomTreeFormatter : IReprTreeFormatter
/// {
///     public JsonNode ToReprTree(object obj, ReprContext context)
///     {
///         var custom = (MyCustomType)obj;
///         return new JsonObject 
///         {
///             ["type"] = "MyCustomType",
///             ["kind"] = "class",
///             ["ImportantProperty"] = custom.ImportantProperty?.FormatAsJsonNode(context.WithIncrementedDepth())
///         };
///     }
/// }
/// </code>
/// </example>
public interface IReprTreeFormatter
{
    /// <summary>
    /// Constructs a tree-like representation of the specified object, including type information,
    /// kind, and its simple representation.
    /// </summary>
    /// <param name="obj">
    /// The object to format in a tree-like structure. This is guaranteed to be non-null.
    /// </param>
    /// <param name="context">
    /// The context in which the tree-like representation is created. This may contain configuration
    /// settings or additional parameters influencing the output.
    /// </param>
    /// <returns>
    /// A <see cref="System.Text.Json.Nodes.JsonNode"/> object containing the tree-like representation
    /// of the specified object. The result includes the object's type, type kind, and its formatted value,
    /// structured as a JSON object.
    /// </returns>
    JsonNode ToReprTree(object obj, ReprContext context);
}