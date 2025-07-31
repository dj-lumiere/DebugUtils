using System.Runtime.CompilerServices;
using DebugUtils.Repr.Formatters;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeLibraries;

namespace DebugUtils.Repr;

public static partial class ReprExtensions
{
    public static string Repr<T>(this T obj, ReprConfig? config = null,
        HashSet<int>? visited = null)
    {
        config ??= ReprConfig.GlobalDefaults;
        visited ??= new HashSet<int>();

        // 1. Handle Nullable<T> as a special case first.
        if (obj.IsNullableStruct())
        {
            return FormatNullableValueType(nullable: obj, config: config);
        }

        if (obj is null)
        {
            return "null";
        }

        // 2. Handle circular references for reference types.
        if (!obj.GetType()
                .IsValueType)
        {
            var id = RuntimeHelpers.GetHashCode(o: obj);
            if (!visited.Add(item: id))
            {
                return $"<circular @{id:X8}>";
            }
        }

        // 3. Get the correct formatter from the registry.
        var formatter = ReprFormatterRegistry.GetFormatter(type: obj.GetType());

        string result;
        // 4. Call the formatter with the correct arguments.
        if (formatter is { } reprFormatter)
        {
            result = reprFormatter.ToRepr(obj: obj, config: config, visited: visited);
        }
        else
        {
            result = obj.ToString() ?? "";
        }

        // 5. Cleanup and apply type prefix.
        try
        {
            return config.TypeMode switch
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
                    .IsValueType)
            {
                visited.Remove(item: RuntimeHelpers.GetHashCode(o: obj));
            }
        }
    }

    // This method remains as it is, correctly handling the logic for Nullable<T>.
    private static string FormatNullableValueType<T>(this T nullable, ReprConfig config)
    {
        var type = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(nullableType: type)!;
        var reprName = underlyingType.GetReprTypeNameByTypeName();

        if (nullable == null)
        {
            return $"{reprName}?(null)";
        }

        var value = type.GetProperty(name: "Value")!.GetValue(obj: nullable)!;
        return
            $"{reprName}?({value.Repr(config: config with { TypeMode = TypeReprMode.AlwaysHide })})";
    }
}