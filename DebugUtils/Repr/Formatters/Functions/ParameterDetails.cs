using System.Reflection;

namespace DebugUtils.Repr.Formatters.Functions;

public class ParameterDetails
{
    public string Name { get; init; }
    public string TypeReprName { get; init; }
    public string Modifier { get; init; }
    public bool HasDefaultValue { get; init; }
    public object? DefaultValue { get; init; }

    public ParameterDetails(ParameterInfo parameterInfo)
    {
        Name = parameterInfo.Name ?? "unnamed";
        TypeReprName = parameterInfo.ParameterType.GetReprTypeNameByTypeName();
        Modifier = parameterInfo.GetParameterModifier();
        HasDefaultValue = parameterInfo.HasDefaultValue;
        DefaultValue = parameterInfo.HasDefaultValue
            ? parameterInfo.DefaultValue
            : null;
    }

    public override string ToString()
    {
        List<string> parts = new();
        if (!string.IsNullOrEmpty(Modifier))
        {
            parts.Add(Modifier);
        }

        if (!string.IsNullOrEmpty(TypeReprName))
        {
            parts.Add(TypeReprName);
        }

        if (!string.IsNullOrEmpty(Name))
        {
            parts.Add(Name);
        }

        return string.Join(" ", parts);
    }
}

internal static class ParameterDetailsExtensions
{
    public static ParameterDetails ToParameterDetails(this ParameterInfo parameterInfo)
    {
        return new ParameterDetails(parameterInfo);
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