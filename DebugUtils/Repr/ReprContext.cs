namespace DebugUtils.Repr;

/// <summary>
/// Represents the context for managing the representation process within the system.
/// </summary>
public record ReprContext
{
    /// <summary>
    /// Represents the configuration settings for the ReprContext.
    /// </summary>
    /// <remarks>
    /// Config provides initialization options for the ReprContext enabling custom behaviors or functionalities.
    /// </remarks>
    public ReprConfig Config { get; } = ReprConfig.GlobalDefaults;

    /// <summary>
    /// Tracks the set of visited object identifiers during the representation process.
    /// </summary>
    /// <remarks>
    /// The <c>Visited</c> property is used to prevent infinite loops while processing object relationships by
    /// maintaining a collection of identifiers for objects that have already been processed.
    /// </remarks>
    public HashSet<int> Visited { get; } = new();

    /// <summary>
    /// Tracks the depth of recursive calls during the representation process.
    /// </summary>
    /// <remarks>
    /// The <c>Depth</c> property is used to prevent infinite loops while processing object relationships by
    /// maintaining depth 
    /// </remarks>
    public int Depth { get; }

    /// <summary>
    /// Initializes a new instance of the ReprContext class with the specified configuration.
    /// Creates a new context with fresh state suitable for starting a new representation operation.
    /// </summary>
    /// <param name="config">
    /// The configuration to use for this representation context. Must not be null.
    /// Contains settings for formatting behavior, limits, and output preferences.
    /// </param>
    /// <remarks>
    /// This constructor creates a new context with:
    /// <list type="bullet">
    /// <item><description>Empty visited set for circular reference detection</description></item>
    /// <item><description>Zero initial depth</description></item>
    /// <item><description>The provided configuration</description></item>
    /// </list>
    /// Use this constructor when starting a new representation operation from a public entry point.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    /// <example>
    /// <code>
    /// var config = new ReprConfig(FloatMode: FloatReprMode.Exact);
    /// var context = new ReprContext(config);
    /// var result = obj.Repr(context);
    /// </code>
    /// </example>
    public ReprContext(ReprConfig config)
    {
        Config = config;
    }

    /// <summary>
    /// Initializes a new instance of the ReprContext class with full control over all context properties.
    /// This constructor provides complete flexibility for creating contexts with specific state.
    /// </summary>
    /// <param name="config">
    /// Optional configuration controlling formatting behavior. If null, uses ReprConfig.GlobalDefaults.
    /// Contains settings for numeric formatting, type display, limits, and other formatting options.
    /// </param>
    /// <param name="visited">
    /// Optional set of object hash codes currently being processed. If null, creates a new empty set.
    /// Used for circular reference detection - should be shared across related contexts in the same operation.
    /// </param>
    /// <param name="depth">
    /// The current recursion depth in the representation process. Defaults to 0.
    /// Used to prevent stack overflow on deeply nested object structures by limiting traversal depth.
    /// </param>
    /// <remarks>
    /// <para>This constructor is primarily used internally when creating derived contexts that need to:</para>
    /// <list type="bullet">
    /// <item><description>Share circular reference tracking state (same visited set)</description></item>
    /// <item><description>Maintain or modify recursion depth</description></item>
    /// <item><description>Apply different configuration while preserving state</description></item>
    /// </list>
    /// <para>The visited set should be the same instance across all related contexts in a single
    /// representation operation to ensure proper circular reference detection.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating a derived context with incremented depth
    /// var childContext = new ReprContext(
    ///     config: parentContext.Config,
    ///     visited: parentContext.Visited,  // Share the same set!
    ///     depth: parentContext.Depth + 1
    /// );
    /// 
    /// // Creating a fresh context with custom config
    /// var freshContext = new ReprContext(
    ///     config: myConfig,
    ///     visited: null,  // Will create new empty set
    ///     depth: 0        // Start at root level
    /// );
    /// </code>
    /// </example>
    public ReprContext(ReprConfig? config = null, HashSet<int>? visited = null, int depth = 0)
    {
        Config = config ?? ReprConfig.GlobalDefaults;
        Visited = visited ?? new HashSet<int>();
        Depth = depth;
    }


    /// <summary>
    /// Creates a new context with incremented depth for processing nested objects.
    /// This method is essential for preventing infinite recursion and controlling
    /// the maximum depth of object traversal during representation.
    /// </summary>
    /// <returns>
    /// A new ReprContext instance with depth increased by 1, sharing the same configuration
    /// and visited set as the current context.
    /// </returns>
    /// <remarks>
    /// <para>This method serves several critical purposes:</para>
    /// <list type="bullet">
    /// <item><description><strong>Stack Overflow Prevention:</strong> Tracks recursion depth to prevent infinite loops in deeply nested objects</description></item>
    /// <item><description><strong>Performance Control:</strong> Enables depth-based truncation when MaxDepth is exceeded</description></item>
    /// <item><description><strong>Readable Output:</strong> Prevents overwhelming output from very deep object hierarchies</description></item>
    /// <item><description><strong>Memory Management:</strong> Maintains consistent configuration and circular reference tracking</description></item>
    /// </list>
    /// <para>Formatters should call this method when processing child objects or collection elements
    /// to ensure proper depth tracking throughout the representation process.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In a custom formatter:
    /// public string ToRepr(object obj, ReprContext context)
    /// {
    ///     var person = (Person)obj;
    ///     var parts = new List&lt;string&gt;();
    ///     
    ///     // ✅ Always increment depth when processing child objects
    ///     parts.Add($"Name: {person.Name.Repr(context.WithIncrementedDepth())}");
    ///     parts.Add($"Manager: {person.Manager?.Repr(context.WithIncrementedDepth()) ?? "null"}");
    ///     
    ///     return string.Join(", ", parts);
    /// }
    /// 
    /// // The depth tracking prevents issues like:
    /// // Manager → Manager → Manager → ... (infinite recursion)
    /// // Instead produces: Manager → Manager → &lt;Max Depth Reached&gt;
    /// </code>
    /// </example>
    public ReprContext WithIncrementedDepth()
    {
        return new ReprContext(
            config: Config,
            visited: Visited, // Share the same visited set
            depth: Depth + 1
        );
    }


    internal ReprContext WithTypeHide()
    {
        return new ReprContext(
            config: Config with
            {
                TypeMode = TypeReprMode.AlwaysHide
            },
            visited: Visited, // Share the same visited set
            depth: Depth
        );
    }

    internal ReprContext WithContainerConfig()
    {
        return new ReprContext(
            config: GetContainerConfig(), visited: Visited, depth: Depth);
    }

    private ReprConfig GetContainerConfig()
    {
        return Config.ContainerReprMode switch
        {
            ContainerReprMode.UseParentConfig => Config,
            ContainerReprMode.UseSimpleFormats => ReprConfig.ContainerDefaults with
            {
                MaxDepth = Config.MaxDepth,
                MaxPropertiesPerObject = Config.MaxPropertiesPerObject,
                MaxElementsPerCollection = Config.MaxElementsPerCollection,
                ShowNonPublicProperties = Config.ShowNonPublicProperties,
                EnablePrettyPrintForReprTree = Config.EnablePrettyPrintForReprTree
            },
            ContainerReprMode.UseCustomConfig => Config.CustomContainerConfig ??
                                                 ReprConfig.ContainerDefaults,
            _ => ReprConfig.GlobalDefaults
        };
    }
}