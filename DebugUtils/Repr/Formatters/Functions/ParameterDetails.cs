using System.Reflection;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Formatters.Fallback;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Functions;

internal class ParameterDetails
{
    public string Name { get; init; }
    public string TypeReprName { get; init; }
    public string Modifier { get; init; }
    public bool HasDefaultValue { get; init; }
    public object? DefaultValue { get; init; }

    public ParameterDetails(ParameterInfo parameterInfo)
    {
        Name = parameterInfo.Name ?? "unnamed";
        TypeReprName = parameterInfo.ParameterType.GetReprTypeName();
        Modifier = parameterInfo.GetParameterModifier();
        HasDefaultValue = parameterInfo.HasDefaultValue;
        DefaultValue = parameterInfo.HasDefaultValue
            ? parameterInfo.DefaultValue
            : null;
    }

    public override string ToString()
    {
        List<string> parts = new();
        if (!String.IsNullOrEmpty(value: Modifier))
        {
            parts.Add(item: Modifier);
        }

        if (!String.IsNullOrEmpty(value: TypeReprName))
        {
            parts.Add(item: TypeReprName);
        }

        if (!String.IsNullOrEmpty(value: Name))
        {
            parts.Add(item: Name);
        }

        return String.Join(separator: " ", values: parts);
    }
}

internal static class ParameterDetailsExtensions
{
    public static ParameterDetails ToParameterDetails(this ParameterInfo parameterInfo)
    {
        return new ParameterDetails(parameterInfo: parameterInfo);
    }
    public static JsonObject ToJsonObject(this ParameterDetails details, ReprConfig config,
        HashSet<int> visited, int depth)
    {
        var result = new JsonObject();
        result.Add(propertyName: "name",
            value: details.Name.ToJsonObject(config: config, visited: visited, depth: depth + 1));
        result.Add(propertyName: "typeReprName",
            value: details.TypeReprName.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "Modifier",
            value: details.Modifier.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "HasDefaultValue",
            value: details.HasDefaultValue.ToJsonObject(config: config, visited: visited,
                depth: depth + 1));
        result.Add(propertyName: "DefaultValue",
            value: details.DefaultValue?.ToJsonObject(config: config, visited: visited,
                depth: depth + 1) ?? null);
        return result;
    }
    public static string GetParameterModifier(this ParameterInfo param)
    {
        // Check for ref/out/in modifiers
        if (param.ParameterType.IsByRef)
        {
            if (param.IsOut)
            {
                return "out";
            }

            if (param.IsIn)
            {
                return "in";
            }

            return "ref"; // Default for ByRef that's not in/out
        }

        // Check for params array
        if (param.IsDefined(attributeType: typeof(ParamArrayAttribute)))
        {
            return "params";
        }

        return ""; // No modifier
    }
}