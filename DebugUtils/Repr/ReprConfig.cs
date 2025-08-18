using System.Globalization;

namespace DebugUtils.Repr;

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
    /// [1, 2, 3] → List([1_i32, 2_i32, 3_i32])
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
    /// "hello" → "hello"  (strings are self-explaining)
    /// [1, 2, 3] → [1_i32, 2_i32, 3_i32]  (Lists are self-explaining)
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
/// Controls how many an object's members are included in a representation (Repr).
/// </summary>
/// <remarks>
/// <para><b>Safety:</b> Accessing properties can execute arbitrary user code and may block,
/// throw, deadlock, or crash (e.g., <c>StackOverflowException</c> cannot be caught).
/// By default, the formatter reads <b>storage</b> only: fields and auto-property
/// <i>backing fields</i>. Non-storage property getters are considered <b>risky</b>
/// and are <b>not</b> invoked unless explicitly enabled in <see cref="ReprConfig"/>.</para>
/// <para><b>Auto-properties:</b> Detected by compiler-generated backing fields:
/// <c>"&lt;Name&gt;k__BackingField"</c> (normal/record) and <c>"&lt;Name&gt;i__Field"</c>
/// (anonymous types). On IL2CPP, only backing-field access is supported (no IL inspection).</para>
/// <para><b>Platform:</b> Any editor-only timeouts guard <i>waiting</i> but do not abort
/// user code. In player builds, risky getters should remain disabled.</para>
/// </remarks>
public enum MemberReprMode
{
    /// <summary>
    /// Includes public fields and auto-properties in object representations.
    /// Displays a simplified view of an object's public structure for debugging.
    /// </summary>
    /// <remarks>
    /// Provides a balance of detail and readability by focusing on publicly accessible fields and auto-implemented properties.
    /// Useful for scenarios where concise and relevant member information is needed without exposing internal or private structures.
    /// </remarks>
    PublicFieldAutoProperty,

    /// <summary>
    /// Includes all public members in object representations.
    /// Captures both public fields and properties for detailed output.
    /// </summary>
    /// <remarks>
    /// This mode is useful for inspecting all public accessible members of an object.
    /// It provides a balance between detail and readability, making it suitable for
    /// most debugging scenarios requiring visibility into an object's public API.
    /// </remarks>
    AllPublic,

    /// <summary>
    /// Includes all fields and auto-properties, regardless of their visibility.
    /// Provides comprehensive detail on both public and non-public members.
    /// </summary>
    /// <remarks>
    /// This mode is useful for scenarios requiring in-depth analysis of an object's structure,
    /// capturing all fields and auto-properties regardless of access modifiers such as `private`,
    /// `protected`, or `internal`. It prioritizes thoroughness over brevity.
    /// </remarks>
    AllFieldAutoProperty,

    /// <summary>
    /// Includes all accessible members, providing the most comprehensive level of detail.
    /// Displays all fields, properties, and members regardless of visibility or access.
    /// </summary>
    /// <remarks>
    /// This mode is suitable for scenarios requiring maximum insight into the object state and structure.
    /// It may produce verbose output, potentially including private and internal members.
    /// Use cautiously in contexts with sensitive or excessive data to avoid performance or security concerns.
    /// </remarks>
    Everything
}

