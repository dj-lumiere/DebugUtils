namespace DebugUtils.Repr.Formatters.Attributes;

/// <summary>
/// Marks a class as a formatter for specific types in the Repr system.
/// This attribute is used by the formatter registry to automatically discover
/// and register formatters for their target types.
/// </summary>
/// <remarks>
/// <para>Classes marked with this attribute must implement IReprFormatter.</para>
/// <para>The formatter will be automatically registered for all specified target types
/// when the registry is initialized.</para>
/// <para>This provides a declarative way to associate formatters with types without
/// requiring manual registration calls.</para>
/// </remarks>
/// <example>
/// <code>
/// [ReprFormatter(typeof(Person), typeof(Employee))]
/// public class PersonFormatter : IReprFormatter
/// {
///     public string ToRepr(object obj, ReprConfig config, HashSet&lt;int&gt;? visited = null)
///     {
///         // Handle both Person and Employee types
///         if (obj is Person person)
///         {
///             return $"{person.Name} (ID: {person.Id})";
///         }
///         return obj.ToString() ?? "";
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(validOn: AttributeTargets.Class)]
public class ReprFormatterAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the ReprFormatterAttribute with the specified target types.
    /// </summary>
    /// <param name="targetTypes">
    /// The types that this formatter can handle. The formatter will be registered
    /// for each of these types in the formatter registry.
    /// </param>
    /// <remarks>
    /// <para>All specified types should be compatible with the formatter's implementation.</para>
    /// <para>The formatter class must implement IReprFormatter.</para>
    /// </remarks>
    public ReprFormatterAttribute(params Type[] targetTypes)
    {
        TargetTypes = targetTypes;
    }

    /// <summary>
    /// Gets the array of types that this formatter targets.
    /// </summary>
    /// <value>
    /// An array of Type objects representing the types this formatter can handle.
    /// These types will be automatically registered with this formatter in the registry.
    /// </value>
    public Type[] TargetTypes { get; }
}