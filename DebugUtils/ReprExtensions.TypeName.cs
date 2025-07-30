using DebugUtils.Formatters;

namespace DebugUtils;

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
                ?
                .IsArray ?? false)
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

        if (CSharpTypeNames.TryGetValue(type, out var typeName))
        {
            return typeName;
        }

        if (obj is Array array)
        {
            return $"{array.GetArrayTypeName()}";
        }

        var result = type.Name;
        if (result.Contains('`'))
        {
            result = result.Split('`')[0];
        }

        return result;
    }
    public static string GetReprTypeNameByTypeName(this Type type)
    {
        // Handle nullable types
        if (type.IsNullableStructType())
        {
            var underlyingType = Nullable.GetUnderlyingType(type)!;

            if (underlyingType.IsTupleType())
            {
                return "Tuple"; // Simple approach
            }
        }

        if (CSharpTypeNames.TryGetValue(type, out var typeName))
        {
            return typeName;
        }

        if (FriendlyTypeNames.TryGetValue(type, out var friendlyTypeName))
        {
            return friendlyTypeName;
        }

        if (type.IsArray)
        {
            return $"{type.GetArrayTypeNameByTypeName()}";
        }

        var result = type.Name;
        if (result.Contains('`'))
        {
            result = result.Split('`')[0];
        }

        return result;
    }
}