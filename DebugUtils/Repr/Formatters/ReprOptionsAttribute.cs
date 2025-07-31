namespace DebugUtils.Repr.Formatters;

/// <summary>
///     Provides options for customizing the representation of a type.
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class ReprOptionsAttribute : Attribute
{
    public ReprOptionsAttribute(bool needsPrefix)
    {
        NeedsPrefix = needsPrefix;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the type prefix should be displayed.
    /// </summary>
    public bool NeedsPrefix { get; set; }
}