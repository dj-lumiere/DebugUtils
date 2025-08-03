namespace DebugUtils.Repr.Records;

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
    Reflection,

    /// <summary>
    /// Produces structured, JSON-like output optimized for debugging and analysis.
    /// Shows object hierarchy, type information, and handles circular references.
    /// <para><strong>Why Use This:</strong> Unlike standard JSON serializers, hierarchical mode 
    /// preserves debugging information like exact numeric representations, handles circular 
    /// references gracefully, and provides type metadata for every value.</para>
    /// </summary>
    /// <remarks>
    /// <para>Output uses JSON-style syntax but includes debugging extensions that may not be valid JSON:</para>
    /// <list type="bullet">
    /// <item><description>Type metadata for all values</description></item>
    /// <item><description>Special numeric formats (hex, binary, exact floats)</description></item>
    /// <item><description>Error information for problematic properties</description></item>
    /// <item><description>Circular reference detection and markers</description></item>
    /// </list>
    /// 
    /// <para><strong>Ideal for:</strong> IDE integration, automated analysis, complex debugging scenarios.</para>
    /// <para><strong>Not for:</strong> Data interchange, API responses, or storage. Use dedicated JSON serializers for those cases.</para>
    /// </remarks>
    /// <example>
    /// Standard mode: Person(Name: "John", Age: int(30))
    /// Hierarchical mode: {"type":"Person","Name":{"type":"string","value":"John"},"Age":{"type":"int","value":"30"}}
    /// </example>
    /// <seealso cref="System.Text.Json.JsonSerializer"/>
    /// <seealso cref="ReprConfig.HierarchicalDefaults"/>
    Hierarchical
}

/// <summary>
/// Configuration record controlling all aspects of object representation formatting.
/// Provides comprehensive control over numeric formatting, type display, container handling,
/// and output modes for the Repr system.
/// </summary>
/// <param name="FloatMode">
/// Specifies how floating-point numbers (float, double, decimal) should be formatted.
/// Controls precision, notation, and level of detail for numeric output.
/// </param>
/// <param name="FloatPrecision">
/// Number of decimal places for floating-point formatting when applicable.
/// Used by Round and Scientific modes. Set to -1 for automatic precision.
/// </param>
/// <param name="IntMode">
/// Specifies how integers should be formatted.
/// Controls the base (decimal, hex, binary) and representation style.
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
/// <item><description><strong>Analysis:</strong> Use Hierarchical mode for structured, machine-readable output</description></item>
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
/// // Debugging configuration - maximum detail
/// var debugConfig = new ReprConfig(
///     FloatMode: FloatReprMode.Exact,
///     IntMode: IntReprMode.Hex,
///     TypeMode: TypeReprMode.AlwaysShow,
///     FormattingMode: FormattingMode.Reflection
/// );
/// 
/// // Clean logging configuration
/// var logConfig = new ReprConfig(
///     FloatMode: FloatReprMode.Round,
///     FloatPrecision: 2,
///     IntMode: IntReprMode.Decimal,
///     TypeMode: TypeReprMode.HideObvious,
///     ContainerReprMode: ContainerReprMode.UseSimpleFormats
/// );
/// 
/// // Structured analysis configuration
/// var analysisConfig = new ReprConfig(
///     FormattingMode: FormattingMode.Hierarchical,
///     TypeMode: TypeReprMode.AlwaysHide
/// );
/// 
/// // Custom container formatting
/// var customConfig = new ReprConfig(
///     FloatMode: FloatReprMode.Exact,  // Exact floats at top level
///     ContainerReprMode: ContainerReprMode.UseCustomConfig,
///     CustomContainerConfig: new ReprConfig(
///         FloatMode: FloatReprMode.Round,  // Rounded floats in containers
///         FloatPrecision: 1
///     )
/// );
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
    /// <summary>
    /// Gets the default configuration optimized for formatting container contents.
    /// Provides clean, readable formatting suitable for elements within collections,
    /// arrays, and object properties.
    /// </summary>
    /// <value>
    /// A configuration with General float mode, 2-decimal precision, Decimal integers,
    /// simple container formatting, and obvious type hiding for clean container display.
    /// </value>
    /// <remarks>
    /// This configuration is automatically used by various container formatters when
    /// ContainerReprMode is set to UseSimpleFormats. It prioritizes readability
    /// over detailed numeric representation within containers.
    /// </remarks>
    public static ReprConfig ContainerDefaults => new(
        FloatMode: FloatReprMode.General,
        FloatPrecision: 2,
        ContainerReprMode: ContainerReprMode.UseSimpleFormats,
        IntMode: IntReprMode.Decimal,
        TypeMode: TypeReprMode.HideObvious);

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
        FloatMode: FloatReprMode.Exact,
        FloatPrecision: -1,
        ContainerReprMode: ContainerReprMode.UseDefaultConfig,
        IntMode: IntReprMode.Decimal,
        TypeMode: TypeReprMode.HideObvious);
    /// <summary>
    /// Default configuration for hierarchical JSON-style output.
    /// Optimized for structured readability and JSON compatibility.
    /// </summary>
    public static ReprConfig HierarchicalDefaults => new(
        FloatMode: FloatReprMode.Exact,      // ✅ JSON-friendly float formatting
        FloatPrecision: -1,                     // ✅ Reasonable precision for JSON
        IntMode: IntReprMode.Decimal,          // ✅ JSON standard - no hex
        ContainerReprMode: ContainerReprMode.UseParentConfig,
        TypeMode: TypeReprMode.AlwaysHide,     // ✅ JSON has type info in structure
        FormattingMode: FormattingMode.Hierarchical
    );
}