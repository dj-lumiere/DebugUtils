namespace DebugUtils.Repr.Formatters.Attributes;

/// <summary>
/// Provides declarative options for customizing how a type is represented in the Repr system.
/// Apply this attribute to classes, structs, or enums to control their display behavior.
/// </summary>
/// <remarks>
/// <para>This attribute allows fine-tuning of representation behavior without implementing
/// custom formatters. It's particularly useful for controlling type prefix display.</para>
/// <para>The attribute settings are checked during the representation process and
/// override default behavior based on type characteristics.</para>
/// </remarks>
/// <example>
/// <code>
/// [ReprOptions(needsPrefix: false)]
/// public class SimpleValue
/// {
///     public string Content { get; set; }
/// }
/// 
/// var obj = new SimpleValue { Content = "test" };
/// Console.WriteLine(obj.Repr());
/// // Output: Content: "test"  (no "SimpleValue(...)" wrapper)
/// </code>
/// </example>
[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class ReprOptionsAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the ReprOptionsAttribute with the specified prefix requirement.
    /// </summary>
    /// <param name="needsPrefix">
    /// true if the type should always display a type prefix unless explicitly said to hidden
    /// (e.g., "TypeName(content)");
    /// false if the type should display content without a type wrapper.
    /// </param>
    public ReprOptionsAttribute(bool needsPrefix)
    {
        NeedsPrefix = needsPrefix;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the type prefix should be displayed
    /// when representing objects of this type.
    /// </summary>
    /// <value>
    /// true if objects of this type should be wrapped with type information;
    /// false if only the content should be displayed.
    /// </value>
    /// <remarks>
    /// <para>When true: Output format is "TypeName(content)"</para>
    /// <para>When false: Output format is just "content"</para>
    /// <para>This setting interacts with the global TypeReprMode configuration.</para>
    /// </remarks>
    public bool NeedsPrefix { get; set; }
}