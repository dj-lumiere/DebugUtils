namespace DebugUtils.Repr;

using System.Globalization;

/// <summary>
/// Fluent builder for creating <see cref="ReprConfig"/> instances with customized settings.
/// Provides a chainable API for configuring all aspects of object representation formatting.
/// </summary>
/// <remarks>
/// <para>The builder pattern allows for clean, readable configuration code:</para>
/// <code>
/// var config = ReprConfig.Configure()
///     .ShowAllTypes()
///     .FloatFormat("F2")
///     .MaxDepth(3)
///     .Build();
/// </code>
/// <para>Each method returns the builder instance, enabling method chaining.
/// Call <see cref="Build"/> to create the final immutable <see cref="ReprConfig"/> instance.</para>
/// </remarks>
public sealed class ReprConfigBuilder
{
    private string _floatFormatString = "EX";
    private string _intFormatString = "D";
    private TypeReprMode _typeMode = TypeReprMode.HideObvious;
    private MemberReprMode _viewMode = MemberReprMode.PublicFieldAutoProperty;
    private bool _useSimpleFormatsInContainers = true;
    private bool _hideChildTypes = false;
    private int _maxDepth = 5;
    private int _maxItemsPerContainer = 50;
    private int _maxStringLength = 120;
    private int _maxMemberTimeMs = 1;
    private bool _enablePrettyPrintForReprTree = false;
    private CultureInfo? _culture = null;

    // === Float Format ===

    /// <summary>
    /// Sets the format string for floating-point numbers (float, double, decimal, Half).
    /// </summary>
    /// <param name="format">The format string to use. Supports standard .NET numeric format strings
    /// plus special modes: "EX" (exact decimal), "HP" (hex power for IEEE 754 analysis).</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var config = ReprConfig.Configure()
    ///     .FloatFormat("F2")  // Fixed 2 decimal places
    ///     .Build();
    /// </code>
    /// </example>
    public ReprConfigBuilder FloatFormat(string format)
    {
        _floatFormatString = format;
        return this;
    }

    /// <summary>
    /// Configures exact decimal representation for floating-point numbers.
    /// Shows the precise decimal value without rounding errors.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>FloatFormat("EX")</c>. Useful for debugging floating-point precision issues.
    /// </remarks>
    public ReprConfigBuilder ExactFloats()
    {
        return FloatFormat(format: "EX");
    }

    /// <summary>
    /// Configures shortest possible representation for floating-point numbers.
    /// Uses general format with automatic precision selection.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>FloatFormat("G")</c>. Produces the most compact output.
    /// </remarks>
    public ReprConfigBuilder ShortestFloats()
    {
        return FloatFormat(format: "G");
    }

    /// <summary>
    /// Configures fixed decimal places for floating-point numbers.
    /// </summary>
    /// <param name="decimals">Number of decimal places to display. Default is 2.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var config = ReprConfig.Configure()
    ///     .FixedFloats(3)  // Shows 3 decimal places
    ///     .Build();
    /// </code>
    /// </example>
    public ReprConfigBuilder FixedFloats(int decimals = 2)
    {
        return FloatFormat(format: $"F{decimals}");
    }

    // === Int Format ===

    /// <summary>
    /// Sets the format string for integer numbers (byte, int, long, BigInteger, etc.).
    /// </summary>
    /// <param name="format">The format string to use. Supports standard formats ("D", "N", etc.)
    /// and custom formats: "X" (hex), "B" (binary), "O" (octal), "Q" (quaternary).</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var config = ReprConfig.Configure()
    ///     .IntFormat("X")  // Hexadecimal with 0x prefix
    ///     .Build();
    /// </code>
    /// </example>
    public ReprConfigBuilder IntFormat(string format)
    {
        _intFormatString = format;
        return this;
    }

    /// <summary>
    /// Configures standard decimal representation for integers.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>IntFormat("D")</c>. This is the default format.
    /// </remarks>
    public ReprConfigBuilder DecimalInts()
    {
        return IntFormat(format: "D");
    }

    /// <summary>
    /// Configures hexadecimal representation for integers with 0x prefix.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>IntFormat("X")</c>. Useful for debugging bit patterns and memory addresses.
    /// </remarks>
    public ReprConfigBuilder HexInts()
    {
        return IntFormat(format: "X");
    }

    // === Type Mode ===

    /// <summary>
    /// Sets when type prefixes should be displayed in object representations.
    /// </summary>
    /// <param name="mode">The type display mode to use.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ReprConfigBuilder TypeMode(TypeReprMode mode)
    {
        _typeMode = mode;
        return this;
    }

