using System.Runtime.CompilerServices;
using DebugUtils;
using DebugUtils.Formatters;

namespace DebugUtils;
public static partial class ReprExtensions
{
    public static string Repr<T>(this T obj, ReprConfig? reprConfig = null,
        HashSet<int>? visited = null)
    {
        reprConfig ??= ReprConfig.GlobalDefaults;
        visited ??= new HashSet<int>();

        // 1. Handle Nullable<T> as a special case first.
        if (obj.IsNullableStruct())
        {
            return FormatNullableValueType(obj, reprConfig);
        }

        if (obj is null) return "null";

        // 2. Handle circular references for reference types.
        if (!obj.GetType()
                .IsValueType)
        {
            var id = RuntimeHelpers.GetHashCode(obj);
            if (!visited.Add(id)) return $"<circular @{id:X8}>";
        }

        // 3. Get the correct formatter from the registry.
        var formatter = ReprFormatterRegistry.GetFormatter(obj.GetType());

        string result;
        // 4. Call the formatter with the correct arguments.
        if (formatter is { } reprFormatter)
        {
            result = reprFormatter.ToRepr(obj, reprConfig, visited);
        }
        else
        {
            result = obj.ToString() ?? "";
        }

        // 5. Cleanup and apply type prefix.
        try
        {
            return reprConfig.TypeMode switch
            {
                TypeReprMode.AlwaysHide => result,
                TypeReprMode.HideObvious => obj.NeedsTypePrefix()
                    ? $"{obj.GetReprTypeName()}({result})"
                    : result,
                _ => $"{obj.GetReprTypeName()}({result})"
            };
        }
        finally
        {
            if (!obj.GetType()
                    .IsValueType) visited.Remove(RuntimeHelpers.GetHashCode(obj));
        }
    }

    // This method remains as it is, correctly handling the logic for Nullable<T>.
    private static string FormatNullableValueType<T>(this T nullable, ReprConfig reprConfig)
    {
        var type = typeof(T);
        var underlyingType = System.Nullable.GetUnderlyingType(type)!;
        var reprName = underlyingType.GetReprTypeNameByTypeName();

        if (nullable == null)
        {
            return $"{reprName}?(null)";
        }

        var value = type.GetProperty("Value")!.GetValue(nullable)!;
        return
            $"{reprName}?({value.Repr(reprConfig with { TypeMode = TypeReprMode.AlwaysHide })})";
    }
}