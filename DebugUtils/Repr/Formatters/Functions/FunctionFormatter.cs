using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters.Functions;

[ReprFormatter(typeof(Delegate))]
[ReprOptions(needsPrefix: false)]
internal class FunctionFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var del = (Delegate)obj;
        var functionDetails = del.Method.ToFunctionDetails();
        return functionDetails.ToString();
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var del = (Delegate)obj;
        var type = del.GetType();

        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = "Function",
                [propertyName: "kind"] = type.GetTypeKind(),
                [propertyName: "maxDepthReached"] = true,
                [propertyName: "depth"] = context.Depth
            };
        }

        var functionDetails = del.Method.ToFunctionDetails();
        var result = new JsonObject();
        result.Add(propertyName: "type", value: "Function");
        result.Add(propertyName: "hashCode", value: $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}");
        result.Add(propertyName: "properties",
            value: functionDetails.FormatAsJsonNode(context: context));
        return result;
    }
}

[ReprFormatter(typeof(FunctionDetails))]
[ReprOptions(needsPrefix: false)]
internal class FunctionDetailsFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var details = (FunctionDetails)obj;
        return details.ToString();
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var details = (FunctionDetails)obj;
        var type = details.GetType();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = type.GetReprTypeName(),
                [propertyName: "kind"] = type.GetTypeKind(),
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth
            };
        }

        var result = new JsonObject();
        result.Add(propertyName: "name",
            value: details.Name);
        result.Add(propertyName: "returnType",
            value: details.ReturnTypeReprName);
        result.Add(propertyName: "modifiers",
            value: details.Modifiers.FormatAsJsonNode(context: context.WithIncrementedDepth()));
        result.Add(propertyName: "parameters",
            value: details.Parameters.FormatAsJsonNode(context: context.WithIncrementedDepth()));
        return result;
    }
}

[ReprFormatter(typeof(MethodModifiers))]
[ReprOptions(needsPrefix: false)]
internal class MethodModifiersFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var modifiers = (MethodModifiers)obj;
        return modifiers.ToString();
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var methodModifiers = (MethodModifiers)obj;
        var type = methodModifiers.GetType();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = type.GetReprTypeName(),
                [propertyName: "kind"] = type.GetTypeKind(),
                [propertyName: "maxDepthReached"] = true,
                [propertyName: "depth"] = context.Depth
            };
        }

        var modifiers = new JsonArray();

        foreach (var (condition, name) in new[]
                 {
                     (methodModifiers.IsPublic, "public"),
                     (methodModifiers.IsPrivate, "private"),
                     (methodModifiers.IsProtected, "protected"),
                     (methodModifiers.IsInternal, "internal"),
                     (methodModifiers.IsStatic, "static"),
                     (methodModifiers.IsAbstract, "abstract"),
                     (methodModifiers.IsVirtual, "virtual"),
                     (methodModifiers.IsOverride, "override"),
                     (methodModifiers.IsSealed, "sealed"),
                     (methodModifiers.IsAsync, "async"),
                     (methodModifiers.IsExtern, "extern"),
                     (methodModifiers.IsUnsafe, "unsafe"),
                     (methodModifiers.IsGeneric, "generic")
                 })
        {
            if (condition)
            {
                modifiers.Add(value: name);
            }
        }

        return modifiers;
    }
}

[ReprFormatter(typeof(ParameterDetails))]
[ReprOptions(needsPrefix: false)]
internal class ParameterDetailsFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var details = (ParameterDetails)obj;
        return details.ToString();
    }

    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var details = (ParameterDetails)obj;
        var type = details.GetType();
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = type.GetReprTypeName(),
                [propertyName: "kind"] = type.GetTypeKind(),
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth
            };
        }

        var result = new JsonObject();
        result.Add(propertyName: "name",
            value: details.Name);
        result.Add(propertyName: "type",
            value: details.TypeReprName);
        result.Add(propertyName: "modifier",
            value: details.Modifier);
        result.Add(propertyName: "defaultValue",
            value: details.DefaultValue.FormatAsJsonNode(
                context: context.WithIncrementedDepth()));
        return result;
    }
}