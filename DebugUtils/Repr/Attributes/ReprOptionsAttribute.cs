namespace DebugUtils.Repr.Attributes;

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
/// 
/// // For numeric types, explicit bit-width suffixes are automatically added:
/// Console.WriteLine(42.Repr());     // Output: 42i32
/// Console.WriteLine(3.14f.Repr());  // Output: 3.14f32
/// </code>
/// </example>
[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class ReprOptionsAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the ReprOptionsAttribute with the specified type append mode.
    /// </summary>
    /// <param name="needsPrefix">
    /// Determines how type information is positioned relative to the content.
    /// Use false for no type info, true for traditional prefix behavior.
    /// For more control, use the TypeAppendMode constructor.
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
    /// <para>Note: For numeric types with explicit bit-width suffixes (like "42i32"), 
    /// this property may be overridden by the formatting engine.</para>
    /// </remarks>
    public bool NeedsPrefix { get; set; }
}