    /// <summary>
    /// Always displays type information for non-null values.
    /// Provides maximum type information for detailed debugging.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>TypeMode(TypeReprMode.AlwaysShow)</c>.
    /// </remarks>
    public ReprConfigBuilder ShowAllTypes()
    {
        return TypeMode(mode: TypeReprMode.AlwaysShow);
    }

    /// <summary>
    /// Hides type prefixes for obvious or commonly used types.
    /// Balances readability with useful type information.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>TypeMode(TypeReprMode.HideObvious)</c>. This is the default mode.
    /// </remarks>
    public ReprConfigBuilder HideObviousTypes()
    {
        return TypeMode(mode: TypeReprMode.HideObvious);
    }

    /// <summary>
    /// Never displays type prefixes, showing only content.
    /// Produces the most compact output.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>TypeMode(TypeReprMode.AlwaysHide)</c>.
    /// </remarks>
    public ReprConfigBuilder HideAllTypes()
    {
        return TypeMode(mode: TypeReprMode.AlwaysHide);
    }

    // === Member View Mode ===

    /// <summary>
    /// Sets which object members are included in representations.
    /// </summary>
    /// <param name="mode">The member visibility mode to use.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Be cautious with modes that access properties, as property getters can execute arbitrary code.
    /// </remarks>
    public ReprConfigBuilder ViewMode(MemberReprMode mode)
    {
        _viewMode = mode;
        return this;
    }

    /// <summary>
    /// Includes only public fields and auto-properties in representations.
    /// Safe default that avoids executing property getters.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>ViewMode(MemberReprMode.PublicFieldAutoProperty)</c>. This is the default mode.
    /// </remarks>
    public ReprConfigBuilder PublicOnly()
    {
        return ViewMode(mode: MemberReprMode.PublicFieldAutoProperty);
    }

    /// <summary>
    /// Includes all fields and auto-properties regardless of visibility.
    /// Shows private and internal members for comprehensive debugging.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>ViewMode(MemberReprMode.AllFieldAutoProperty)</c>.
    /// </remarks>
    public ReprConfigBuilder AllMembers()
    {
        return ViewMode(mode: MemberReprMode.AllFieldAutoProperty);
    }

    // === Container Behavior ===

    /// <summary>
    /// Sets whether to use simplified formatting within containers.
    /// </summary>
    /// <param name="use">True to use simplified formats ("G" for floats, "D" for ints) in containers;
    /// false to use parent formatting. Default is true.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Simplified formatting makes nested collections more readable by reducing precision
    /// and formatting complexity for container elements.
    /// </remarks>
    public ReprConfigBuilder UseSimpleFormatsInContainers(bool use = true)
    {
        _useSimpleFormatsInContainers = use;
        return this;
    }

    /// <summary>
    /// Uses simplified formatting within containers for better readability.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>UseSimpleFormatsInContainers(true)</c>. This is the default behavior.
    /// </remarks>
    public ReprConfigBuilder SimplifyContainers()
    {
        return UseSimpleFormatsInContainers(use: true);
    }

    /// <summary>
    /// Uses full detailed formatting within containers.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>UseSimpleFormatsInContainers(false)</c>.
    /// Useful when precise values in collections are important.
    /// </remarks>
    public ReprConfigBuilder DetailedContainers()
    {
        return UseSimpleFormatsInContainers(use: false);
    }

    /// <summary>
    /// Sets whether to hide type prefixes for child elements in containers.
    /// </summary>
    /// <param name="hide">True to hide obvious type prefixes in container contents;
    /// false to show types according to TypeMode. Default is true.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Only affects container contents, not top-level objects.
    /// Helps reduce visual clutter in nested structures.
    /// </remarks>
    public ReprConfigBuilder HideChildTypes(bool hide = true)
    {
        _hideChildTypes = hide;
        return this;
    }

    // === Limits ===

    /// <summary>
    /// Sets the maximum recursion depth for nested objects.
    /// </summary>
    /// <param name="depth">Maximum depth to traverse. Use -1 for unlimited depth.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Prevents stack overflow and controls output size for deeply nested structures.
    /// Objects beyond this depth show as "&lt;Max Depth Reached&gt;".
    /// </remarks>
    public ReprConfigBuilder MaxDepth(int depth)
    {
        _maxDepth = depth;
        return this;
    }

    /// <summary>
    /// Removes depth limiting, allowing unlimited recursion.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxDepth(-1)</c>. Use with caution on deeply nested structures.
    /// </remarks>
    public ReprConfigBuilder UnlimitedDepth()
    {
        return MaxDepth(depth: -1);
    }

    /// <summary>
    /// Sets shallow depth limit of 2 levels.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxDepth(2)</c>. Good for overview without too much detail.
    /// </remarks>
    public ReprConfigBuilder ShallowDepth()
    {
        return MaxDepth(depth: 2);
    }

