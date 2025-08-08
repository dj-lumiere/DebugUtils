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
/// [ReprFormatter(typeof(Person))]
/// [ReprOptions(needsPrefix: false)]
/// public class PersonFormatter : IReprFormatter
/// {
///     public string ToRepr(object obj, ReprContext context)
///     {
///         var person = (Person)obj;
///         var nameRepr = person.Name?.Repr(context.WithIncrementedDepth()) ?? "null";
///         var ageRepr = person.Age.Repr(context.WithIncrementedDepth());
///         return $"Name: {nameRepr}, Age: {ageRepr}";
///     }
/// }
/// </code>
/// </example>
public interface IReprFormatter
{
    /// <summary>
    /// Converts the specified object to its string representation according to the given configuration.
    /// </summary>
    /// <param name="obj">
    /// The object to format. Guaranteed to be non-null and of a type this formatter handles.
    /// The formatter should cast this to the expected type.
    /// </param>
    /// <param name="context">
    /// Optional context controlling formatting behavior and tracking state. If null, uses default configuration.
    /// Contains configuration settings, circular reference tracking, and depth management.
    /// </param>
    /// <returns>
    /// A string representation of the object. Should not include type prefixes if
    /// config.TypeMode is AlwaysHide, as type prefixes are handled by the main Repr system.
    /// </returns>
    /// <remarks>
    /// <para>Implementation guidelines:</para>
    /// <list type="bullet">
    /// <item><description>Always respect the configuration settings when applicable</description></item>
    /// <item><description>Pass the context to child object Repr() calls</description></item>
    /// <item><description>Handle null properties gracefully</description></item>
    /// <item><description>Consider performance for frequently used formatters</description></item>
    /// <item><description>Provide meaningful output even when properties throw exceptions</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// public string ToRepr(object obj, ReprConfig config, HashSet&lt;int&gt;? visited = null)
    /// {
    ///     var person = (Person)obj;
    ///     var nameRepr = person.Name?.Repr(config, visited) ?? "null";
    ///     var ageRepr = person.Age.Repr(config, visited);
    ///     return $"Name: {nameRepr}, Age: {ageRepr}";
    /// }
    /// </code>
    /// </example>
    string ToRepr(object obj, ReprContext context);
}