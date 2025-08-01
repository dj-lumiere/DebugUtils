using DebugUtils.Repr.TypeLibraries;
using static DebugUtils.Repr.TypeLibraries.TypeNameMappings;

namespace DebugUtils.Repr;

/// <summary>
/// Provides extension methods for obtaining human-readable type names for representation purposes.
/// Handles complex scenarios including nullable types, generic types, arrays, and special .NET types.
/// </summary>
public static partial class ReprExtensions
{
    /// <summary>
    /// Gets a human-readable representation name for the specified type.
    /// Converts technical .NET type names into more readable formats suitable for debugging output.
    /// </summary>
    /// <param name="type">The type to get the representation name for.</param>
    /// <returns>
    /// A string representing the type in a human-readable format. Examples:
    /// - "int" instead of "Int32"
    /// - "List" instead of "List`1" 
    /// - "Task&lt;string&gt;" instead of "Task`1"
    /// - "int?" instead of "Nullable`1"
    /// </returns>
    /// <remarks>
    /// <para>This method handles several special cases:</para>
    /// <list type="bullet">
    /// <item><description>Nullable value types are displayed with "?" suffix</description></item>
    /// <item><description>Generic types show clean parameter names</description></item>
    /// <item><description>Arrays show dimensional information</description></item>
    /// <item><description>Task types show their result types</description></item>
    /// <item><description>Anonymous types are labeled as "Anonymous"</description></item>
    /// <item><description>Reference types (ref parameters) show "ref" prefix</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(typeof(int).GetReprTypeName());        // "int"
    /// Console.WriteLine(typeof(List&lt;string&gt;).GetReprTypeName()); // "List"
    /// Console.WriteLine(typeof(int?).GetReprTypeName());       // "int"
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
                return "Tuple"; // Simple approach
            }
        }

        if (CSharpTypeNames.TryGetValue(key: type, value: out var typeName))
        {
            return typeName;
        }

        if (FriendlyTypeNames.TryGetValue(key: type, value: out var friendlyTypeName))
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
        var innerType = type?.GetElementType() ?? null;
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