    /// <summary>
    /// Sets deep depth limit of 10 levels.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxDepth(10)</c>. Suitable for detailed debugging.
    /// </remarks>
    public ReprConfigBuilder DeepDepth()
    {
        return MaxDepth(depth: 10);
    }

    /// <summary>
    /// Sets the maximum number of items to display per collection.
    /// </summary>
    /// <param name="count">Maximum items to show. Use -1 for unlimited items.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Remaining items are indicated with "... (N more items)".
    /// Helps manage output size for large collections.
    /// </remarks>
    public ReprConfigBuilder MaxItems(int count)
    {
        _maxItemsPerContainer = count;
        return this;
    }

    /// <summary>
    /// Removes item limiting for collections.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxItems(-1)</c>. Shows all items in collections.
    /// </remarks>
    public ReprConfigBuilder UnlimitedItems()
    {
        return MaxItems(count: -1);
    }

    /// <summary>
    /// Sets item limit to 10 per collection.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxItems(10)</c>. Good for compact output.
    /// </remarks>
    public ReprConfigBuilder FewItems()
    {
        return MaxItems(count: 10);
    }

    /// <summary>
    /// Sets item limit to 100 per collection.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxItems(100)</c>. Shows more detail while still limiting very large collections.
    /// </remarks>
    public ReprConfigBuilder ManyItems()
    {
        return MaxItems(count: 100);
    }

    /// <summary>
    /// Sets the maximum length for string representations.
    /// </summary>
    /// <param name="length">Maximum characters to display. Use -1 for unlimited length.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Truncated strings show as "prefix... (N more letters)".
    /// </remarks>
    public ReprConfigBuilder MaxStringLength(int length)
    {
        _maxStringLength = length;
        return this;
    }

    /// <summary>
    /// Sets string limit to 50 characters.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxStringLength(50)</c>. Good for compact output.
    /// </remarks>
    public ReprConfigBuilder ShortStrings()
    {
        return MaxStringLength(length: 50);
    }

    /// <summary>
    /// Sets string limit to 500 characters.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxStringLength(500)</c>. Shows more content while still limiting very long strings.
    /// </remarks>
    public ReprConfigBuilder LongStrings()
    {
        return MaxStringLength(length: 500);
    }

    /// <summary>
    /// Removes string length limiting.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxStringLength(-1)</c>. Shows complete strings regardless of length.
    /// </remarks>
    public ReprConfigBuilder UnlimitedStrings()
    {
        return MaxStringLength(length: -1);
    }

    /// <summary>
    /// Sets the maximum time allowed for property getter execution.
    /// </summary>
    /// <param name="milliseconds">Maximum milliseconds for getter execution.
    /// Use 0 to disable property access entirely.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Properties exceeding this limit show as "[Timed Out]".
    /// Only applies to non-auto properties; fields and auto-properties are always safe.
    /// </remarks>
    public ReprConfigBuilder MaxMemberTime(int milliseconds)
    {
        _maxMemberTimeMs = milliseconds;
        return this;
    }

    /// <summary>
    /// Sets strict 1ms timeout for property getters.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxMemberTime(1)</c>. Default setting that provides safety
    /// while allowing most normal getters to complete.
    /// </remarks>
    public ReprConfigBuilder FastOnly()
    {
        return MaxMemberTime(milliseconds: 1);
    }

    /// <summary>
    /// Sets generous 100ms timeout for property getters.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>MaxMemberTime(100)</c>. Allows slower properties to complete
    /// but increases risk of blocking.
    /// </remarks>
    public ReprConfigBuilder AllowSlowMembers()
    {
        return MaxMemberTime(milliseconds: 100);
    }

    // === Output Format ===

    /// <summary>
    /// Sets whether to format ReprTree output with indentation and line breaks.
    /// </summary>
    /// <param name="enable">True to enable pretty printing; false for compact output.
    /// Default is true.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Pretty printing significantly increases output size but improves human readability
    /// for JSON tree representations.
    /// </remarks>
    public ReprConfigBuilder EnablePrettyPrint(bool enable = true)
    {
        _enablePrettyPrintForReprTree = enable;
        return this;
    }

    /// <summary>
    /// Enables pretty printing for ReprTree output.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>EnablePrettyPrint(true)</c>. Produces formatted JSON with indentation.
    /// </remarks>
    public ReprConfigBuilder PrettyPrint()
    {
        return EnablePrettyPrint(enable: true);
    }

    /// <summary>
    /// Disables pretty printing for compact ReprTree output.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>EnablePrettyPrint(false)</c>. Produces minified JSON without whitespace.
    /// </remarks>
    public ReprConfigBuilder CompactPrint()
    {
        return EnablePrettyPrint(enable: false);
    }

