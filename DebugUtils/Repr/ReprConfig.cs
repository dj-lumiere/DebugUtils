namespace DebugUtils.Repr;

/// <summary>
/// Specifies how floating-point numbers should be represented in object representations.
/// Controls the precision, format, and level of detail for float, double, and decimal values.
/// </summary>
/// <remarks>
/// Different modes serve different debugging and diagnostic purposes:
/// <list type="bullet">
/// <item><description>Exact - Shows precise decimal values without rounding, useful for identifying precision issues</description></item>
/// <item><description>Scientific - Compact notation for very large or small numbers</description></item>
/// <item><description>Round - Fixed precision for clean display in logs and output</description></item>
/// <item><description>General - Standard .NET formatting, balances readability and precision</description></item>
/// <item><description>HexBytes - Low-level bit representation for debugging floating-point issues</description></item>
/// <item><description>BitField - IEEE 754 structure analysis for understanding floating-point behavior</description></item>
/// </list>
/// </remarks>
public enum FloatReprMode
{
    /// <summary>
    /// Exact decimal representation without rounding or approximation.
    /// Shows the true decimal value of the floating-point number.
    /// </summary>
    /// <remarks>
    /// This config is outdated due to BigInteger dependency and performance reasons. Use Exact instead.
    /// Useful for debugging precision issues and understanding exact floating-point values.
    /// May produce very long decimal representations for some values.
    /// </remarks>
    /// <example>
    /// 3.14f → "3.1400001049041748046875E0"
    /// </example>
    Exact_Old,

    /// <summary>
    /// Exact decimal representation without rounding or approximation.
    /// Shows the true decimal value of the floating-point number.
    /// </summary>
    /// <remarks>
    /// Useful for debugging precision issues and understanding exact floating-point values.
    /// May produce very long decimal representations for some values.
    /// </remarks>
    /// <example>
    /// 3.14f → "3.1400001049041748046875E0"
    /// </example>
    Exact,

    /// <summary>
    /// Scientific notation with configurable precision.
    /// Formats numbers in exponential form (e.g., 1.23E+4).
    /// </summary>
    /// <remarks>
    /// Ideal for very large or very small numbers where the magnitude is more important than exact decimal places.
    /// Precision is controlled by the FloatPrecision configuration parameter.
    /// </remarks>
    /// <example>
    /// 1234.5f → "1.23E+03" (with precision 2)
    /// </example>
    Scientific,

    /// <summary>
    /// Fixed-point notation with specified precision.
    /// Rounds to the specified number of decimal places.
    /// </summary>
    /// <remarks>
    /// Best for clean, readable output in logs and user-facing displays.
    /// Precision is controlled by the FloatPrecision configuration parameter.
    /// </remarks>
    /// <example>
    /// 3.14159f → "3.14" (with precision 2)
    /// </example>
    Round,

    /// <summary>
    /// Default .NET ToString() formatting.
    /// Uses the standard .NET representation for floating-point numbers.
    /// </summary>
    /// <remarks>
    /// Provides familiar formatting that matches standard .NET output.
    /// Good balance between readability and precision for most use cases.
    /// </remarks>
    /// <example>
    /// 3.14159f → "3.14159"
    /// </example>
    General,

    /// <summary>
    /// Raw bytes as hexadecimal representation.
    /// Shows the underlying binary representation of the floating-point value.
    /// </summary>
    /// <remarks>
    /// Useful for low-level debugging of floating-point storage and representation issues.
    /// Shows the exact bit pattern stored in memory.
    /// </remarks>
    /// <example>
    /// 3.14f → "0x4048F5C3"
    /// </example>
    HexBytes,

    /// <summary>
    /// IEEE 754 bit field representation showing sign, exponent, and mantissa.
    /// Displays the internal structure of floating-point numbers.
    /// </summary>
    /// <remarks>
    /// Provides deep insight into floating-point representation for debugging numerical issues.
    /// Format: sign|exponent|mantissa where each component is shown as binary.
    /// </remarks>
    /// <example>
    /// 3.14f → "0|10000000|10010001111010111000011"
    /// </example>
    BitField
}

