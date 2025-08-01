using DebugUtils.Repr.TypeLibraries;

namespace DebugUtils.Repr;

public static partial class ReprExtensions
{
    private static string GetArrayTypeName(this Array array)
    {
        var rank = array.Rank;
        if (rank != 1)
        {
            return $"{rank}DArray";
        }

        if (array.GetType()
                 .GetElementType()
                ?.IsArray ?? false)
        {
            return "JaggedArray";
        }

        return "1DArray";
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

    public static string GetReprTypeName<T>(this T obj)
    {
        var type = obj?.GetType() ?? typeof(T);

        if (CSharpTypeNames.TryGetValue(key: type, value: out var typeName))
        {
            return typeName;
        }

        if (obj is Array array)
        {
            return $"{array.GetArrayTypeName()}";
        }

        var result = type.Name;
        if (result.Contains(value: '`'))
        {
            result = result.Split(separator: '`')[0];
        }

        return result;
    }
    public static string GetReprTypeNameByTypeName(this Type type)
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
            return type.ProcessRefType();
        }

        var isTaskType = type == typeof(Task);
        var isGenericTaskType =
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);
        var isValueTask = type == typeof(ValueTask);
        var isGenericValueTask = type.IsGenericType &&
                                 type.GetGenericTypeDefinition() == typeof(ValueTask<>);

        if (isTaskType || isGenericTaskType || isValueTask || isGenericValueTask)
        {
            return type.ProcessTaskType();
        }

        var result = type.Name;
        if (result.Contains(value: '`'))
        {
            result = result.Split(separator: '`')[0];
        }

        return result;
    }

    public static string ProcessRefType(this Type type)
    {
        var innerType = type?.GetElementType() ?? null;
        return $"ref {innerType?.GetReprTypeNameByTypeName() ?? "null"}";
    }

    public static string ProcessTaskType(this Type type)
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
                var innerTypeReprName = innerType.GetReprTypeNameByTypeName();
                return $"{genericDef.Name.Split(separator: '`')[0]}<{innerTypeReprName}>";
            }
        }

        return type.Name;
    }
}