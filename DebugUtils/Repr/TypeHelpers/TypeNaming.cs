namespace DebugUtils.Repr.TypeHelpers;

/// <summary>
/// Provides utilities for generating human-readable type names and categorizing types
/// for the Repr system. This class handles the complex logic of converting .NET's
/// technical type names into clean, developer-friendly representations.
/// </summary>
/// <remarks>
/// <para>This class is the core of type name resolution in the Repr system, handling:</para>
/// <list type="bullet">
/// <item><description><strong>C# Type Aliases:</strong> Converting System.Int32 to "int"</description></item>
/// <item><description><strong>Generic Types:</strong> Simplifying List&lt;T&gt; to "List"</description></item>
/// <item><description><strong>Nullable Types:</strong> Adding "?" suffix for nullable value types</description></item>
/// <item><description><strong>Array Types:</strong> Categorizing as 1D, multidimensional, or jagged</description></item>
/// <item><description><strong>Task Types:</strong> Showing result types for async operations</description></item>
/// <item><description><strong>Anonymous Types:</strong> Providing consistent "Anonymous" labels</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Basic type name resolution
/// Console.WriteLine(typeof(int).GetReprTypeName());              // "int"
/// Console.WriteLine(typeof(List&lt;string&gt;).GetReprTypeName());     // "List"
/// Console.WriteLine(typeof(Dictionary&lt;int, string&gt;).GetReprTypeName()); // "Dictionary"
/// 
/// // Nullable type handling
/// Console.WriteLine(typeof(int?).GetReprTypeName());             // "int?"
/// Console.WriteLine(typeof(DateTime?).GetReprTypeName());        // "DateTime?"
/// 
/// // Array type categorization
/// Console.WriteLine(typeof(int[]).GetReprTypeName());            // "1DArray"
/// Console.WriteLine(typeof(int[,]).GetReprTypeName());           // "2DArray"
/// Console.WriteLine(typeof(int[][]).GetReprTypeName());          // "JaggedArray"
/// 
/// // Task type resolution
/// Console.WriteLine(typeof(Task&lt;bool&gt;).GetReprTypeName());        // "Task&lt;bool&gt;"
/// Console.WriteLine(typeof(ValueTask&lt;int&gt;).GetReprTypeName());    // "ValueTask&lt;int&gt;"
/// 
/// // Type kind categorization
/// Console.WriteLine(typeof(MyClass).GetTypeKind());              // "class"
/// Console.WriteLine(typeof(MyRecord).GetTypeKind());             // "record class"
/// Console.WriteLine(typeof(MyStruct).GetTypeKind());             // "struct"
/// Console.WriteLine(typeof(MyEnum).GetTypeKind());               // "enum"
/// </code>
/// </example>
public static class TypeNaming
{
    /// <summary>
    /// Gets a human-readable representation name for the specified type.
    /// Converts technical .NET type names into more readable formats suitable for debugging output.
    /// </summary>
    /// <param name="type">The type to get the representation name for.</param>
    /// <returns>
    /// A string representing the type in a human-readable format.
    /// </returns>
    /// <remarks>
    /// <para>This method handles several special cases:</para>
    /// <list type="bullet">
    /// <item><description>Nullable value types are displayed with a "?" suffix</description></item>
    /// <item><description>Generic types show clean parameter names</description></item>
    /// <item><description>Arrays show dimensional information</description></item>
    /// <item><description>Task types show their result types</description></item>
    /// <item><description>Anonymous types are labeled as "Anonymous"</description></item>
    /// <item><description>Reference types (ref parameters) show a "ref" prefix</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(typeof(int).GetReprTypeName());        // "int"
    /// Console.WriteLine(typeof(List&lt;string&gt;).GetReprTypeName()); // "List"
    /// Console.WriteLine(typeof(int?).GetReprTypeName());       // "int?"
    /// Console.WriteLine(typeof(Task&lt;bool&gt;).GetReprTypeName());   // "Task&lt;bool&gt;"
    /// </code>
    /// </example>
    public static string GetReprTypeName(this Type type)
    {
        // Handle nullable types
        if (type.IsNullableStructType())
        {
            var underlyingType = Nullable.GetUnderlyingType(nullableType: type)!;

            if (underlyingType.IsTupleType())
            {
                return "Tuple?"; // Simple approach
            }

            return $"{underlyingType.GetReprTypeName()}?";
        }

        if (TypeNameMappings.CSharpTypeNames.TryGetValue(key: type, value: out var typeName))
        {
            return typeName;
        }

        if (TypeNameMappings.FriendlyTypeNames.TryGetValue(key: type,
                value: out var friendlyTypeName))
        {
            return friendlyTypeName;
        }

        if (type.IsArray)
        {
            return $"{type.GetArrayTypeNameByTypeName()}";
        }

        var isRefType = type.IsByRef;

        if (isRefType)
        {
            return type.GetRefTypeReprName();
        }

        var isTaskType = type == typeof(Task);
        var isGenericTaskType =
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);
        var isValueTask = type == typeof(ValueTask);
        var isGenericValueTask = type.IsGenericType &&
                                 type.GetGenericTypeDefinition() == typeof(ValueTask<>);