/// <summary>
/// Configuration record controlling all aspects of object representation formatting.
/// Provides comprehensive control over numeric formatting, type display, container handling,
/// and output modes for the Repr system.
/// </summary>
/// <param name="FloatFormatString">
/// Format string for floating-point numbers (float, double, decimal, Half).
/// Supports standard .NET numeric format strings plus special modes:
/// <list type="bullet">
/// <item>Standard formats: "F2" (fixed 2 decimals), "E" (scientific), "G" (general), "P" (percent), etc.</item>
/// <item>Special modes: "EX" (exact decimal), "HP" (hex power for IEEE 754 analysis)</item>
/// </list>
/// See: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
/// </param>
/// <param name="IntFormatString">
/// Format string for integer numbers (byte, int, long, BigInteger, etc.).
/// Supports standard .NET numeric format strings plus special modes:
/// <list type="bullet">
/// <item>Standard formats: "D" (decimal), "N" (number with separators), etc.</item>
/// <item>Custom formats: "X" (hex with 0x prefix), "O" (octal with 0o prefix), "Q" (quaternary with 0q prefix), "B" (binary with 0b prefix)</item>
/// </list>
/// See: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
/// </param>
/// <param name="TypeMode">
/// Specifies when to display type prefixes in the output.
/// Controls the verbosity of type information for better readability or debugging detail.
/// </param>
/// <param name="ViewMode">
/// Specifies the level of member detail included in object representations.
/// Determines the scope of fields and properties to display for debugging purposes.
/// <remarks>
/// It is generally NOT RECOMMENDED to try fetching for ANY properties.
/// It is generally NOT RECOMMENDED to try fetching for PRIVATE fields/properties.
/// </remarks>
/// </param>
/// <param name="UseSimpleFormatsInContainers">
/// When true, uses simplified formatting ("G" for floats, "D" for integers) within containers.
/// When false, container contents inherit the parent's formatting configuration.
/// Provides cleaner, more readable output for collections and nested objects.
/// </param>
/// <param name="HideChildTypes">
/// When true, hides type prefixes for obvious types (string, bool, char, List, Dictionary, etc.) in container contents.
/// When false, shows type information for all nested objects according to TypeMode.
/// Only affects container contents, not top-level objects.
/// </param>
/// <param name="MaxItemsPerContainer">
/// Specifies the maximum number of elements to include per collection (arrays, lists, etc.).
/// Higher values may significantly increase processing time and output size.
/// Set to a negative integer (e.g., -1) to disable element limiting entirely.
/// </param>
/// <param name="MaxStringLength">
/// Specifies the maximum length limit for string.
/// Higher values may significantly increase processing time and memory usage.
/// Set to a negative integer (e.g., -1) to disable length limiting entirely.
/// </param>
/// <param name="MaxDepth">
/// Specifies the maximum recursion depth for nested objects.
/// Higher values may significantly increase processing time and memory usage.
/// Set to a negative integer (e.g., -1) to disable depth limiting entirely.
/// </param>
/// <param name="MaxMemberTimeMs">
/// Specifies the maximum time in milliseconds for a single property getter to execute.
/// Property getters exceeding this time limit will be marked as "[Timed Out]".
/// <para><b>Scope:</b> Only applies to non-auto properties. Fields and auto-property backing fields are always safe to access.</para>
/// <para><b>Limitation:</b> Cannot prevent process crashes from StackOverflowException or similar unrecoverable exceptions.</para>
/// <para><b>Default:</b> 1ms provides safety while allowing most normal getters to complete.</para>
/// <para><b>Performance:</b> Higher values increase the risk of blocking on problematic getters.</para>
/// Set to 0 to disable property getter access entirely (recommended for production/release builds).
/// Set to a negative integer (e.g., -1) to disable property getter access entirely (NOT recommended for production/release builds).
/// </param>
/// <param name="EnablePrettyPrintForReprTree">
/// Specifies whether to format ReprTree output with indentation and line breaks for readability.
/// Enabling this significantly increases output size but improves human readability.
/// </param>
/// <param name="Culture">
/// Specifies the culture to use for standard numeric formatting. When null, uses the current thread's culture.
/// Affects decimal separators, three-digit separators, and other locale-specific number formatting.
/// <para><b>Examples:</b></para>
/// <list type="bullet">
/// <item><description>en-US: 1,234.56 (comma thousands, period decimal)</description></item>
/// <item><description>de-DE: 1.234,56 (period thousands, comma decimal)</description></item>
/// <item><description>InvariantCulture: 1234.56 (no thousands separator, period decimal)</description></item>
/// </list>
/// <para><b>Limitation:</b> Does not affect special format modes (EX, HP, B, Q, O, X) or
/// DateTime/TimeSpan formatting, which always use culture-independent formats for debugging clarity.</para>
/// </param>
/// <remarks>
/// <para><strong>Configuration Strategies:</strong></para>
/// <list type="bullet">
/// <item><description><strong>Debugging:</strong> Use "EX" floats, "X" integers, AlwaysShow types for maximum detail</description></item>
/// <item><description><strong>Logging:</strong> Use "G" floats, "D" integers, HideObvious types for clean output</description></item>
/// <item><description><strong>Analysis:</strong> Use EnablePrettyPrintForReprTree for structured, tree-like output</description></item>
/// <item><description><strong>Container Control:</strong> Use UseSimpleFormatsInContainers and HideChildTypes for clean nested formatting</description></item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><description>"EX" (exact) float formats have a higher computational cost</description></item>
/// <item><description>EnablePrettyPrintForReprTree produces larger output and requires more processing</description></item>
/// <item><description>ViewMode.Everything may be slower due to reflection overhead</description></item>
/// <item><description>Higher MaxDepth and MaxItemsPerContainer values increase processing time</description></item>
/// </list>
/// 
/// <para><strong>Container Configuration:</strong></para>
/// <para>UseSimpleFormatsInContainers and HideChildTypes allow different formatting strategies for nested content. 
/// For example, you might want exact floats at the top level but simplified floats within collections for readability.</para>
/// </remarks>
/// <example>
/// <code>
/// // Debugging configuration - maximum detail
/// var debugConfig = new ReprConfig(
///     FloatFormatString: "EX",    // Exact floating-point representation
///     IntFormatString: "X",       // Hexadecimal integers
///     TypeMode: TypeReprMode.AlwaysShow,
///     UseSimpleFormatsInContainers: false,  // Keep detailed formatting in containers
///     HideChildTypes: false,      // Show all type information
///     Culture: CultureInfo.InvariantCulture  // Consistent output
/// );
/// 
/// // Clean logging configuration
/// var logConfig = new ReprConfig(
///     FloatFormatString: "F2",    // Fixed 2 decimal places
///     IntFormatString: "D",       // Decimal integers
///     TypeMode: TypeReprMode.HideObvious,
///     UseSimpleFormatsInContainers: true,   // Clean container formatting
///     HideChildTypes: true,       // Hide obvious types in containers
///     Culture: CultureInfo.InvariantCulture  // Consistent decimal separators
/// );
/// 
/// // Low-level debugging modes  
/// var lowLevelConfig = new ReprConfig(
///     FloatFormatString: "HP",    // Hex power for floats
///     IntFormatString: "X",       // Hex integers with 0x prefix
///     TypeMode: TypeReprMode.AlwaysShow
/// );
/// 
/// // IEEE 754 analysis
/// var ieeeConfig = new ReprConfig(
///     FloatFormatString: "BF",    // Bit field representation
///     IntFormatString: "B",       // Binary representation
///     UseSimpleFormatsInContainers: false  // Keep detailed formatting everywhere
/// );
/// 
/// // Member visibility examples
/// var publicOnlyConfig = new ReprConfig(
///     ViewMode: MemberReprMode.PublicFieldAutoProperty  // Default - public fields and auto-properties
/// );
/// 
/// var detailedConfig = new ReprConfig(
///     ViewMode: MemberReprMode.Everything,  // All accessible members
///     TypeMode: TypeReprMode.AlwaysShow
/// );
/// 
/// // Tree output for structured analysis
/// var treeConfig = new ReprConfig(
///     EnablePrettyPrintForReprTree: true,
///     UseSimpleFormatsInContainers: true
/// );
/// </code>
/// </example>
public record ReprConfig(
    string FloatFormatString = "EX",
    string IntFormatString = "D",
    TypeReprMode TypeMode = TypeReprMode.HideObvious,
    MemberReprMode ViewMode = MemberReprMode.PublicFieldAutoProperty,
    bool UseSimpleFormatsInContainers = true,
    bool HideChildTypes = false,
    int MaxDepth = 5,
    int MaxItemsPerContainer = 50,
    int MaxStringLength = 120,
    int MaxMemberTimeMs = 1,
    bool EnablePrettyPrintForReprTree = false,
    CultureInfo? Culture = null
);