    /// <summary>
    /// Sets the culture for numeric formatting.
    /// </summary>
    /// <param name="culture">The culture to use. Null uses current thread culture.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Affects decimal separators and thousand separators in standard numeric formats.
    /// Does not affect special formats (EX, HP, B, Q, O, X) which use invariant formatting.
    /// </remarks>
    public ReprConfigBuilder Culture(CultureInfo? culture)
    {
        _culture = culture;
        return this;
    }

    /// <summary>
    /// Uses invariant culture for consistent formatting across locales.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>Culture(CultureInfo.InvariantCulture)</c>.
    /// Ensures consistent output regardless of system locale.
    /// </remarks>
    public ReprConfigBuilder InvariantCulture()
    {
        return Culture(culture: CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Uses current thread culture for locale-specific formatting.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <c>Culture(CultureInfo.CurrentCulture)</c>.
    /// Formats numbers according to user's locale preferences.
    /// </remarks>
    public ReprConfigBuilder CurrentCulture()
    {
        return Culture(culture: CultureInfo.CurrentCulture);
    }

    // === Preset Configurations ===

    /// <summary>
    /// Applies debug configuration preset with maximum detail.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// <para>Configures:</para>
    /// <list type="bullet">
    /// <item>Shows all types</item>
    /// <item>Includes all members</item>
    /// <item>Deep recursion (10 levels)</item>
    /// <item>Many items per container (100)</item>
    /// <item>Long strings (500 chars)</item>
    /// <item>Pretty printed output</item>
    /// <item>Detailed container formatting</item>
    /// </list>
    /// <para>Ideal for comprehensive debugging sessions.</para>
    /// </remarks>
    public ReprConfigBuilder Debug()
    {
        return ShowAllTypes()
              .AllMembers()
              .DeepDepth()
              .ManyItems()
              .LongStrings()
              .PrettyPrint()
              .DetailedContainers();
    }

    /// <summary>
    /// Applies production configuration preset with safety and performance.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// <para>Configures:</para>
    /// <list type="bullet">
    /// <item>Hides obvious types</item>
    /// <item>Public members only</item>
    /// <item>Shallow recursion (2 levels)</item>
    /// <item>Few items per container (10)</item>
    /// <item>Short strings (50 chars)</item>
    /// <item>Compact output</item>
    /// <item>Simplified container formatting</item>
    /// </list>
    /// <para>Suitable for logging and production environments.</para>
    /// </remarks>
    public ReprConfigBuilder Production()
    {
        return HideObviousTypes()
              .PublicOnly()
              .ShallowDepth()
              .FewItems()
              .ShortStrings()
              .CompactPrint()
              .SimplifyContainers();
    }

    /// <summary>
    /// Applies console output configuration preset with balanced settings.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// <para>Configures:</para>
    /// <list type="bullet">
    /// <item>Hides obvious types</item>
    /// <item>Public members only</item>
    /// <item>Moderate recursion (3 levels)</item>
    /// <item>Moderate items per container (20)</item>
    /// <item>Moderate strings (80 chars)</item>
    /// <item>Compact output</item>
    /// </list>
    /// <para>Optimized for console/terminal display with reasonable detail.</para>
    /// </remarks>
    public ReprConfigBuilder Console()
    {
        return HideObviousTypes()
              .PublicOnly()
              .MaxDepth(depth: 3)
              .MaxItems(count: 20)
              .MaxStringLength(length: 80)
              .CompactPrint();
    }

    // === Build ===

    /// <summary>
    /// Creates an immutable <see cref="ReprConfig"/> instance with the configured settings.
    /// </summary>
    /// <returns>A new <see cref="ReprConfig"/> instance with all configured settings applied.</returns>
    /// <remarks>
    /// This method should be called after all configuration methods to produce the final
    /// configuration object. The resulting <see cref="ReprConfig"/> is immutable and thread-safe.
    /// </remarks>
    /// <example>
    /// <code>
    /// var config = ReprConfig.Configure()
    ///     .ShowAllTypes()
    ///     .FloatFormat("F2")
    ///     .MaxDepth(5)
    ///     .Build();  // Creates the final ReprConfig
    /// 
    /// var result = myObject.Repr(config);
    /// </code>
    /// </example>
    public ReprConfig Build()
    {
        return new ReprConfig(FloatFormatString: _floatFormatString,
            IntFormatString: _intFormatString, TypeMode: _typeMode, ViewMode: _viewMode,
            UseSimpleFormatsInContainers: _useSimpleFormatsInContainers,
            HideChildTypes: _hideChildTypes, MaxDepth: _maxDepth,
            MaxItemsPerContainer: _maxItemsPerContainer, MaxStringLength: _maxStringLength,
            MaxMemberTimeMs: _maxMemberTimeMs,
            EnablePrettyPrintForReprTree: _enablePrettyPrintForReprTree, Culture: _culture);
    }
}