/// <summary>
/// Specifies how integer values should be represented in object representations.
/// Controls the base, format, and level of detail for integer types.
/// </summary>
/// <remarks>
/// Different representations serve various debugging and analysis purposes:
/// <list type="bullet">
/// <item><description>Decimal - Standard base-10 representation, most readable for general use</description></item>
/// <item><description>Hex - Base-16 representation, useful for bit manipulation and low-level debugging</description></item>
/// <item><description>Binary - Base-2 representation, ideal for understanding bit patterns</description></item>
/// <item><description>HexBytes - Raw memory representation, useful for understanding storage layout</description></item>
/// </list>
/// </remarks>
public enum IntReprMode
{
    /// <summary>
    /// Hexadecimal notation with 0x prefix.
    /// Displays integers in base-16 format.
    /// </summary>
    /// <remarks>
    /// Useful for bit manipulation, memory addresses, and low-level debugging.
    /// Negative numbers are shown with proper two's complement representation.
    /// </remarks>
    /// <example>
    /// 255 → "0xFF"
    /// -1 → "-0x1"
    /// </example>
    Hex,

    /// <summary>
    /// Binary notation with 0b prefix.
    /// Displays integers in base-2 format showing individual bits.
    /// </summary>
    /// <remarks>
    /// Excellent for understanding bit patterns, flags, and bitwise operations.
    /// Shows the exact bit representation of the value.
    /// </remarks>
    /// <example>
    /// 255 → "0b11111111"
    /// 5 → "0b101"
    /// </example>
    Binary,

    /// <summary>
    /// Standard decimal notation.
    /// Displays integers in familiar base-10 format.
    /// </summary>
    /// <remarks>
    /// Most readable format for general use and mathematical operations.
    /// Default representation that most users expect.
    /// </remarks>
    /// <example>
    /// 255 → "255"
    /// -42 → "-42"
    /// </example>
    Decimal,

    /// <summary>
    /// Raw bytes as hexadecimal with 0x prefix.
    /// Shows the underlying memory representation of the integer.
    /// </summary>
    /// <remarks>
    /// Displays the exact bytes stored in memory, useful for understanding
    /// endianness, padding, and low-level data representation.
    /// </remarks>
    /// <example>
    /// 255 (int) → "0x000000FF"
    /// 255 (byte) → "0xFF"
    /// </example>
    HexBytes
}

/// <summary>
/// Specifies when type prefixes should be displayed in object representations.
/// Controls the verbosity of type information in the output.
/// </summary>
/// <remarks>
/// Type prefix modes balance between information density and readability:
/// <list type="bullet">
/// <item><description>AlwaysShow - Maximum information, useful for detailed debugging</description></item>
/// <item><description>HideObvious - Clean output while preserving important type information</description></item>
/// <item><description>AlwaysHide - Minimal output focusing on content over type information</description></item>
/// </list>
/// </remarks>
public enum TypeReprMode
{
    /// <summary>
    /// Always display type information for non-null values.
    /// Shows type prefixes for all objects except null values.
    /// </summary>
    /// <remarks>
    /// Provides maximum type information, useful for detailed debugging sessions
    /// where understanding exact types is crucial. Can produce verbose output.
    /// </remarks>
    /// <example>
    /// "hello" → string("hello")
    /// [1, 2, 3] → List([int(1), int(2), int(3)])
    /// </example>
    AlwaysShow,

    /// <summary>
    /// Hide type prefixes for obvious or commonly used types.
    /// Selectively displays type information only when it adds value.
    /// </summary>
    /// <remarks>
    /// <para>Hides type prefixes for commonly understood types including:</para>
    /// <list type="bullet">
    /// <item><description>Basic types: string, char, bool</description></item>
    /// <item><description>Common collections: List, Dictionary, HashSet</description></item>
    /// <item><description>Tuples and obvious container types</description></item>
    /// </list>
    /// <para>Still shows type information for complex, ambiguous, or less common types.</para>
    /// </remarks>
    /// <example>
    /// "hello" → "hello"  (string type is obvious)
    /// [1, 2, 3] → [1, 2, 3]  (List type is obvious)
    /// myCustomObject → MyCustomType(field1: value1)  (custom type shown)
    /// </example>
    HideObvious,

    /// <summary>
    /// Never display type prefixes.
    /// Shows only the content without any type information.
    /// </summary>
    /// <remarks>
    /// Produces the most compact output focusing entirely on values.
    /// Useful when type information is not needed or when integrating with systems
    /// that expect minimal formatting.
    /// </remarks>
    /// <example>
    /// "hello" → "hello"
    /// [1, 2, 3] → [1, 2, 3]
    /// myCustomObject → field1: value1, field2: value2
    /// </example>
    AlwaysHide
}

