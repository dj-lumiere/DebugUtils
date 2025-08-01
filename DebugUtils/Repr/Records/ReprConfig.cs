namespace DebugUtils.Repr.Records;

/// <summary>
///     Specifies how floating-point numbers should be represented.
/// </summary>
public enum FloatReprMode
{
    /// <summary>Exact decimal representation without rounding</summary>
    Exact,

    /// <summary>Scientific notation (e.g., 1.23E+4)</summary>
    Scientific,

    /// <summary>Fixed-point notation with specified precision</summary>
    Round,

    /// <summary>Default .NET formatting</summary>
    General,

    /// <summary>Raw bytes as hexadecimal</summary>
    HexBytes,

    /// <summary>IEEE 754 bit field representation (sign|exponent|mantissa)</summary>
    BitField
}

/// <summary>
///     Specifies how integers should be represented.
/// </summary>
public enum IntReprMode
{
    /// <summary>Hexadecimal notation</summary>
    Hex,

    /// <summary>Binary notation</summary>
    Binary,

    /// <summary>Decimal notation</summary>
    Decimal,

    /// <summary>Raw bytes as hexadecimal</summary>
    HexBytes
}

/// <summary>
///     Specifies when types should be represented.
/// </summary>
public enum TypeReprMode
{
    /// <summary>
    ///     Always show type info besides null.
    /// </summary>
    AlwaysShow,

    /// <summary>
    ///     Hide obvious type names.
    ///     Those include Tuples, List, Set, Dictionary, string, char.
    /// </summary>
    HideObvious,

    /// <summary>
    ///     Do not show type names.
    /// </summary>
    AlwaysHide
}

public enum ContainerReprMode
{
    /// <summary>
    ///     Use default config
    /// </summary>
    UseDefaultConfig,

    /// <summary>
    ///     Use the same config as parent
    /// </summary>
    UseParentConfig,

    /// <summary>
    ///     Force simple formats (General float, Decimal int)
    /// </summary>
    UseSimpleFormats,

    /// <summary>
    ///     Use explicit container config
    /// </summary>
    UseCustomConfig
}

public enum FormattingMode
{
    /// <summary>
    ///     Uses custom formatter with ToString fallback when ToString provides more useful information.
    /// </summary>
    Smart,

    /// <summary>
    ///     Always uses reflection-based custom formatting, never falls back to ToString.
    /// </summary>
    Reflection,

    /// <summary>
    ///     Always uses reflection-based custom formatting with hierarchical format,
    ///     which is in json-like dialect.
    ///     Output: JSON objects with type and value information for all objects.
    /// </summary>
    Hierarchical
}

/// <summary>
///     Configuration options for controlling how objects are represented in string form.
/// </summary>
/// <param name="FloatMode">Specifies how floating-point numbers should be formatted</param>
/// <param name="FloatPrecision">Number of decimal places for floating-point formatting (when applicable)</param>
/// <param name="IntMode">Specifies how integers should be formatted (decimal, hex, binary, etc.)</param>
/// <param name="ContainerReprMode">
///     Specifies how elements/fields/properties in container/struct/class/.record should be
///     formatted
/// </param>
/// <param name="TypeMode">Specifies when to hide type prefixes</param>
/// <param name="FormattingMode">Specifies how to do formatting (adaptive mode, always reflection)</param>
/// <param name="CustomContainerConfig">The config to use when ContainerReprMode is in custom mode</param>
/// <example>
///     <code>
/// // Exact floating-point representation
/// var exactConfig = new ReprConfig(FloatMode: FloatReprMode.Exact);
/// 
/// // Debug-friendly configuration
/// var debugConfig = new ReprConfig(
///     FloatMode: FloatReprMode.BitField,
///     IntMode: IntReprMode.Hex
/// );
/// 
/// // Use predefined configurations
/// obj.Repr(ReprConfig.GlobalDefaults);
/// obj.Repr(ReprConfig.ContainerDefaults);
/// </code>
/// </example>
public record ReprConfig(
    FloatReprMode FloatMode = FloatReprMode.General,
    int FloatPrecision = 2,
    IntReprMode IntMode = IntReprMode.Decimal,
    ContainerReprMode ContainerReprMode = ContainerReprMode.UseSimpleFormats,
    TypeReprMode TypeMode = TypeReprMode.HideObvious,
    FormattingMode FormattingMode = FormattingMode.Smart,
    ReprConfig? CustomContainerConfig = null
)
{
    public static ReprConfig ContainerDefaults => new(
        FloatMode: FloatReprMode.General,
        FloatPrecision: 2,
        ContainerReprMode: ContainerReprMode.UseSimpleFormats,
        IntMode: IntReprMode.Decimal,
        TypeMode: TypeReprMode.HideObvious);

    public static ReprConfig GlobalDefaults => new(
        FloatMode: FloatReprMode.Exact,
        FloatPrecision: -1,
        ContainerReprMode: ContainerReprMode.UseDefaultConfig,
        IntMode: IntReprMode.Decimal,
        TypeMode: TypeReprMode.HideObvious);
}