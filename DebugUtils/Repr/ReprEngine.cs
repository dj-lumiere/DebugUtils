using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr;

internal static class ReprEngine
{
    public static string ToRepr<T>(this T obj, ReprContext? context = null)
    {
        context ??= new ReprContext();

        // Handle ALL edge cases in the main entry point
        if (obj.IsNullableStruct())
        {
            return obj.FormatNullableValueType(context: context);
        }

        if (obj is null)
        {
            return "null";
        }

        // Handle circular references HERE
        var id = RuntimeHelpers.GetHashCode(o: obj);
        if (!obj.GetType()
                .IsValueType)
        {
            if (!context.Visited.Add(item: id))
            {
                return $"<Circular Reference to {obj.GetReprTypeName()} @0x{id:X8}>";
            }
        }

        try
        {
            // Get formatter and format - no safety concerns needed in formatters!
            var type = obj.GetType();
            var formatter = type.GetStandardFormatter();
            var result = formatter.ToRepr(obj: obj, context: context);
            var haveTypeSuffix =
                TypeNameMappings.TypeSuffixNames.TryGetValue(key: type, value: out var suffix);
            var needsTypePrefix = obj.NeedsTypePrefix();

            // Apply type prefix
            return (context.Config.TypeMode, haveTypeSuffix, needsTypePrefix) switch
            {
                (_, true, true) => throw new InvalidOperationException(
                    message: "Should not be possible"),
                (_, true, false) => $"{result}_{suffix}",
                (TypeReprMode.AlwaysHide, _, _) => result,
                (TypeReprMode.HideObvious, false, true) => $"{obj.GetReprTypeName()}({result})",
                (TypeReprMode.HideObvious, false, false) => $"{result}",
                _ => $"{obj.GetReprTypeName()}({result})"
            };
        }
        finally
        {
            // Cleanup circular reference tracking
            if (!obj.GetType()
                    .IsValueType)
            {
                context.Visited.Remove(item: id);
            }
        }
    }
    private static string FormatNullableValueType<T>(this T nullable, ReprContext context)
    {
        var type = typeof(T);
        var reprName = type.GetReprTypeName();
        var haveTypeSuffix =
            TypeNameMappings.TypeSuffixNames.TryGetValue(key: type, value: out var suffix);
        string result;
        if (nullable == null)
        {
            if (haveTypeSuffix)
            {
                result = "null_" + suffix;
            }
            else
            {
                result = "null";
            }
        }
        else
        {
            var value = type.GetProperty(name: "Value")!.GetValue(obj: nullable)!;
            result = value.Repr(context: context.WithTypeHide()) + "?";
        }

        return (context.Config.TypeMode, haveTypeSuffix) switch
        {
            (_, true) => $"{result}",
            (TypeReprMode.AlwaysHide, _) => result,
            _ => $"{reprName}({result})"
        };

    }

    public static JsonNode ToReprTree<T>(this T obj, ReprContext? context = null)
    {
        context ??= new ReprContext();

        if (obj.IsNullableStruct())
        {
            return obj.FormatNullableAsHierarchical(context: context);
        }

        if (obj is null && context.Depth == 0)
        {
            return CreateNullObjectJson<T>();
        }

        if (obj is null)
        {
            return null!;
        }

        // ✅ Add circular reference detection!
        var id = RuntimeHelpers.GetHashCode(o: obj);
        if (!obj.GetType()
                .IsValueType)
        {
            if (!context.Visited.Add(item: id))
            {
                return obj.CreateCircularRefJson(id: id);
            }
        }

        try
        {
            var type = obj.GetType();
            var formatter = type.GetTreeFormatter();
            return formatter.ToReprTree(obj: obj, context: context);
        }
        finally
        {
            // ✅ Cleanup circular reference tracking
            if (!obj.GetType()
                    .IsValueType)
            {
                context.Visited.Remove(item: id);
            }
        }
    }

    private static JsonObject CreateCircularRefJson(this object obj, int id)
    {
        return new JsonObject
        {
            [propertyName: "type"] = "CircularReference",
            [propertyName: "target"] = new JsonObject
            {
                [propertyName: "type"] = obj.GetReprTypeName(),
                [propertyName: "hashCode"] = $"0x{id:X8}"
            }
        };
    }

    private static JsonObject CreateNullObjectJson<T>()
    {
        return new JsonObject
        {
            [propertyName: "type"] = typeof(T).GetReprTypeName(),
            [propertyName: "kind"] = typeof(T).GetTypeKind(),
            [propertyName: "value"] = null
        };
    }

    public static JsonObject CreateMaxDepthReachedJson(this Type type, int depth)
    {
        return new JsonObject
        {
            [propertyName: "type"] = type.GetReprTypeName(),
            [propertyName: "kind"] = type.GetTypeKind(),
            [propertyName: "maxDepthReached"] = "true",
            [propertyName: "depth"] = depth
        };
    }

    private static JsonNode FormatNullableAsHierarchical<T>(this T nullable, ReprContext context)
    {
        var type = typeof(T);
        var reprName = type.GetReprTypeName();
        var typeKind = type.UnderlyingSystemType.GetTypeKind();
        var haveTypeSuffix =
            TypeNameMappings.TypeSuffixNames.TryGetValue(key: type, value: out var suffix);

        // Handle depth > 0 case like other formatters - show with suffix for numeric types
        if (context.Depth > 0 && haveTypeSuffix)
        {
            var stringRepr = nullable.FormatNullableValueType(context: context);
            return stringRepr!;
        }

        if (nullable is null)
        {
            // Handle depth > 0 case for null values with suffix
            if (context.Depth > 0 && haveTypeSuffix)
            {
                return nullable.FormatNullableValueType(context: context)!;
            }

            var nullJson = new JsonObject
            {
                [propertyName: "type"] = reprName,
                [propertyName: "kind"] = typeKind,
                [propertyName: "value"] = null
            };
            return nullJson;
        }

        var value = type.GetProperty(name: "Value")!.GetValue(obj: nullable)!;
        var formatter = value.GetType()
                             .GetTreeFormatter();
        var valueRepr = formatter.ToReprTree(obj: value, context: context);
        valueRepr[propertyName: "type"] = reprName;

        return valueRepr;
    }
}