/// <summary>
/// Specifies how elements within containers (collections, objects, records) should be formatted.
/// Controls the configuration inheritance and formatting strategy for nested content.
/// </summary>
/// <remarks>
/// Container formatting modes provide different strategies for handling nested content:
/// <list type="bullet">
/// <item><description>UseDefaultConfig - Consistent default formatting throughout</description></item>
/// <item><description>UseParentConfig - Maintains formatting consistency with parent context</description></item>
/// <item><description>UseSimpleFormats - Clean, readable formatting optimized for containers</description></item>
/// <item><description>UseCustomConfig - Full control with user-specified configuration</description></item>
/// </list>
/// </remarks>
public enum ContainerReprMode
{
    /// <summary>
    /// Use the system default configuration for container contents.
    /// Applies GlobalDefaults configuration to all nested elements.
    /// </summary>
    /// <remarks>
    /// Ensures consistent formatting across all containers using the library's default settings.
    /// Good for uniform output regardless of the parent object's configuration.
    /// </remarks>
    UseDefaultConfig,

    /// <summary>
    /// Use the same configuration as the parent object.
    /// Inherits all formatting settings from the containing context.
    /// </summary>
    /// <remarks>
    /// Maintains formatting consistency throughout the entire object hierarchy.
    /// If parent uses exact floats and hex integers, containers will use the same formatting.
    /// </remarks>
    UseParentConfig,

    /// <summary>
    /// Force simple, readable formats for container contents.
    /// Uses General float mode and Decimal integer mode for clean display.
    /// </summary>
    /// <remarks>
    /// Optimized for readability within containers where detailed numeric formatting
    /// might be overwhelming. Provides clean, compact representation of container contents.
    /// </remarks>
    UseSimpleFormats,

    /// <summary>
    /// Use an explicitly specified custom configuration for container contents.
    /// Applies the configuration specified in CustomContainerConfig parameter.
    /// </summary>
    /// <remarks>
    /// Provides maximum flexibility by allowing different formatting rules for containers
    /// versus their contents. Requires CustomContainerConfig to be specified.
    /// </remarks>
    UseCustomConfig
}

/// <summary>
/// Specifies the overall formatting strategy and output mode for object representation.
/// Controls how the system chooses between different representation approaches.
/// </summary>
/// <remarks>
/// Formatting modes provide different balances between automation and control:
/// <list type="bullet">
/// <item><description>Smart - Adaptive approach using best representation for each type</description></item>
/// <item><description>Reflection - Consistent deep inspection of all objects</description></item>
/// <item><description>Hierarchical - Structured JSON-like output for analysis and tooling</description></item>
/// </list>
/// </remarks>
public enum FormattingMode
{
    /// <summary>
    /// Uses intelligent formatter selection with ToString() fallback when appropriate.
    /// Automatically chooses the most useful representation for each object type.
    /// </summary>
    /// <remarks>
    /// <para>This mode intelligently decides between custom formatting and ToString() based on:</para>
    /// <list type="bullet">
    /// <item><description>Whether the type has a meaningful ToString() override</description></item>
    /// <item><description>The type's characteristics and common usage patterns</description></item>
    /// <item><description>The information value of reflection-based inspection</description></item>
    /// </list>
    /// <para>Provides the best balance between usefulness and performance for most scenarios.</para>
    /// </remarks>
    Smart,

    /// <summary>
    /// Always uses reflection-based custom formatting, never falls back to ToString().
    /// Provides consistent deep inspection of all objects regardless of ToString() implementations.
    /// </summary>
    /// <remarks>
    /// Ensures uniform, detailed representation by always examining object structure
    /// through reflection. Useful for debugging scenarios where ToString() might hide
    /// important details or when you need consistent formatting across all types.
    /// </remarks>
    Reflection
}