        if (isTaskType || isGenericTaskType || isValueTask || isGenericValueTask)
        {
            return type.GetTaskTypeReprName();
        }

        if (type.IsAnonymousType())
        {
            return "Anonymous";
        }

        var result = type.Name;
        if (result.Contains(value: '`'))
        {
            result = result.Split(separator: '`')[0];
        }

        return result;
    }
    /// <summary>
    /// Gets a human-readable representation name for the type of the specified object.
    /// This is a convenience method that extracts the runtime type from the object.
    /// </summary>
    /// <typeparam name="T">The compile-time type of the object.</typeparam>
    /// <param name="obj">The object whose type name should be retrieved. Can be null.</param>
    /// <returns>
    /// The human-readable type name. If the object is null, returns the name of the 
    /// compile-time type T.
    /// </returns>
    /// <remarks>
    /// This method uses the runtime type of the object when available, allowing for 
    /// accurate representation of polymorphic objects.
    /// </remarks>
    /// <example>
    /// <code>
    /// object stringObj = "hello";
    /// Console.WriteLine(stringObj.GetReprTypeName()); // "string" (runtime type)
    /// 
    /// string nullString = null;
    /// Console.WriteLine(nullString.GetReprTypeName()); // "string" (compile-time type)
    /// </code>
    /// </example>
    public static string GetReprTypeName<T>(this T obj)
    {
        var type = obj?.GetType() ?? typeof(T);
        return type.GetReprTypeName();
    }

    /// <summary>
    /// Gets the kind/category of the specified type (class, struct, record, enum, etc.).
    /// This determines the fundamental nature of the type for debugging purposes.
    /// </summary>
    /// <param name="type">The type to categorize.</param>
    /// <returns>
    /// A string describing the type kind: "enum", "interface", "record struct", "struct", 
    /// "record class", "class", or "unknown" for unrecognized types.
    /// </returns>
    public static string GetTypeKind(this Type type)
    {
        return type switch
        {
            _ when type.IsEnum => "enum",
            _ when type.IsInterface => "interface",
            _ when type.IsValueType && type.IsRecordType() => "record struct",
            _ when type.IsValueType => "struct",
            _ when type.IsRecordType() => "record class",
            _ when type.IsClass => "class",
            _ => "unknown"
        };
    }

    private static string GetArrayTypeNameByTypeName(this Type type)
    {
        if (!type.IsArray)
        {
            return "";
        }

        var rank = type.GetArrayRank();
        if (rank != 1)
        {
            return $"{rank}DArray";
        }

        if (type.GetElementType()
               ?.IsArray ?? false)
        {
            return "JaggedArray";
        }

        return "1DArray";
    }
    private static string GetRefTypeReprName(this Type type)
    {
        var innerType = type.GetElementType() ?? null;
        return $"ref {innerType?.GetReprTypeName() ?? "null"}";
    }
    private static string GetTaskTypeReprName(this Type type)
    {
        // For Task (non-generic)
        if (type == typeof(Task) || type == typeof(ValueTask))
        {
            return type.Name; // "Task" or "ValueTask"
        }

        // For Task<T> or ValueTask<T>
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            if (genericDef == typeof(Task<>) || genericDef == typeof(ValueTask<>))
            {
                var innerType = type.GetGenericArguments()[0]; // ✅ Use this instead!
                var innerTypeReprName = innerType.GetReprTypeName();
                return $"{genericDef.Name.Split(separator: '`')[0]}<{innerTypeReprName}>";
            }
        }

        return type.Name;
    }
}