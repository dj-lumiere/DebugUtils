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
            return obj.FormatNullableValueType(context: context.WithTypeHide());
        }

        if (obj is null)
        {
            return "null";
        }

        // Handle circular references HERE - single place
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
            var formatter =
                ReprFormatterRegistry.GetStandardFormatter(type: obj.GetType(), context: context);

            var result = formatter.ToRepr(obj: obj, context: context);

            // Apply type prefix
            return context.Config.TypeMode switch
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

        if (nullable == null)
        {
            return $"{reprName}(null)";
        }

        var value = type.GetProperty(name: "Value")!.GetValue(obj: nullable)!;
        return
            $"{reprName}({value.Repr(context: context.WithTypeHide())})";
    }

    public static JsonNode ToReprTree<T>(this T obj, ReprContext? context = null)
    {
        context ??= new ReprContext();

        if (obj.IsNullableStruct())
        {
            return obj.FormatNullableAsHierarchical(context: context);
        }

        if (obj is null)
        {
            return CreateNullObjectJson<T>();
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
            var formatter =
                ReprFormatterRegistry.GetTreeFormatter(type: obj.GetType(), context: context);
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

        if (nullable == null)
        {
            var nullJson = new JsonObject
            {
                [propertyName: "type"] = reprName,
                [propertyName: "kind"] = type.UnderlyingSystemType.GetTypeKind(),
                [propertyName: "value"] = null
            };
            return nullJson;
        }

        var value = type.GetProperty(name: "Value")!.GetValue(obj: nullable)!;
        var formatter =
            ReprFormatterRegistry.GetTreeFormatter(type: value.GetType(), context: context);
        var valueRepr = formatter.ToReprTree(obj: value, context: context);
        valueRepr[propertyName: "type"] = reprName;

        return valueRepr;
    }
}