/// <summary>
/// Configuration record controlling all aspects of object representation formatting.
/// Provides comprehensive control over numeric formatting, type display, container handling,
/// and output modes for the Repr system.
/// </summary>
/// <param name="FloatMode">
/// Specifies how floating-point numbers (float, double, decimal) should be formatted.
/// Controls precision, notation, and level of detail for numeric output.
/// <remarks>
/// This property is obsolete. Use FloatFormatString instead for more flexibility. Migration guide:
/// FloatMode.Exact → "EX"
/// FloatMode.Scientific → "E" + FloatPrecision
/// FloatMode.Round → "F" + FloatPrecision
/// FloatMode.General → "G"
/// FloatMode.HexBytes → "HB"
/// FloatMode.BitField → "BF"
/// </remarks>
/// </param>
/// <param name="FloatPrecision">
/// Number of decimal places for floating-point formatting when applicable.
/// Used by Round and Scientific modes.
/// Set to a negative integer (e.g., -1) for automatic precision.
/// <remarks>
/// This property is obsolete. Use FloatFormatString instead.
/// Migration guide: Combine with FloatMode like "F2" for Round mode with 2 decimal places.
/// </remarks>
/// </param>
/// <param name="IntMode">
/// Specifies how integers should be formatted.
/// Controls the base (decimal, hex, binary) and representation style.
/// <remarks>
/// This property is obsolete. Use IntFormatString instead for more flexibility.
/// Migration guide: IntMode.Decimal → "D", IntMode.Hex → "X", 
/// IntMode.Binary → "B", IntMode.HexBytes → "HB"
/// </remarks>
/// </param>
/// <param name="FloatFormatString">
/// Format string for floating-point numbers (float, double, decimal, Half).
/// Supports standard .NET numeric format strings plus special modes:
/// <list type="bullet">
/// <item>Standard formats: "F2" (fixed 2 decimals), "E" (scientific), "G" (general), "P" (percent), etc.</item>
/// <item>Special modes: "EX" (exact decimal), "HB" (hex bytes), "BF" (bit field for analysis)</item>
/// <item>When empty, falls back to FloatMode enum behavior for backward compatibility.</item>
/// </list>
/// See: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
/// </param>
/// <param name="IntFormatString">
/// Format string for integer numbers (byte, int, long, BigInteger, etc.).
/// Supports standard .NET numeric format strings plus special modes:
/// <list type="bullet">
/// <item>Standard formats: "D" (decimal), "X" (hex uppercase), "x" (hex lowercase), "N" (number with separators), etc..</item>
/// <item>Special modes: "HB" (hex bytes), "B"/"b" (binary for older .NET versions)</item>
/// <item>When empty, falls back to IntMode enum behavior for backward compatibility.</item>
/// </list>
/// See: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
/// </param>
/// <param name="ContainerReprMode">
/// Specifies how elements within containers, structs, classes, and records should be formatted.
/// Controls configuration inheritance and formatting strategy for nested content.
/// </param>
/// <param name="TypeMode">
/// Specifies when to display type prefixes in the output.
/// Controls the verbosity of type information for better readability or debugging detail.
/// </param>
/// <param name="FormattingMode">
/// Specifies the overall formatting strategy and output mode.
/// Controls how the system chooses between different representation approaches.
/// </param>
/// <param name="ShowNonPublicProperties">
/// Specifies whether non-public properties and fields should be included in the output.
/// May expose sensitive internal data - use with caution in production environments.
/// </param>
/// <param name="MaxDepth">
/// Specifies the maximum recursion depth for nested objects.
/// Higher values may significantly increase processing time and memory usage.
/// Set to a negative integer (e.g., -1) to disable depth limiting entirely.
/// </param>
/// <param name="MaxElementsPerCollection">
/// Specifies the maximum number of elements to include per collection (arrays, lists, etc.).
/// Higher values may significantly increase processing time and output size.
/// Set to a negative integer (e.g., -1) to disable element limiting entirely.
/// </param>
/// <param name="MaxPropertiesPerObject">
/// Specifies the maximum number of properties and fields to include per object.
/// Higher values may significantly increase processing time and output size.
/// Set to a negative integer (e.g., -1) to disable property limiting entirely.
/// </param>
/// <param name="MaxStringLength">
/// Specifies the maximum length for a string to be visible.
/// Strings exceeding this length will be truncated with a suffix like "...{count} more characters".
/// Higher values may significantly increase processing time and output size.
/// Set to a negative integer (e.g., -1) to disable string length limiting entirely.
/// </param>
/// <param name="EnablePrettyPrintForReprTree">
/// Specifies whether to format ReprTree output with indentation and line breaks for readability.
/// Enabling this significantly increases output size but improves human readability.
/// </param>
/// <param name="CustomContainerConfig">
/// The configuration to use when ContainerReprMode is set to UseCustomConfig.
/// Allows different formatting rules for container contents versus their parents.
/// Must be specified when using UseCustomConfig mode.
/// </param>
/// <remarks>
/// <para><strong>Configuration Strategies:</strong></para>
/// <list type="bullet">
/// <item><description><strong>Debugging:</strong> Use Exact floats, Hex integers, AlwaysShow types for maximum detail</description></item>
/// <item><description><strong>Logging:</strong> Use General floats, Decimal integers, HideObvious types for clean output</description></item>
/// <item><description><strong>Analysis:</strong> Use ReprTree() for structured, tree-like output</description></item>
/// <item><description><strong>Development:</strong> Use Smart mode with balanced settings for general development work</description></item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><description>Exact and BitField float modes have higher computational cost</description></item>
/// <item><description>Hierarchical mode produces larger output and requires more processing</description></item>
/// <item><description>Reflection mode may be slower than Smart mode for types with good ToString() implementations</description></item>
/// </list>
/// 
/// <para><strong>Container Configuration:</strong></para>
/// <para>Container modes allow different formatting strategies for nested content. For example,
/// you might want exact floats at the top level but simplified floats within collections for readability.</para>
/// </remarks>
/// <example>
/// <code>
/// // NEW FORMAT STRING APPROACH (recommended):
/// 
/// // Debugging configuration - maximum detail with format strings
/// var debugConfig = new ReprConfig(
///     FloatFormatString: "EX",    // Exact floating-point representation
///     IntFormatString: "X",       // Hexadecimal integers
///     TypeMode: TypeReprMode.AlwaysShow,
///     FormattingMode: FormattingMode.Reflection
/// );
/// 
/// // Clean logging configuration with format strings
/// var logConfig = new ReprConfig(
///     FloatFormatString: "F2",    // Fixed 2 decimal places
///     IntFormatString: "D",       // Decimal integers
///     TypeMode: TypeReprMode.HideObvious,
///     ContainerReprMode: ContainerReprMode.UseSimpleFormats
/// );
/// 
/// // Special debugging modes
/// var lowLevelConfig = new ReprConfig(
///     FloatFormatString: "HB",    // Hex bytes for floats
///     IntFormatString: "HB",      // Hex bytes for integers
///     TypeMode: TypeReprMode.AlwaysShow
/// );
/// 
/// // IEEE 754 analysis
/// var ieeeConfig = new ReprConfig(
///     FloatFormatString: "BF",    // Bit field representation
///     IntFormatString: "B"        // Binary representation
/// );
/// 
/// // LEGACY ENUM APPROACH (deprecated but still supported):
/// 
/// // Old debugging configuration (will show deprecation warnings)
/// var oldDebugConfig = new ReprConfig(
///     FloatMode: FloatReprMode.Exact,     // Use FloatFormatString: "EX" instead
///     IntMode: IntReprMode.Hex,           // Use IntFormatString: "X" instead
///     TypeMode: TypeReprMode.AlwaysShow,
///     FormattingMode: FormattingMode.Reflection
/// );
/// 
/// // For structured tree analysis, use ReprTree() method:
/// // var treeOutput = obj.ReprTree(new ReprContext(new ReprConfig(EnablePrettyPrintForReprTree: true)));
/// 
/// // Custom container formatting with new approach
/// var customConfig = new ReprConfig(
///     FloatFormatString: "EX",    // Exact floats at top level
///     ContainerReprMode: ContainerReprMode.UseCustomConfig,
///     CustomContainerConfig: new ReprConfig(
///         FloatFormatString: "F1",    // 1 decimal place in containers
///         IntFormatString: "D"        // Decimal integers in containers
///     )
/// );
/// </code>
/// </example>
public record ReprConfig(
    FloatReprMode FloatMode = FloatReprMode.Exact,
    int FloatPrecision = -1,
    IntReprMode IntMode = IntReprMode.Decimal,
    string FloatFormatString = "",
    string IntFormatString = "",
    ContainerReprMode ContainerReprMode = ContainerReprMode.UseSimpleFormats,
    TypeReprMode TypeMode = TypeReprMode.HideObvious,
    FormattingMode FormattingMode = FormattingMode.Smart,
    bool ShowNonPublicProperties = false,
    int MaxDepth = 5,
    int MaxPropertiesPerObject = 10,
    int MaxElementsPerCollection = 50,
    int MaxStringLength = 120,
    bool EnablePrettyPrintForReprTree = false,
    ReprConfig? CustomContainerConfig = null
)
{
    /// <summary>
    /// Specifies how floating-point numbers (float, double, decimal) should be formatted.
    /// Controls precision, notation, and level of detail for numeric output.
    /// </summary>
    /// <remarks>
    /// This property is obsolete. Use FloatFormatString instead for more flexibility.
    /// Migration guide: FloatMode.Exact → "EX", FloatMode.Scientific → "E", 
    /// FloatMode.Round → "F" + FloatPrecision, FloatMode.General → "G" (ignores FloatPrecision), 
    /// FloatMode.HexBytes → "HB", FloatMode.BitField → "BF"
    /// </remarks>
    [Obsolete(
        message:
        "Use FloatFormatString instead. FloatMode.Exact becomes \"EX\", FloatMode.Scientific becomes \"E\", FloatMode.Round becomes \"F\" + FloatPrecision, FloatMode.General becomes \"G\" (ignores FloatPrecision), FloatMode.HexBytes becomes \"HB\", FloatMode.BitField becomes \"BF\"",
        error: false)]
    public FloatReprMode FloatMode { get; } = FloatMode;

    /// <summary>
    /// Number of decimal places for floating-point formatting when applicable.
    /// Used by Round and Scientific modes.
    /// Set to a negative integer (e.g., -1) for automatic precision.
    /// </summary>
    /// <remarks>
    /// This property is obsolete. Use FloatFormatString instead.
    /// Migration guide: Combine with FloatMode like "F2" for Round mode with 2 decimal places.
    /// </remarks>
    [Obsolete(
        message:
        "Use FloatFormatString instead. Combine with FloatMode like \"F2\" for Round mode with 2 decimal places",
        error: false)]
    public int FloatPrecision { get; } = FloatPrecision;

    /// <summary>
    /// Specifies how integers should be formatted.
    /// Controls the base (decimal, hex, binary) and representation style.
    /// </summary>
    /// <remarks>
    /// This property is obsolete. Use IntFormatString instead for more flexibility.
    /// Migration guide: IntMode.Decimal → "D", IntMode.Hex → "X", 
    /// IntMode.Binary → "B", IntMode.HexBytes → "HB"
    /// </remarks>
    [Obsolete(
        message:
        "Use IntFormatString instead. IntMode.Decimal becomes \"D\", IntMode.Hex becomes \"X\", IntMode.Binary becomes \"B\", IntMode.HexBytes becomes \"HB\"",
        error: false)]
    public IntReprMode IntMode { get; } = IntMode;

    /// <summary>
    /// Gets the default configuration optimized for formatting container contents.
    /// Provides clean, readable formatting suitable for elements within collections,
    /// arrays, and object properties.
    /// </summary>
    /// <value>
    /// A configuration with General float mode, Decimal integers,
    /// simple container formatting, and obvious type hiding for clean container display.
    /// </value>
    /// <remarks>
    /// This configuration is automatically used by various container formatters when
    /// ContainerReprMode is set to UseSimpleFormats. It prioritizes readability
    /// over detailed numeric representation within containers.
    /// </remarks>
    public static ReprConfig ContainerDefaults => new(
        FloatFormatString: "G",
        IntFormatString: "D",
        ContainerReprMode: ContainerReprMode.UseSimpleFormats,
        TypeMode: TypeReprMode.HideObvious,
        ShowNonPublicProperties: false,
        MaxDepth: 5,
        MaxPropertiesPerObject: 10,
        MaxElementsPerCollection: 50,
        MaxStringLength: 120
    );

    /// <summary>
    /// Gets the default configuration for top-level object representation.
    /// Provides detailed, precise formatting suitable for debugging and diagnostic purposes.
    /// </summary>
    /// <value>
    /// A configuration with Exact float mode, automatic precision, Decimal integers,
    /// default container handling, and obvious type hiding for balanced detail and readability.
    /// </value>
    /// <remarks>
    /// This is the default configuration used when no explicit configuration is provided
    /// to the Repr() method. It balances detailed information with readability,
    /// using exact float representation for precision while keeping integer and type
    /// display clean and familiar.
    /// </remarks>
    public static ReprConfig GlobalDefaults => new(
        FloatFormatString: "EX",
        IntFormatString: "D",
        ContainerReprMode: ContainerReprMode.UseSimpleFormats,
        TypeMode: TypeReprMode.HideObvious,
        ShowNonPublicProperties: false,
        MaxDepth: 5,
        MaxPropertiesPerObject: 10,
        MaxElementsPerCollection: 50,
        MaxStringLength: 120
    );
}